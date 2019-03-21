using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {
	/*

	プレイヤー
			止まる
			歩く（前後キー）
			回転（左右キー）
			ノードの操作（角度のZ縮小、X拡大）
			連結

	さまよう仲間
			止まる
			目的地をランダムに決定して歩く

	連結した仲間
			ノードを追従する




	 */
	[System.Serializable]
	public sealed class PlayerNode {
		public int nodeId;
		public float targetLocalAngle;
		public float localAngle;
		public float forwardAngle;
		public Vector3 position;

		public bool IsClose() {
			return localAngle != 0 && localAngle == targetLocalAngle;
		}

		public static void calcPostion(List<PlayerNode> list, Vector3 parentPosition, float parentAngle, float sign, float length) {
			if (list.Count <= 0) return;
			var angle = parentAngle;
			{
				var n = list[0];
				n.position = parentPosition;
				n.forwardAngle = angle + 90f * sign;
			}

			for (var i = 0; i < list.Count - 1; i++) {
				var n1 = list[i];
				var n2 = list[i + 1];
				n2.forwardAngle = angle + 90f * sign;

				angle += n1.localAngle * sign;
				var rot = Quaternion.Euler(0f, angle, 0f);
				var diff = rot * Vector3.forward * length;
				var nextPosition = n1.position + diff;
				n2.position = nextPosition;
			}
		}

		public static void drawGizmo(List<PlayerNode> list) {
			for (var i = 0; i < list.Count; i++) {
				var n = list[i];
				Gizmos.DrawWireSphere(n.position, 0.1f);
			}
		}
	}
}
