using System.Reflection;
using HotelBooking.Core.Domain;
using Xunit;

namespace HotelBooking.Tests;

/// <summary>
/// Перевіряє напрям залежностей: домен і сервіси не знають про файли,
/// JSON і конкретні сховища. Порушення видно як червоний тест, а не на рев'ю.
/// </summary>
public class ArchitectureTests
{
    private const string Root = "HotelBooking.Core.";

    [Theory]
    [InlineData("Domain", "HotelBooking.Core.Infrastructure")]
    [InlineData("Domain", "System.IO")]
    [InlineData("Domain", "System.Text.Json")]
    [InlineData("Domain", "Microsoft.Extensions.Logging")]
    [InlineData("Services", "HotelBooking.Core.Infrastructure")]
    [InlineData("Services", "System.Text.Json")]
    [InlineData("Pricing", "HotelBooking.Core.Infrastructure")]
    [InlineData("Abstractions", "HotelBooking.Core.Infrastructure")]
    public void Layer_DoesNotDependOn(string layer, string forbidden)
    {
        List<string> violations = TypesOf(layer)
            .SelectMany(type => UsedTypes(type).Select(used => (type, used)))
            .Where(pair => (pair.used.Namespace ?? string.Empty).StartsWith(
                forbidden, StringComparison.Ordinal))
            .Select(pair => $"{pair.type.Name} -> {pair.used.FullName}")
            .Distinct()
            .ToList();

        Assert.True(violations.Count == 0, string.Join("; ", violations));
    }

    [Fact]
    public void Core_DoesNotReferenceApp()
    {
        Assembly core = typeof(Booking).Assembly;

        bool referencesApp = core.GetReferencedAssemblies()
            .Any(name => name.Name == "HotelBooking.App");

        Assert.False(referencesApp);
    }

    [Fact]
    public void Layers_AreNotEmpty()
    {
        Assert.NotEmpty(TypesOf("Domain"));
        Assert.NotEmpty(TypesOf("Services"));
        Assert.NotEmpty(TypesOf("Infrastructure"));
    }

    private static List<Type> TypesOf(string layer) =>
        typeof(Booking).Assembly.GetTypes()
            .Where(t => t.Namespace == Root + layer)
            .ToList();

    private static IEnumerable<Type> UsedTypes(Type type)
    {
        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        IEnumerable<Type> fields = type.GetFields(All).Select(f => f.FieldType);
        IEnumerable<Type> properties = type.GetProperties(All).Select(p => p.PropertyType);
        IEnumerable<Type> methods = type.GetMethods(All)
            .SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType));
        IEnumerable<Type> constructors = type.GetConstructors(All)
            .SelectMany(c => c.GetParameters().Select(p => p.ParameterType));
        IEnumerable<Type> bases = type.GetInterfaces()
            .Append(type.BaseType ?? typeof(object));

        return fields.Concat(properties).Concat(methods).Concat(constructors).Concat(bases)
            .SelectMany(Unwrap);
    }

    private static IEnumerable<Type> Unwrap(Type type)
    {
        if (type.HasElementType && type.GetElementType() is Type element)
        {
            return Unwrap(element);
        }

        return type.IsGenericType
            ? type.GetGenericArguments().SelectMany(Unwrap).Append(type)
            : new[] { type };
    }
}
