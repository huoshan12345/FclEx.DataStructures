namespace FclEx.Collections
{
    public interface IStack<T>
    {
        /// <summary>
        /// Pushes an item to the top of the stack.
        /// </summary>
        /// <param name="item"></param>
        void Push(T item);

        /// <summary>
        /// Pops an item from the top of the stack.  If the stack is empty, Pop throws an InvalidOperationException.
        /// </summary>
        /// <returns></returns>
        T Pop();

        /// <summary>
        /// Returns the top object on the stack without removing it.  If the stack
        /// is empty, Peek throws an InvalidOperationException.
        /// </summary>
        /// <returns></returns>
        T Peek();
    }
}
