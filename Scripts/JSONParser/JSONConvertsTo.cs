public interface IJSONConvertsTo<T> where T : new()
{
    public T ConvertTo();
    public void ConvertFrom(T value);
}