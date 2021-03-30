using CK.Monitoring;
using CK.Text;
using J2N.Threading.Atomic;
using Lucene.Net.Analysis.Path;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CK.LogViewer
{
    public class LogSearcher : IDisposable
    {
        const LuceneVersion _appLuceneVersion = LuceneVersion.LUCENE_48;
        readonly FSDirectory _dir;
        readonly DirectoryReader _dirReader;
        readonly IndexSearcher _searcher;

        public LogSearcher( FSDirectory dir, DirectoryReader dirReader, IndexSearcher searcher )
        {
            _dir = dir;
            _dirReader = dirReader;
            _searcher = searcher;
        }

        public static LogSearcher Create( NormalizedPath path )
        {
            FSDirectory dir = FSDirectory.Open( path );
            DirectoryReader dirReader = DirectoryReader.Open( dir );
            IndexSearcher searcher = new( dirReader );
            return new LogSearcher( dir, dirReader, searcher );
        }

        public IEnumerable<(int, Document)> FilteredLogs( int depth, int scopedOnGroupId = -1 )
        {
            int minDepth = 0;
            MatchAllDocsQuery all = new();
            BooleanFilter filter = new();
            if( scopedOnGroupId != -1 )
            {
                Document doc = _dirReader.Document( scopedOnGroupId );
                int docDepth = doc.GetField( "depth" ).GetInt32Value()!.Value;
                depth += docDepth;
                minDepth = docDepth + 1;
                filter.Add( new FilterClause( new TermFilter( new Term( "groupPath", doc.Get( "groupPath" ) ) ), Occur.MUST ) );
            }
            filter.Add( new FilterClause( NumericRangeFilter.NewInt32Range( "depth", minDepth, depth, true, true ), Occur.MUST ) );
            TopDocs results = _searcher.Search( all, filter, int.MaxValue );
            return FromBitField( results.ScoreDocs.Select( s => s.Doc ) );
        }

        IEnumerable<(int, Document)> FromBitField( IEnumerable<int> ids ) => ids.Select( s => (s, _dirReader.Document( s )) );


        public void Dispose()
        {
            _dir.Dispose();
            _dirReader.Dispose();
        }
    }
}
