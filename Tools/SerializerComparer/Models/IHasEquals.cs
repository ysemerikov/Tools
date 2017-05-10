namespace Tools.SerializerComparer.Models
{
    internal interface IHasEquals<T>
    {
        bool IsEquals(T b);
    }
}