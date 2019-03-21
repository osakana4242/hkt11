using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {
	public class MainPart : MonoBehaviour {
		public AppCore appCore;
		public StateMachine<MainPart> sm;
		// Start is called before the first frame update

		public Player player;
		public Transform cameraAnchor;
		public List<SlaveFriend> friendList;
		public int autoIncrement;
		public int createGameInstanceId() {
			return ++autoIncrement;
		}

		void Awake() {
			sm = new StateMachine<MainPart>();
		}

		void Start() {
			sm.SwitchState(StateFunc.init_g);
		}

		// Update is called once per frame
		void Update() {
			sm.Update(this);
		}

		public static void TracePlayer(MainPart self, Transform cameraTr, Player player) {

			var ppos = player.transform.position;
			var min = ppos;
			var max = ppos;
			foreach (var friend in self.friendList) {
				var fpos = friend.transform.position;
				min = Vector3.Min(min, fpos);
				max = Vector3.Max(max, fpos);
			}
			var bounds = new Bounds();
			bounds.SetMinMax(min, max);
			var pos = cameraTr.position;
			var tpos = bounds.center;
			var npos = Vector3.Lerp( pos, tpos, self.appCore.assetData.config.cameraSpeed * Time.deltaTime);
			pos = npos;
			cameraTr.position = pos;
		}

		public static void TraceNode(Player player, List<SlaveFriend> friendList) {
			for (var i = 0; i < friendList.Count; i++) {
				var firend = friendList[i];
				var node = Player.findNode(player, firend.charaBase.gameInstanceId);
				SlaveFriend.TraceNode(firend, node);
			}
		}

		public static void Piyo(List<PlayerNode> nodeList, float tangle, float angleSpeed) {
			for (var ni = 0; ni < nodeList.Count; ni++) {
				var node = nodeList[ni];
				var tangle2 = ni == 0 ? tangle / 2 : tangle;
				var next = Mathf.MoveTowardsAngle(node.localAngle, tangle2, angleSpeed * Time.deltaTime);
				node.localAngle = next;
				if (node.localAngle != tangle2) break;
			}
		}

		public static class StateFunc {
			public static readonly StateMachine<MainPart>.StateFunc init_g = (self, ope) => {
				switch (ope) {
					case StateMachine.Operation.Init: {
							var playerPrefab = self.appCore.assetData.getAsset<GameObject>("player");
							var friendPrefab = self.appCore.assetData.getAsset<GameObject>("friend");
							self.player = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity, self.transform).GetComponent<Player>();
							self.player.charaBase.gameInstanceId = self.createGameInstanceId();
							self.player.nodeListL.Clear();
							self.player.nodeListL.Add(new PlayerNode());
							self.player.nodeListR.Clear();
							self.player.nodeListR.Add(new PlayerNode());
							for (var i = 0; i < 8; i++) {
								var friend = GameObject.Instantiate(friendPrefab, new Vector3(3 * i, 0, 0), Quaternion.identity, self.transform).GetComponent<SlaveFriend>();
								friend.charaBase.gameInstanceId = self.createGameInstanceId();
								self.friendList.Add(friend);
								var nodeList = ((i & 1) == 0) ? self.player.nodeListL : self.player.nodeListR;
								nodeList.Add(new PlayerNode() {
									nodeId = friend.charaBase.gameInstanceId,
								});
							}
							break;
						}
					case StateMachine.Operation.Update: {
							{
								var player = self.player;
								var nextAnimName = player.playingAnimName;
								float f = 0f;
								if (Input.GetKey(KeyCode.UpArrow)) {
									f = 1f;
								}
								if (Input.GetKey(KeyCode.DownArrow)) {
									f = -1f;
								}
								if (f != 0f) {
									var pos = player.transform.position;
									var dir = player.transform.forward * f;
									var delta = dir * player.walkSpeed * Time.deltaTime;
									pos += delta;
									player.transform.position = pos;
									nextAnimName = "Idle";
									player.charaBase.hasMove = true;
								}
							}
							{
								float f = 0f;
								if (Input.GetKey(KeyCode.LeftArrow)) {
									f = -1f;
								}
								if (Input.GetKey(KeyCode.RightArrow)) {
									f = 1f;
								}
								if (f != 0f) {
									var angles = self.player.transform.rotation.eulerAngles;
									var delta = f * self.player.angleSpeed * Time.deltaTime;
									angles.y += delta;
									self.player.transform.rotation = Quaternion.Euler(0f, angles.y, 0f);
								}
							}
							{
								float f = 0f;
								if (Input.GetKey(KeyCode.Z)) {
									f = -1f;
								}
								if (Input.GetKey(KeyCode.X)) {
									f = 1f;
								}

								var tangle = 0f;
								var itemAngleMin = 360f / (self.player.nodeListL.Count + self.player.nodeListR.Count - 1);
								var itemAngleMax = 0f;
								if (f != 0f) {
									tangle = f < 0 ? itemAngleMin : itemAngleMax;
									Piyo(self.player.nodeListL, tangle, self.player.angleSpeed);
									Piyo(self.player.nodeListR, tangle, self.player.angleSpeed);
								}
							}
							TraceNode(self.player, self.friendList);
							CharaBase.updateAnim(self.player.charaBase);
							self.friendList.ForEach(_item => CharaBase.updateAnim(_item.charaBase));
							TracePlayer(self, self.cameraAnchor, self.player);

							break;
						}
				}
				return StateMachine<MainPart>.Result.Default;
			};
		}
	}
}
