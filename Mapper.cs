using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleMapper
{
    public class Mapper
    {
        // TODO(selim): Use thread-safe dictionary
        Dictionary<Type, Func<object>> m_BuilderMap = new();
        BindingFlags m_Flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

        public T Map<T>(object source)
            where T : class
        {
            var sourceType = source.GetType();
            var resultType = typeof(T);
            var builder = GetOrCreateBuilder(resultType);
            var result = (T)builder();

            // Map
            var properties = source.GetType().GetProperties();
            for(int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var value = property.GetValue(source);
                var targetProperty = resultType.GetProperty(property.Name, m_Flags);
                if(targetProperty != null) 
                    targetProperty.SetValue(result, value);
            }

            return result;
        }

        // TODO(selim): Benchmark 
        public Func<object> GetOrCreateBuilder(Type type)
        {
            if (m_BuilderMap.TryGetValue(type, out var func))
                return func;

            func = Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
            m_BuilderMap[type] = func;
            return func;
        }
        
        public Func<object> GetOrCreateBuilder(object obj) =>
            GetOrCreateBuilder(obj.GetType());
    }
}
