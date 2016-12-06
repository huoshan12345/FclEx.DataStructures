using System.Collections.Generic;
using DataStructuresCSharpTest.Common;
using Xunit;
using FclEx.Collections;

namespace DataStructuresCSharpTest.Collections.DoublyCircularLinkedList
{
    public abstract partial class DoublyCircularLinkedListTests<T> : IListGenericTests<T>
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

        protected virtual DoublyCircularLinkedList<T> GenericListFactory()
        {
            return new DoublyCircularLinkedList<T>();
        }

        protected virtual DoublyCircularLinkedList<T> GenericListFactory(int count)
        {
            var toCreateFrom = CreateEnumerable(EnumerableType.List, null, count, 0, 0);
            return new DoublyCircularLinkedList<T>(toCreateFrom);
        }

        protected void VerifyList(DoublyCircularLinkedList<T> list, DoublyCircularLinkedList<T> expectedItems)
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
