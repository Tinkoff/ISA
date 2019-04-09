namespace Tinkoff.ISA.Domain.Search
{
    public abstract class SearchableWithTitle : SearchableText
    {
        public string Title { get; set; }
    }
}
