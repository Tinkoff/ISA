namespace Tinkoff.ISA.Core.Documents
{
    public interface ISearchableDocument
    { 
        string Id { get; set; }
        
        string Text { get; set; }
    }
}
