namespace AuthService.Web.Core.Common;

public readonly struct Optional<T>
{
    private Optional(T? value)
    {
        Value = value;
        HasValue = true;
    }

    public T? Value { get; }
    public bool HasValue { get; }

    public static Optional<T> Of(T? value) => new(value);
}
