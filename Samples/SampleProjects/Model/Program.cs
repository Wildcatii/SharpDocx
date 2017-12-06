﻿using System;
using System.IO;
using Model.Models;
using Model.Repositories;
using SharpDocx;

namespace Model
{
    internal class Program
    {
        private static readonly string BasePath = Path.GetDirectoryName(typeof(Program).Assembly.Location) + @"\..\..\..\..";

        private static void Main(string[] args)
        {
            var viewPath = $"{BasePath}\\Views\\Model.cs.docx";
            var documentPath = $"{BasePath}\\Documents\\Model.docx";

            var model = new DocumentViewModel
            {
                Title = "Model sample",
                Date = DateTime.Now.ToShortDateString(),
                Countries = CountryRepository.GetCountries()
            };
            
            //var document = DocumentFactory.Create(viewPath, model);
            //document.Generate(documentPath);

            Ide.Start(viewPath, documentPath, model);
        }
    }
}