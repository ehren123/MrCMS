using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Data;
using MrCMS.Entities;

namespace MrCMS.Tests.TestSupport
{
    public class InMemoryRepository<T> : IRepository<T> where T : SystemEntity
    {
        private readonly Dictionary<int, T> _store = new Dictionary<int, T>();
        private int _currentId = 1;
        public T Get(int id)
        {
            return _store.ContainsKey(id) ? _store[id] : null;
        }

        public TSubclass Get<TSubclass>(int id) where TSubclass : T
        {
            return Get(id) as TSubclass;
        }

        public void Add(T entity)
        {
            entity.Id = _currentId++;
            _store[entity.Id] = entity;
        }

        public void Delete(T entity)
        {
            _store.Remove(entity.Id);
        }

        public void DeleteAll()
        {
            _store.Clear();
        }

        public IQueryable<T> Query()
        {
            return _store.Values.AsQueryable();
        }

        public void Transact(Action<IRepository<T>> action)
        {
            action(this);
        }

        public IDisposable DisableSiteFilter()
        {
            return null;
        }

        public void Update(T entity)
        {
            if (!_store.ContainsKey(entity.Id))
                throw new Exception($"#{entity.Id} does not exist");
            _store[entity.Id] = entity;
        }
    }
}