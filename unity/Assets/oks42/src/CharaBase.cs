using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {
	public class CharaBase : MonoBehaviour {
		public int gameInstanceId;
		public string animName;
		public bool hasMove;
		public bool hasRot;

		public static void updateAnim(CharaBase self) {
			var next = self.animName;
			if (self.hasMove) {
				next = "Run";
			} else if (self.hasMove) {
				next = "Walk01";
			} else {
				next = "Idle";
			}
			if (self.animName != next) {
				var animator = self.GetComponentInChildren<Animator>();
				animator.PlayInFixedTime(next, 0, 0.25f);
				self.animName = next;
			}
			self.hasMove = false;
		}
	}
}
