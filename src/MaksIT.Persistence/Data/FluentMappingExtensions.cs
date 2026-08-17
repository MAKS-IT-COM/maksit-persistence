using System.Linq.Expressions;
using System.Reflection;
using LinqToDB.Mapping;


namespace MaksIT.Persistence.Data;

/// <summary>
/// Fluent mapping helpers that apply snake_case column names to scalar properties.
/// </summary>
public static class FluentMappingExtensions {
  /// <summary>
  /// Maps public instance scalar properties to snake_case columns. Skip collections and ignored members.
  /// </summary>
  public static EntityMappingBuilder<T> MapScalarPropertiesToSnakeCase<T>(
    this EntityMappingBuilder<T> builder,
    params Expression<Func<T, object?>>[] ignore
  ) {
    var ignored = new HashSet<string>(StringComparer.Ordinal);
    foreach (var expr in ignore) {
      var name = GetMemberName(expr);
      if (name is not null)
        ignored.Add(name);
    }

    foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
      if (ignored.Contains(prop.Name) || !IsScalar(prop.PropertyType))
        continue;

      InvokeHasColumnName(builder, prop, NameConverter.ToSnakeCase(prop.Name));
    }

    foreach (var expr in ignore)
      builder.Property(expr).IsNotColumn();

    return builder;
  }

  static bool IsScalar(Type type) {
    var t = Nullable.GetUnderlyingType(type) ?? type;
    if (t.IsPrimitive || t.IsEnum)
      return true;

    return t == typeof(string)
      || t == typeof(Guid)
      || t == typeof(DateTime)
      || t == typeof(DateTimeOffset)
      || t == typeof(decimal)
      || t == typeof(byte[]);
  }

  static string? GetMemberName(LambdaExpression expression) {
    var body = expression.Body is UnaryExpression unary ? unary.Operand : expression.Body;
    return body is MemberExpression member ? member.Member.Name : null;
  }

  static void InvokeHasColumnName<T>(EntityMappingBuilder<T> builder, PropertyInfo prop, string columnName) {
    var propertyMethod = typeof(EntityMappingBuilder<T>)
      .GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Single(m => m.Name == nameof(EntityMappingBuilder<T>.Property) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);

    var lambda = MakePropertyLambda(typeof(T), prop);
    var generic = propertyMethod.MakeGenericMethod(prop.PropertyType);
    var columnBuilder = generic.Invoke(builder, [lambda])!;
    columnBuilder.GetType().GetMethod("HasColumnName")!.Invoke(columnBuilder, [columnName]);
  }

  static LambdaExpression MakePropertyLambda(Type entityType, PropertyInfo prop) {
    var param = Expression.Parameter(entityType, "x");
    var access = Expression.Property(param, prop);
    var funcType = typeof(Func<,>).MakeGenericType(entityType, prop.PropertyType);
    return Expression.Lambda(funcType, access, param);
  }
}