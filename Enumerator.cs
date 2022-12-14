/* *********************************************************************
 * Date: 03 May 2012
 * Created by: Zoltan Juhasz
 * E-Mail: forge@jzo.hu
***********************************************************************/

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Forge.Collections
{

    /// <summary>
    /// Mutable enumerator implementation
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Enumerator<T> : IEnumeratorSpecialized<T>, IEnumerator
    {

        private readonly ISubList<T> mList;
        private int mIndex;
        private int mVersion;
        private T mCurrent;
        private int mStartIndex;
        private int mEndIndex;

        /// <summary>
        /// Initializes a new instance of the Enumerator struct.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        public Enumerator(ISubList<T> list, int startIndex, int endIndex)
        {
            if (list == null) throw new ArgumentNullException("list");
            mList = list;
            mIndex = startIndex;
            mVersion = list.Version;
            mCurrent = default(T);
            mStartIndex = startIndex;
            mEndIndex = endIndex;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public bool MoveNext()
        {
            if ((mVersion == mList.Version) && (mIndex < mEndIndex))
            {
                mCurrent = mList[mIndex];
                mIndex++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            if (mVersion != mList.Version)
            {
                throw new InvalidOperationException("Collection modified while iterating on it.");
            }
            mIndex = mEndIndex + 1;
            mCurrent = default(T);
            return false;
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        /// <exception cref="System.InvalidOperationException">>Occurs when no element selected</exception>
        public T Current
        {
            get
            {
                if ((mIndex == mStartIndex) || (mIndex == (mEndIndex + 1)))
                {
                    throw new InvalidOperationException();
                }
                return mCurrent;
            }
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        void IEnumerator.Reset()
        {
            if (mVersion != mList.Version)
            {
                throw new InvalidOperationException("Collection modified while iterating on it.");
            }
            mIndex = mStartIndex;
            mCurrent = default(T);
        }

        /// <summary>
        /// Removes the current element.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Occurs when no element selected</exception>
        public void Remove()
        {
            if ((mIndex == mStartIndex) || (mIndex == (mEndIndex + 1)))
            {
                throw new InvalidOperationException();
            }
            mIndex--;
            mEndIndex--;
            mList.RemoveAt(mIndex);
            mVersion = mList.Version;
        }

        /// <summary>
        /// Determines whether this instance has next.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has next; otherwise, <c>false</c>.
        /// </returns>
        public bool HasNext()
        {
            return mIndex < mEndIndex;
        }

    }

}
