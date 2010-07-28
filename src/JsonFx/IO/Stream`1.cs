﻿#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.Collections.Generic;

namespace JsonFx.IO
{
	/// <summary>
	/// Supports forward-only iteration over an input sequence of <typeparamref name="T"/>
	/// </summary>
	public class Stream<T> : IStream<T>
	{
		#region Fields

		private readonly IEnumerator<T> Enumerator;
		private bool isReady;
		private bool isCompleted;
		private T current;

		private IList<T> chunk;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="sequence"></param>
		public Stream(IEnumerable<T> sequence)
		{
			if (sequence == null)
			{
				throw new ArgumentNullException("sequence");
			}

			this.Enumerator = sequence.GetEnumerator();
		}

		#endregion Fields

		#region IStream<T> Properties

		/// <summary>
		/// Determines if the input sequence has reached the end
		/// </summary>
		public virtual bool IsCompleted
		{
			get
			{
				this.EnsureReady();

				return this.isCompleted;
			}
		}

		#endregion IStream<T> Properties

		#region IStream<T> Methods

		/// <summary>
		/// Returns but does not remove the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public virtual T Peek()
		{
			this.EnsureReady();

			// return the current item or null if complete
			return this.current;
		}

		/// <summary>
		/// Returns and removes the item at the front of the sequence.
		/// </summary>
		/// <returns></returns>
		public virtual T Pop()
		{
			this.EnsureReady();

			// flag as needing to be iterated, but don't execute yet
			this.isReady = this.isCompleted;

			// return the current item
			return this.current;
		}

		#endregion IStream<T> Methods

		#region Chunking Members

		public bool IsChunking
		{
			get { return (this.chunk != null); }
		}

		public int ChunkSize
		{
			get
			{
				if (this.chunk == null)
				{
					throw new InvalidOperationException("Not currently chunking.");
				}

				return this.chunk.Count;
			}
		}

		public void BeginChunk()
		{
			if (this.chunk == null)
			{
				this.chunk = new List<T>();
			}
			else
			{
				this.chunk.Clear();
			}
		}

		public IEnumerable<T> EndChunk()
		{
			if (this.chunk == null)
			{
				throw new InvalidOperationException("Not currently chunking.");
			}

			// build chunk value
			IEnumerable<T> value = this.chunk;

			// reset internal buffer
			this.chunk = null;

			return value;
		}

		#endregion Chunking Members

		#region Methods

		/// <summary>
		/// Deferred execution of iterator
		/// </summary>
		private void EnsureReady()
		{
			// only execute when requested
			if (this.isReady)
			{
				return;
			}
			this.isReady = true;

			// lazy execution of MoveNext
			this.isCompleted = !this.Enumerator.MoveNext();

			// store the current item or null if complete
			if (this.isCompleted)
			{
				this.current = default(T);
			}
			else
			{
				this.current = this.Enumerator.Current;
			}
		}

		#endregion Methods

		#region IDisposable Members

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IDisposable)this.Enumerator).Dispose();
			}
		}

		#endregion IDisposable Members
	}
}
