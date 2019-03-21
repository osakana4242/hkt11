using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {

	public class TempolaryListPool {
		public static TempolaryListPool<T>.Container alloc<T>() {
			return TempolaryListPool<T>.instance.alloc();
		}
	}

	public class TempolaryListPool<T> {

		public static readonly TempolaryListPool<T> instance = new TempolaryListPool<T>();
		public List<List<T>> pool = new List<List<T>>();
		public Container alloc() {
			int index = pool.Count - 1;
			if (0 <= index) {
				var list = pool[index];
				pool.RemoveAt(index);
				return new Container(this, list);
			}
			return new Container(this, new List<T>());
		}

		public struct Container : System.IDisposable {
			TempolaryListPool<T> owner_;
			public readonly List<T> list;
			public Container(TempolaryListPool<T> owner, List<T> list) {
				this.owner_ = owner;
				this.list = list;
			}
			public void Dispose() {
				list.Clear();
				owner_.pool.Add(list);
			}
		}
	}
}
