using phaeno.api.Infrastructure.WebSearchServices.SupportingFiles;

namespace phaeno.api.Infrastructure.WebSearchServices;

public interface IWebSearchService
{
    void RebuildIndex(IEnumerable<IndexedPage> newPages);
    IEnumerable<string> GetAllIndexedUrls();
    void DeleteStaleDocuments(IEnumerable<string> liveUrls);
    List<IndexedPage> Search(string queryText);

}
