using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// From "Jacobs Data Solutions" on StackOverflow
public static class ReflectiveEnumerator
{
    static ReflectiveEnumerator() { }

    public static List<Type> GetEnumerableOfType<T>() where T : class
    {
        List<Type> objects = new() { typeof(T) };
        foreach (Type type in
            Assembly.GetAssembly(typeof(T)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
        {
            objects.Add(type);
        }
        return objects;
    }
}