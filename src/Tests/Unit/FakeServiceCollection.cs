using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Cloud.Core.Messaging.AzureServiceBus.Tests.Unit
{
    public class FakeServiceCollection : IServiceCollection
    {
        List<ServiceDescriptor> _services = new List<ServiceDescriptor>();

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _services.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            _services.Add(item);
        }

        public void Clear()
        {
            _services.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }
        public int IndexOf(ServiceDescriptor item)
        {
            for (int i = 0; i < _services.Count; i++)
            {
                if (_services[i].ServiceType == item?.ServiceType)
                    return i;
            }

            return -1;
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public ServiceDescriptor this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
