using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {
	[ExecuteInEditMode]
	public class Player : MonoBehaviour {
		public CharaBase charaBase;
		public float walkSpeed = 10f;
		public float angleSpeed = 180f;
		public List<PlayerNode> nodeListL;
		public List<PlayerNode> nodeListR;
		public float nodeLength = 1.5f;
		public string playingAnimName;

		void Awake() {
		}

		// Start is called before the first frame update
		void Start() {

		}

		// Update is called once per frame
		void Update() {
			var position = transform.position;
			PlayerNode.calcPostion(nodeListL, position, transform.rotation.eulerAngles.y - 90f, 1f, nodeLength);
			PlayerNode.calcPostion(nodeListR, position, transform.rotation.eulerAngles.y + 90f, -1f, nodeLength);
		}

		void OnDrawGizmos() {
			PlayerNode.drawGizmo(nodeListL);
			PlayerNode.drawGizmo(nodeListR);
		}
		public static PlayerNode findNode(Player player, int gameInstanceId) {
			var node = player.nodeListL.Find(_item => _item.nodeId == gameInstanceId);
			if (node != null) return node;
			return player.nodeListR.Find(_item => _item.nodeId == gameInstanceId);
		}

	}
}
