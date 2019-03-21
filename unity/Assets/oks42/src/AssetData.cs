using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace Osk42 {
	[CreateAssetMenu]
	[System.Serializable]
	public sealed class AssetData : ScriptableObject {

		[System.Serializable]
		public sealed class Config {
			public float cameraSpeed = 5f;
		}

		public Config config;

		Dictionary<string, Object> dict_;
		Dictionary<string, Object> Dict {
			get {
				if ( dict_ != null ) return dict_;
				dict_ = new Dictionary<string, Object>(assets.Length);
				foreach (var item in assets) {
					dict_[item.name] = item;
				}
				return dict_;
			}
		}

		public Object[] assets;

		public T getAsset<T>(string name) where T : Object {
			var t = Dict[name] as T;
			return t;
		}
	}
}
