namespace Filters
{
    public interface IFilter<T>
    {
        bool Filter(T entry);
    }
}
