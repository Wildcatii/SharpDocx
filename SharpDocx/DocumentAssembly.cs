﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SharpDocx.Extensions;

namespace SharpDocx
{
    internal class DocumentAssembly
    {
        private readonly Assembly assembly;
        private readonly string className;

        internal DocumentAssembly(
            string viewPath,
            Type baseClass,
            object model)
        {
            if (!File.Exists(viewPath))
            {
                throw new ArgumentException("Could not find the file " + viewPath, nameof(viewPath));
            }

            // Load base class assembly.
            var a = Assembly.LoadFrom(baseClass.Assembly.Location);
            if (a == null)
            {
                throw new ArgumentException("Can't load assembly '" + baseClass.Assembly + "'", nameof(baseClass));
            }

            // Get the base class type.
            var t = a.GetType(baseClass.FullName);
            if (t == null)
            {
                throw new ArgumentException("Can't find base class '" + baseClass.FullName + "' in assembly '" + baseClass.Assembly + "'", nameof(baseClass));
            }

            // Check base class type.
            if (t != typeof(DocumentBase) && !t.IsSubclassOf(typeof(DocumentBase)))
            {
                throw new ArgumentException("baseClass should be a BaseDocument derived type", nameof(baseClass));
            }

            // Get user defined using directives by calling the static BaseDocument.GetUsingDirectives method.
            var usingDirectives = (List<string>) a.Invoke(
                baseClass.FullName,
                null,
                "GetUsingDirectives",
                null);

            // Get user defined assemblies to reference.
            var referencedAssemblies = (List<string>) a.Invoke(
                baseClass.FullName,
                null,
                "GetReferencedAssemblies",
                null);

            if (model != null)
            {
                // Add namespace(s) of Model.
                if (usingDirectives == null)
                {
                    usingDirectives = new List<string>();
                }

                foreach (var type in GetTypes(model.GetType()))
                {
                    usingDirectives.Add($"using {type.Namespace};");
                }

                // Reference Model assembly/assemblies.
                if (referencedAssemblies == null)
                {
                    referencedAssemblies = new List<string>();
                }

                foreach (var type in GetTypes(model.GetType()))
                {
                    referencedAssemblies.Add(type.Assembly.Location);
                }
            }

            // Create a unique class name.
            this.className = $"SharpDocument_{Guid.NewGuid():N}";

            // Create an assembly for this class.
            this.assembly = DocumentCompiler.Compile(
                viewPath,
                this.className,
                baseClass.Name,
                model,
                usingDirectives,
                referencedAssemblies);
        }

        public object Instance()
        {
            return this.assembly.CreateInstance(DocumentCompiler.Namespace + "." + this.className, null);
        }

        private static IEnumerable<Type> GetTypes(Type type)
        {
#if !NET35
            if (type.IsConstructedGenericType)
            {
                foreach (var t in type.GenericTypeArguments)
                {
                    yield return t;
                }
            }
#endif
            yield return type;
        }
    }
}