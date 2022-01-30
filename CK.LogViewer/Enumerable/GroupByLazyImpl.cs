using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CK.LogViewer.Enumerable
{
    public static class GroupByLazyExtension
    {
        public static IAsyncEnumerable<IAsyncGrouping<TKey, TValue>> GroupByLazy<TKey, TValue>(
            this IAsyncEnumerable<TValue> @this,
            Func<TValue, TKey> selector
        ) where TKey : notnull
            => new GroupByLazyImpl<TKey, TValue>( @this, selector );

        class GroupByLazyImpl<TKey, TValue> : IAsyncEnumerable<IAsyncGrouping<TKey, TValue>> where TKey : notnull
        {
            readonly IAsyncEnumerable<TValue> _enumerable;
            readonly Func<TValue, TKey> _selector;

            public GroupByLazyImpl( IAsyncEnumerable<TValue> enumerable, Func<TValue, TKey> selector )
            {
                _enumerable = enumerable;
                _selector = selector;
            }

            public IAsyncEnumerator<IAsyncGrouping<TKey, TValue>> GetAsyncEnumerator( CancellationToken cancellationToken = default )
                => new Enumerator( _enumerable.GetAsyncEnumerator( cancellationToken ), _selector );

            class Enumerator : IAsyncEnumerator<IAsyncGrouping<TKey, TValue>>
            {
                readonly Dictionary<TKey, Group> _groups;
                readonly IAsyncEnumerator<TValue> _enumerator;
                readonly Func<TValue, TKey> _selector;

                public Enumerator( IAsyncEnumerator<TValue> enumerator, Func<TValue, TKey> selector )
                {
                    _enumerator = enumerator;
                    _selector = selector;
                    _groups = new();
                    Current = null!;
                }
                public IAsyncGrouping<TKey, TValue> Current { get; private set; }

                public ValueTask DisposeAsync() => _enumerator.DisposeAsync();

                public async ValueTask<bool> MoveNextAsync()
                {
                    while( true )
                    {
                        bool res = await _enumerator.MoveNextAsync();
                        if( !res )
                        {
                            Current = null!;
                            return false;
                        }

                        TKey key = _selector( _enumerator.Current );
                        if( !_groups.TryGetValue( key, out Group? group ) )
                        {
                            group = new( key );
                            _groups.Add( key, group );
                            group.QueueItem( _enumerator.Current );
                            Current = group;
                            return true;
                        }
                        group.QueueItem( _enumerator.Current );
                    }
                }
            }

            class Group : IAsyncGrouping<TKey, TValue>
            {
                readonly Channel<TValue> _channel = Channel.CreateUnbounded<TValue>();
                public Group( TKey key ) => Key = key;

                public void QueueItem( TValue value )
                {
                    _channel.Writer.TryWrite( value );
                }

                public TKey Key { get; }

                public IAsyncEnumerator<TValue> GetAsyncEnumerator( CancellationToken cancellationToken = default )
                    => _channel.Reader.ReadAllAsync( cancellationToken ).GetAsyncEnumerator( cancellationToken );
            }
        }
    }
}
