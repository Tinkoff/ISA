namespace Tinkoff.ISA.Core.Documents
{
    public interface ISearchableByTitleDocument : ISearchableDocument
    {
        string Title { get; set; }
    }
}