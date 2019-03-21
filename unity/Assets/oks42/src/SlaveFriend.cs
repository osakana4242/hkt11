using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {
	public class SlaveFriend : MonoBehaviour {
		public CharaBase charaBase;
		public float walkSpeed = 5f;
		public float angleSpeed = 360f;
		public float farDistance = 0.1f;

		public static void TraceNode(SlaveFriend self, PlayerNode node) {
			var pos = self.transform.position;
			var delta = Vector3.MoveTowards(pos, node.position, self.walkSpeed * Time.deltaTime) - pos;
			var nextPos = pos + delta;
			self.charaBase.hasMove = pos != nextPos;
			var rot = self.transform.rotation;
			var nextRot = rot;

			var distance = Vector3.SqrMagnitude(pos - node.position);
			var isFar = self.farDistance <= distance;
			Quaternion trot;
			if (isFar) {
				trot = Quaternion.LookRotation(node.position - pos);
			} else {
				trot = Quaternion.Euler(0f, node.forwardAngle, 0f);
			}
			nextRot = Quaternion.RotateTowards(rot, trot, self.angleSpeed * Time.deltaTime);

			self.transform.position = nextPos;
			self.transform.rotation = nextRot;
		}
	}
}
