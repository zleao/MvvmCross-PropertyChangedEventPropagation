using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using PropertyChangedEventPropagation.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PropertyChangedEventPropagation.Core.ViewModels
{
    public abstract class ViewModel : MvxViewModel, IDisposable
    {
        #region Dependency Managment

        /// <summary>
        /// Gets the property dependencies.
        /// </summary>
        private IDictionary<string, IList<PropertyInfo>> PropertyDependencies
        {
            get { return _dependencies; }
        }
        private readonly IDictionary<string, IList<PropertyInfo>> _dependencies;

        /// <summary>
        /// Gets the method dependencies.
        /// </summary>
        private IDictionary<string, IList<MethodInfo>> MethodDependencies
        {
            get { return _methodDependencies; }
        }
        private readonly IDictionary<string, IList<MethodInfo>> _methodDependencies;

        /// <summary>
        /// Gets the notifiable collections.
        /// </summary>
        private IDictionary<string, INotifyCollectionChanged> NotifiableCollections
        {
            get { return _notifiableCollections; }
        }
        private readonly IDictionary<string, INotifyCollectionChanged> _notifiableCollections;

        /// <summary>
        /// Initializes the property dependencies.
        /// </summary>
        private IDictionary<string, IList<PropertyInfo>> InitializePropertyDependencies()
        {
            var dependencies = new Dictionary<string, IList<PropertyInfo>>();
            lock (dependencies)
            {
                foreach (var property in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var attributes = property.GetCustomAttributes<DependsOnAttribute>(true);
                    foreach (var attribute in attributes)
                    {
                        if (!dependencies.ContainsKey(attribute.Name))
                            dependencies.Add(attribute.Name, new List<PropertyInfo>());
                        dependencies[attribute.Name].Add(property);
                    }
                }
            }
            return dependencies;
        }

        /// <summary>
        /// Initializes the method dependencies.
        /// </summary>
        private IDictionary<string, IList<MethodInfo>> InitializeMethodDependencies()
        {
            var methodDependencies = new Dictionary<string, IList<MethodInfo>>();

            lock (methodDependencies)
            {
                foreach (var method in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    if (!method.ReturnType.Equals(typeof(void)))
                        continue;
                    if (method.GetParameters().Length > 0)
                        continue;

                    var attributes = method.GetCustomAttributes<DependsOnAttribute>(true);
                    foreach (var attribute in attributes)
                    {
                        if (!methodDependencies.ContainsKey(attribute.Name))
                            methodDependencies.Add(attribute.Name, new List<MethodInfo>());
                        methodDependencies[attribute.Name].Add(method);
                    }
                }
            }

            return methodDependencies;
        }

        /// <summary>
        /// Initializes the notifiable collections.
        /// </summary>
        private IDictionary<string, INotifyCollectionChanged> InitializeNotifiableCollections()
        {
            var notifiableCollections = new Dictionary<string, INotifyCollectionChanged>();
            lock (notifiableCollections)
            {
                foreach (var property in this.GetType().GetProperties())
                {
                    if (typeof(INotifyCollectionChanged).IsAssignableFrom(property.PropertyType))
                    {
                        var collection = property.GetValue(this, null) as INotifyCollectionChanged;
                        notifiableCollections.Add(property.Name, collection);
                    }
                }
            }

            return notifiableCollections;
        }

        /// <summary>
        /// Initializes the property changed.
        /// </summary>
        private void InitializePropertyChanged()
        {
            this.PropertyChanged += OnPropertyChanged;

            foreach (var collection in NotifiableCollections.Values)
            {
                if (collection != null)
                    collection.CollectionChanged += OnCollectionChanged;
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null)
                return;

            UpdatePropertyChanged(e.PropertyName);
            RaiseDependenciesPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Updates the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void UpdatePropertyChanged(string propertyName)
        {
            INotifyCollectionChanged collection;
            if (NotifiableCollections.TryGetValue(propertyName, out collection))
            {
                var senderCollection = GetType().GetProperty(propertyName).GetValue(this, null) as INotifyCollectionChanged;
                if (!object.ReferenceEquals(collection, senderCollection))
                {
                    if (collection != null)
                    {
                        try
                        {
                            collection.CollectionChanged -= OnCollectionChanged;
                        }
                        catch (InvalidOperationException)
                        {
                            // This error might occur during dispose.
                        }
                    }
                    if (senderCollection != null)
                    {
                        senderCollection.CollectionChanged += OnCollectionChanged;
                    }
                    NotifiableCollections[propertyName] = senderCollection;
                }
            }
        }

        /// <summary>
        /// Raises the dependencies property changed.
        /// </summary>
        /// <param name="dependencyName">Name of the dependency.</param>
        public void RaiseDependenciesPropertyChanged(string dependencyName)
        {
            lock (PropertyDependencies)
            {
                IList<PropertyInfo> properties;
                if (PropertyDependencies.TryGetValue(dependencyName, out properties))
                {
                    foreach (var property in properties)
                    {
                        RaisePropertyChanged(property.Name);
                    }
                }
            }

            lock (MethodDependencies)
            {
                IList<MethodInfo> methods;
                if (MethodDependencies.TryGetValue(dependencyName, out methods))
                {
                    foreach (var method in methods)
                    {
                        method.Invoke(this, null);
                    }
                }
            }
        }

        /// <summary>
        /// Called when collection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var collection in NotifiableCollections.Where(nc => object.ReferenceEquals(sender, nc.Value)).OfType<KeyValuePair<string, INotifyCollectionChanged>?>())
            {
                RaisePropertyChanged(collection.Value.Key);
            }
        }

        /// <summary>
        /// Removes the property changed handlers.
        /// </summary>
        private void RemovePropertyChangedHandlers()
        {
            this.PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>
        /// Removes the collection changed handlers.
        /// </summary>
        private void RemoveCollectionChangedHandlers()
        {
            if (NotifiableCollections != null)
            {
                foreach (var item in NotifiableCollections.Values)
                {
                    if (item == null)
                        continue;
                    try
                    {
                        item.CollectionChanged -= OnCollectionChanged;
                    }
                    catch (InvalidOperationException)
                    {
                        // This error might occur during dispose.
                    }
                }
                NotifiableCollections.Clear();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel"/> class.
        /// </summary>
        public ViewModel()
        {
            _dependencies = InitializePropertyDependencies();
            _methodDependencies = InitializeMethodDependencies();
            _notifiableCollections = InitializeNotifiableCollections();

            InitializePropertyChanged();
        }

        #endregion

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    DisposeManagedResources();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                DisposeUnmanagedResources();

                // Note disposing has been done.
                _disposed = true;
            }
        }

        /// <summary>
        /// Disposes the managed resources.
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
            RemovePropertyChangedHandlers();
            RemoveCollectionChangedHandlers();
        }

        /// <summary>
        /// Disposes the unmanaged resources.
        /// </summary>
        protected virtual void DisposeUnmanagedResources()
        {
        }

        #endregion

        #region Finalizers

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Model"/> is reclaimed by garbage collection.
        /// </summary>
        ~ViewModel()
        {
            Dispose(false);
        }

        #endregion
    }
}
