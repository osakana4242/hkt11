using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {
	public sealed class CharaBase : MonoBehaviour {
		public int gameInstanceId;
		public string animName;
		public CharaType type;
		public int hp = 1;
		public bool hasRun;
		public bool hasWalk;
		public bool hasRot;

		public Slave slave() => GetComponent<Slave>();
		public Friend friend()=> GetComponent<Friend>();
		public Player player() => GetComponent<Player>();

		public static void changeToFriend(CharaBase self) {
			Debug.Assert(self.type == CharaType.Slave);
			self.type = CharaType.Friend;
			var slave = self.slave();
			GameObject.Destroy(slave);
			var friend = self.gameObject.AddComponent<Friend>();
			friend.charaBase = self;
		}

		public static void changeToSlave(CharaBase self) {
			Debug.Assert(self.type == CharaType.Friend);
			self.type = CharaType.Slave;
			var friend = self.friend();
			GameObject.Destroy(friend);
			var slave = self.gameObject.AddComponent<Slave>();
			slave.charaBase = self;
		}

		public static void updateAnim(CharaBase self) {
			var next = self.animName;
			if (self.hasRun) {
				next = "run";
			} else if (self.hasWalk) {
				next = "walk";
			} else if (self.hasRot) {
				next = "walk";
			} else {
				next = "idle";
			}
			if (self.animName != next) {
				var animator = self.GetComponentInChildren<Animator>();
				if (animator != null) {
					animator.PlayInFixedTime(next, 0, 0.25f);
				}
				self.animName = next;
			}
			self.hasRun = false;
			self.hasWalk = false;
			self.hasRot = false;
		}
	}
	public enum CharaType {
		Undef,
		Player,
		Friend,
		Slave,
		Enemy,
		Ring,
	}
}
