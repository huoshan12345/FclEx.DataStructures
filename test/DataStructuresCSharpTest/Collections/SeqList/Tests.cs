using System.Collections.Generic;
using DataStructuresCSharpTest.Common;
using Xunit;
using FxUtility.Collections;

namespace DataStructuresCSharpTest.Collections.SeqList
{
    public abstract partial class SeqListTests<T> : IListGenericTests<T>
    {
        protected override bool EnumeratorCurrentUndefinedOperationThrows => true;

        #region IList<T> Helper Methods

        protected override IList<T> GenericIListFactory()
        {
            return GenericListFactory();
        }

        protected override IList<T> GenericIListFactory(int count)
        {
            return GenericListFactory(count);
        }

        #endregion

        #region List<T> Helper Methods

        protected virtual SeqList<T> GenericListFactory()
        {
            return new SeqList<T>();
        }

        protected virtual SeqList<T> GenericListFactory(int count)
        {
            var toCreateFrom = CreateEnumerable(EnumerableType.List, null, count, 0, 0);
            return new SeqList<T>(toCreateFrom);
        }

        protected void VerifyList(SeqList<T> list, SeqList<T> expectedItems)
        {
            Assert.Equal(expectedItems.Count, list.Count);

            //Only verify the indexer. List should be in a good enough state that we
            //do not have to verify consistancy with any other method.
            for (var i = 0; i < list.Count; ++i)
            {
                Assert.True(list[i] == null ? expectedItems[i] == null : list[i].Equals(expectedItems[i]));
            }
        }

        #endregion
    }
}
