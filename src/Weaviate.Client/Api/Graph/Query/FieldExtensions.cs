using System;
using System.Linq;

namespace Weaviate.Client;

public static class FieldExtensions
{
    public static Field AsField(this string name) => new() { Name = name };
    
    public static Field[] AsFields(this string name) => new[] { name.AsField() };
    
    public static Field AsAdditional(this string name) => new() { Name = name };
    
    public static Field AsAdditionalField(this Field field) => field;
    
    public static Field[] AsAdditionalFields(this Field[] fields) => fields;
}
