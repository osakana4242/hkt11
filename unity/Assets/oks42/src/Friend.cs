using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {
	public class Friend : MonoBehaviour {
		public CharaBase charaBase;
		public float walkSpeed = 10f;
		public float angleSpeed = 180f;
		public Vector3 targetPosition;
		public StateMachine<Friend> sm;

		void Awake() {
			sm = new StateMachine<Friend>();
		}

		// Start is called before the first frame update
		void Start() {
			sm.SwitchState(StateFunc.main_g);
		}

		// Update is called once per frame
		void Update() {
			sm.Update(this);
		}

		void OnDrawGizmos() {
			Gizmos.DrawWireSphere(targetPosition, 0.5f);
		}

		public static class StateFunc {
			public static readonly StateMachine<Friend>.StateFunc main_g = (self, ope) => {
				switch (ope) {
					case StateMachine.Operation.Init: {
							var position = self.transform.position;
							self.targetPosition = position + new Vector3(
								(1 - Random.Range(0, 2) * 2) * Random.Range(1f, 2f),
								0f,
								(1 - Random.Range(0, 2) * 2) * Random.Range(1f, 2f)
								);

							break;
						}
					case StateMachine.Operation.Update: {
							var position = self.transform.position;
							var sqrDistance = (self.targetPosition - position).sqrMagnitude;
							var rot = self.transform.rotation;
							var trot = Quaternion.LookRotation(self.targetPosition - position);
							var nrot = Quaternion.RotateTowards(rot, trot, 360f * Time.deltaTime);
							var delta = nrot * Vector3.forward * Time.deltaTime;
							position += delta;
							self.transform.position = position;
							self.transform.rotation = nrot;
							self.charaBase.hasWalk = true;
							if (sqrDistance < 0.1f * 0.1f) {
								return StateMachine<Friend>.Result.Change(StateFunc.wait_g);
							}
							break;
						}
				}
				return StateMachine<Friend>.Result.Default;
			};

			public static readonly StateMachine<Friend>.StateFunc wait_g = (self, ope) => {
				switch (ope) {
					case StateMachine.Operation.Init: {
							break;
						}
					case StateMachine.Operation.Update: {
							if (1f <= self.sm.time) {
								return StateMachine<Friend>.Result.Change(StateFunc.main_g);
							}
							break;
						}
				}
				return StateMachine<Friend>.Result.Default;
			};
		}
	}
}
