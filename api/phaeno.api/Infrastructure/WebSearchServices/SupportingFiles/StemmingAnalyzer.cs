using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace phaeno.api.Infrastructure.WebSearchServices.SupportingFiles
{
    public class StemmingAnalyzer : Analyzer
    {
        private readonly LuceneVersion matchVersion = LuceneVersion.LUCENE_48;

        protected override TokenStreamComponents CreateComponents(string fieldName, System.IO.TextReader reader)
        {
            // Standard tokenizer
            var tokenizer = new StandardTokenizer(matchVersion, reader);

            // Chain filters
            TokenStream tokenStream = tokenizer;
            tokenStream = new StandardFilter(matchVersion, tokenStream);   // Removes punctuation
            tokenStream = new LowerCaseFilter(matchVersion, tokenStream);  // Lowercases text
            tokenStream = new PorterStemFilter(tokenStream);               // Applies Porter stemming

            return new TokenStreamComponents(tokenizer, tokenStream);
        }
    }
}
