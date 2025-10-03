using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace STAMP.Lib
{
    internal class PropertyDefinition : IEquatable<PropertyDefinition>
    {
        public string PropertyName { get; private set; }
        public string TypeName { get; private set; }

        public PropertyDefinition(string propertyName, string typeName)
        {
            PropertyName = propertyName;
            TypeName = typeName;
        }

        public PropertyDefinition(IPropertySymbol propertySymbol)
        {
            PropertyName = propertySymbol.Name;
            TypeName = propertySymbol.Type.ToDisplayString();
        }

        public static bool operator ==(PropertyDefinition left, PropertyDefinition right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(PropertyDefinition left, PropertyDefinition right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyDefinition property)
            {
                return Equals(property);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(PropertyDefinition other)
        {
            return other.PropertyName == PropertyName && other.TypeName == TypeName;
        }
    }

    internal class PropertyDefinitionEqualityComparer : IEqualityComparer<PropertyDefinition>
    {
        public bool Equals(PropertyDefinition x, PropertyDefinition y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(PropertyDefinition obj)
        {
            return obj.PropertyName.Sum(x => x) * obj.TypeName.Sum(x => x);
        }
    }
}
