using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Caliburn.Micro;

namespace MangaScraper.UI.Core.Helpers {    
    public class ReactiveCollection<T> : IObservableCollection<T>, IObserver<List<T>> {
        private readonly BindableCollection<T> _collection;

        public ReactiveCollection(IObservable<List<T>> thing) {
            _collection = new BindableCollection<T>();
            thing.Subscribe(this);
        }

        public void OnCompleted() { }

        public void OnError(Exception error) {
            //todo
            throw error;
        }

        public void OnNext(List<T> value) {
            _collection.Clear();
            _collection.AddRange(value);
        }

        #region observableCollection       

        public T this[int index] {
            get => _collection[index];
            set => _collection[index] = value;
        }

        public int Count => _collection.Count;

        public bool IsReadOnly => ((IObservableCollection<T>) _collection).IsReadOnly;

        public bool IsNotifying {
            get => _collection.IsNotifying;
            set => _collection.IsNotifying = value;
        }

        public event PropertyChangedEventHandler PropertyChanged {
            add => ((IObservableCollection<T>) _collection).PropertyChanged += value;
            remove => ((IObservableCollection<T>) _collection).PropertyChanged -= value;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged {
            add => _collection.CollectionChanged += value;
            remove => _collection.CollectionChanged -= value;
        }

        public void Add(T item) => _collection.Add(item);

        public void AddRange(IEnumerable<T> items) => _collection.AddRange(items);

        public void Clear() => _collection.Clear();

        public bool Contains(T item) => _collection.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();

        public int IndexOf(T item) => _collection.IndexOf(item);

        public void Insert(int index, T item) => _collection.Insert(index, item);

        public void NotifyOfPropertyChange(string propertyName) => _collection.NotifyOfPropertyChange(propertyName);

        public void Refresh() => _collection.Refresh();

        public bool Remove(T item) => _collection.Remove(item);

        public void RemoveAt(int index) => _collection.RemoveAt(index);

        public void RemoveRange(IEnumerable<T> items) => _collection.RemoveRange(items);

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();

        #endregion
    }
}