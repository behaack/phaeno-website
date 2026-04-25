using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace phaeno.api.Infrastructure.WebSearchServices.SupportingFiles
{
    public sealed class PorterStemFilter : TokenFilter
    {
        private readonly PorterStemmer stemmer = new PorterStemmer();
        private readonly ICharTermAttribute termAttr;

        public PorterStemFilter(TokenStream input) : base(input)
        {
            termAttr = AddAttribute<ICharTermAttribute>();
        }

        public override bool IncrementToken()
        {
            if (!m_input.IncrementToken())
                return false;

            string term = termAttr.ToString();
            string stemmed = stemmer.Stem(term);

            if (!string.IsNullOrEmpty(stemmed) && !stemmed.Equals(term, StringComparison.Ordinal))
            {
                termAttr.SetEmpty().Append(stemmed);
            }

            return true;
        }
    }
}
