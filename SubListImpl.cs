﻿/* *********************************************************************
 * Date: 03 May 2012
 * Created by: Zoltan Juhasz
 * E-Mail: forge@jzo.hu
***********************************************************************/

using Forge.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Forge.Collections
{

    /// <summary>
    /// Sub list (view) implementation for a mutable collection
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    [Serializable, DebuggerDisplay("Count = {Count}")]
    public sealed class SubListImpl<TItem> : AbstractSubList<TItem>
    {

        #region Field(s)

        private readonly ISubList<TItem> _list;
        private int _offset = 0;
        private int _size = 0;
        private int _expectedModCount = 0;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="SubListImpl&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="fromIndex">From index.</param>
        /// <param name="toIndex">To index.</param>
        public SubListImpl(ISubList<TItem> list, int fromIndex, int toIndex)
        {
            if (list== null) throw new ArgumentNullException("list");
            if (fromIndex < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException("fromIndex");
            }
            if (toIndex > list.Count)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException("toIndex");
            }
            if (fromIndex > toIndex)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(string.Format("fromIndex({0}) > toIndex({1})", fromIndex.ToString(), toIndex.ToString()));
            }
            _list = list;
            _offset = fromIndex;
            _size = toIndex - fromIndex + 1;
            _expectedModCount = list.Version;
        }

        #endregion

        #region Public method(s)

        /// <summary>
        /// Subs the list.
        /// </summary>
        /// <param name="fromIndex">From index.</param>
        /// <param name="toIndex">To index.</param>
        /// <returns>Sub list of the items</returns>
        public override ISubList<TItem> SubList(int fromIndex, int toIndex)
        {
            return new SubListImpl<TItem>(this, fromIndex, toIndex);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public override void Add(TItem item)
        {
            AddRange(_size, new TItem[] { item });
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public override void Clear()
        {
            while (Count > 0)
            {
                RemoveAt(0);
            }
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="c">The c.</param>
        public override void AddRange(IEnumerable<TItem> c)
        {
            AddRange(_size, c);
        }

        /// <summary>
        /// Inserts the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        public override void Insert(int index, TItem item)
        {
            if (index < 0 || index > _size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException("index");
            }
            CheckForComodification();
            _list.Insert(index + _offset, item);
            _expectedModCount = _list.Version;
            _size++;
            this.mVersion++;
        }

        /// <summary>
        /// Removes at.
        /// </summary>
        /// <param name="index">The index.</param>
        public override void RemoveAt(int index)
        {
            RangeCheck(index);
            CheckForComodification();
            _list.RemoveAt(index + _offset);
            _expectedModCount = _list.Version;
            _size--;
            this.mVersion++;
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public override void CopyTo(TItem[] array, int arrayIndex)
        {
            TItem[] temp = new TItem[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                temp[i] = this[i];
            }
            Array.Copy(temp, 0, array, arrayIndex, this.Count);
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public override bool Contains(TItem item)
        {
            bool result = false;

            if (this.Count > 0)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    TItem _item = this[i];
                    if (item == null && _item == null)
                    {
                        result = true;
                        break;
                    }
                    else if (item != null && _item != null && item.Equals(_item))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True, if the collection changed, otherwise False.</returns>
        public override bool Remove(TItem item)
        {
            bool result = false;

            if (this.Count > 0)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    TItem _item = this[i];
                    if (item == null && _item == null)
                    {
                        RemoveAt(i);
                        result = true;
                        break;
                    }
                    else if (item != null && _item != null && item.Equals(_item))
                    {
                        RemoveAt(i);
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets or sets the object at the specified index.
        /// </summary>
        /// <value>
        /// The value
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public override TItem this[int index]
        {
            get
            {
                RangeCheck(index);
                CheckForComodification();
                return _list[index + _offset];
            }
            set
            {
                RangeCheck(index);
                CheckForComodification();
                _list[index + _offset] = value;
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public override int Count
        {
            get
            {
                CheckForComodification();
                return _size;
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>Enumerator of generic items</returns>
        public override IEnumeratorSpecialized<TItem> GetEnumerator()
        {
            return new Enumerator<TItem>(this, 0, _size); // offset eredetileg a 0
        }

        /// <summary>
        /// Indexes the of.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>Index of the element, if found in the collection, -1 if not found.</returns>
        public override int IndexOf(TItem item)
        {
            int result = -1;

            if (this.Count > 0)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    TItem currentItem = this[i];
                    if (item == null && currentItem == null)
                    {
                        result = i;
                        break;
                    }
                    else if (item.Equals(currentItem))
                    {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        }

        #endregion

        #region Private method(s)

        private void RemoveRange(int fromIndex, int toIndex)
        {
            CheckForComodification();
            for (int i = fromIndex + _offset; i <= toIndex + _offset; i++)
            {
                _list.RemoveAt(i);
            }
            _expectedModCount = _list.Version;
            _size = _size - (toIndex - fromIndex);
            this.mVersion++;
        }

        private void AddRange(int index, IEnumerable<TItem> c)
        {
            if (index < 0 || index > _size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(string.Format("Index: {0}, Size: {1}", index.ToString(), _size.ToString()));
            }

            int cSize = 0;
            CheckForComodification();
            IEnumerator<TItem> e = c.GetEnumerator();
            while (e.MoveNext())
            {
                _list.Insert(_offset + index + cSize, e.Current);
                cSize++;
            }
            _expectedModCount = _list.Version;
            _size += cSize;
            this.mVersion++;
        }

        private void RangeCheck(int index)
        {
            if (index < 0 || index >= _size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(string.Format("Index: {0}, Size: {1}", index.ToString(), _size.ToString()));
            }
        }

        private void CheckForComodification()
        {
            if (_list.Version != _expectedModCount)
            {
                throw new InvalidOperationException();
            }
        }

        #endregion

    }

}
