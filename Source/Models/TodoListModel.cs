﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Modules.Managers;
using Todos.Source.Persistence;
using Todos.Source.Utils.Reactive;

namespace Todos.Source.Models
{
    public class TodoListModel : ModelBase
    {
        private readonly IVariable<List<TodoModel>> _allTodos; 
        
        public IProperty<IReadOnlyList<TodoModel>> AllTodos => _allTodos;
        public IProperty<IReadOnlyList<TodoModel>> VisibleTodos;

        public static async Task<TodoListModel> Initialize(DirectoriesManager manager)
        {
            var storedTodos = await new Persistence.Persistence(manager).LoadAll();
            return new TodoListModel(storedTodos.OrderBy(t => t.OrderIndex.Value).ToList());
        }

        private TodoListModel(List<TodoModel> sortedTodos)
        {
            _allTodos = Add(Variables.Transient(sortedTodos));
            VisibleTodos = Add(AllTodos.Select(all => all.Where(t => t.IsVisible.Value).ToList()));

            foreach (var todo in sortedTodos)
                SetupTodo(todo);
        }

        private void SetupTodo(TodoModel todo)
        {
            void SortAllTodos() => _allTodos.Value = AllTodos.Value.OrderBy(t => t.OrderIndex.Value).ToList();
            todo.OrderIndex.Subscribe(this, _ => SortAllTodos());
            todo.IsVisible.Subscribe(this, _ => SortAllTodos());
            todo.IsDeleted.Subscribe(this, _ => OnTodoDeleted(todo), false);
        }

        private void OnTodoDeleted(TodoModel todo)
        {
            TearDownTodo(todo);
            _allTodos.Value = AllTodos.Value.Where(t => t != todo).ToList();
        }

        private void TearDownTodo(TodoModel todo)
        {
            todo.IsDeleted.Unsubscribe(this);
            todo.OrderIndex.Unsubscribe(this);
            todo.IsVisible.Unsubscribe(this);
            todo.Dispose();
        }

        public void AddNewTodo()
        {
            var todo = new TodoModel(new TodoJson(), true);
            SetupTodo(todo);
            _allTodos.Value = AllTodos.Value.Append(todo).ToList();
        }

        public void MoveUp(TodoModel todo)
        {
            var todos = new List<TodoModel>(AllTodos.Value);
            var topIndex = todos.FindLastIndex(t => t.IsVisible.Value && t.OrderIndex.Value < todo.OrderIndex.Value);
            var bottomIndex = todos.IndexOf(todo);
            var newOrderIndexes = new Dictionary<TodoModel, long>();

            for (var i = topIndex; i < bottomIndex; i++)
                newOrderIndexes[todos[i]] = todos[i + 1].OrderIndex.Value;
            newOrderIndexes[todo] = todos[topIndex].OrderIndex.Value;

            foreach (var update in newOrderIndexes)
                update.Key.OrderIndex.Value = update.Value;
        }

        public void MoveDown(TodoModel todo)
        {
            var todos = new List<TodoModel>(AllTodos.Value);
            var topIndex = todos.IndexOf(todo);
            var bottomIndex = todos.FindIndex(t => t.IsVisible.Value && t.OrderIndex.Value > todo.OrderIndex.Value);
            var newOrderIndexes = new Dictionary<TodoModel, long>();

            for (var i = topIndex + 1; i <= bottomIndex; i++)
                newOrderIndexes[todos[i]] = todos[i - 1].OrderIndex.Value;
            newOrderIndexes[todo] = todos[bottomIndex].OrderIndex.Value;

            foreach (var update in newOrderIndexes)
                update.Key.OrderIndex.Value = update.Value;
        }

        protected override void DisposeModel()
        {
            foreach (var todo in AllTodos.Value)
                TearDownTodo(todo);
        }
    }
}