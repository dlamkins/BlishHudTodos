﻿using System;
using System.Collections.Generic;
using System.IO;
using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Newtonsoft.Json;
using Todos.Source.Models;

namespace Todos.Source.Utils
{
    public class Persistence
    {
        private const string FILE_ENDING = ".todo.json";
        private static readonly Logger Logger = Logger.GetLogger<TodoModule>();
        
        private static readonly JsonSerializerSettings SETTINGS = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };
        
        private readonly string _directoryPath;
        
        public Persistence(DirectoriesManager manager)
        {
            _directoryPath = manager.GetFullDirectoryPath("todos");
        }

        public List<Todo> LoadAll()
        {
            var todos = new List<Todo>();
            foreach (var filePath in Directory.GetFiles(_directoryPath, $"*{FILE_ENDING}"))
            {
                Try(filePath, "deserialize", file =>
                {
                    var jsonString = File.ReadAllText(file);
                    var todo = JsonConvert.DeserializeObject<Todo>(jsonString, SETTINGS);
                    if (todo != null)
                        todos.Add(todo);
                });
            }
            return todos;
        }

        public void Add(Todo todo)
        {
            Try(GetFilePath(todo), "add", 
                filePath => File.WriteAllText(filePath, JsonConvert.SerializeObject(todo, SETTINGS)));
        }

        public void Save(Todo todo)
        {
            Try(GetFilePath(todo), "save", 
                filePath => File.WriteAllText(filePath, JsonConvert.SerializeObject(todo, SETTINGS)));
        }

        public void Delete(Todo todo)
        {
            Try(GetFilePath(todo), "delete", filePath =>
            {
                if (File.Exists(filePath)) 
                    File.Delete(filePath);
            });
        }
        
        private static void Try(string filePath, string operation, Action<string> action)
        {
            try { action(filePath); }
            catch (Exception e) { Logger.Error($"Could not {operation} file '{filePath}':\r\n{e.Message}"); }
        }
        
        private string GetFilePath(Todo todo)
        {
            return Path.Combine(_directoryPath, $"{todo.CreatedAt.Ticks.ToString()}{FILE_ENDING}");
        }
    }
}