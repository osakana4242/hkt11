using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {
	public class MainPart : MonoBehaviour {
		public AppCore appCore;
		public StateMachine<MainPart> sm;
		// Start is called before the first frame update
		public Player player;
		public CharaBase ring;
		public Transform cameraAnchor;
		public List<CharaBase> charaList;
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
			refleshCharaList(charaList);
			sm.Update(this);
		}

		public static void TracePlayer(MainPart self, Transform cameraTr, Player player, List<CharaBase> friendList) {
			var bounds = GetBounds(self, player, friendList);
			var pos = cameraTr.position;
			var tpos = bounds.center + player.transform.forward * 2f;
			var npos = Vector3.Lerp(pos, tpos, self.appCore.assetData.config.cameraSpeed * Time.deltaTime);
			pos = npos;
			cameraTr.position = pos;
		}

		public static Bounds GetBounds(MainPart self, Player player, List<CharaBase> friendList) {
			var ppos = player.transform.position;
			var min = ppos;
			var max = ppos;
			foreach (var friend in friendList) {
				var fpos = friend.transform.position;
				min = Vector3.Min(min, fpos);
				max = Vector3.Max(max, fpos);
			}
			var bounds = new Bounds();
			bounds.SetMinMax(min, max);
			return bounds;
		}

		public static void TraceNode(Player player, List<CharaBase> friendList) {
			for (var i = 0; i < friendList.Count; i++) {
				var firend = friendList[i];
				var node = Player.findNode(player, firend.gameInstanceId);
				Slave.TraceNode(firend.slave(), node);
			}
		}

		public static void UpdateRing(List<PlayerNode> nodeList, float tangle, float angleSpeed) {
			for (var ni = 0; ni < nodeList.Count; ni++) {
				var node = nodeList[ni];
				var tangle2 = ni == 0 ? tangle / 2 : tangle;
				var next = Mathf.MoveTowardsAngle(node.localAngle, tangle2, angleSpeed * Time.deltaTime);
				node.targetLocalAngle = tangle2;
				node.localAngle = next;
				if (node.localAngle != tangle2) break;
			}
		}

		public static bool IsCloseRing(Player player) {
			var cnt = player.nodeListL.Count + player.nodeListR.Count - 1;
			if (cnt < 3) return false;

			var list = player.nodeListL;
			var index = list.FindIndex(_node => !_node.IsClose());
			if (0 <= index && index < list.Count - 1) return false;
			list = player.nodeListR;
			index = list.FindIndex(_node => !_node.IsClose());
			if (0 <= index && index < list.Count - 1) return false;
			return true;
		}

		public static TempolaryListPool<CharaBase>.Container findChara(List<CharaBase> list, System.Predicate<CharaBase> isMatch) {
			var lc = TempolaryListPool<CharaBase>.instance.alloc();
			for (var i = 0; i < list.Count; i++) {
				var item = list[i];
				if (!isMatch(item)) continue;
				lc.list.Add(item);
			}
			return lc;
		}

		public static TempolaryListPool<T>.Container findChara<T>(List<CharaBase> list, System.Predicate<CharaBase> isMatch) {
			var lc = TempolaryListPool<T>.instance.alloc();
			for (var i = 0; i < list.Count; i++) {
				var item = list[i];
				if (!isMatch(item)) continue;
				var comp = item.GetComponent<T>();
				lc.list.Add(comp);
			}
			return lc;
		}

		public static bool isHit(CharaBase c1, CharaBase c2) {
			var v = c1.transform.position - c2.transform.position;
			var sqrLength = 0.5f * 0.5f;
			if (sqrLength < v.sqrMagnitude) return false;
			return true;
		}

		/** null を除去. */
		public static void refleshCharaList(List<CharaBase> list) {
			for (var i = list.Count - 1; 0 <= i; i--) {
				var item = list[i];
				if (item != null) continue;
				list.RemoveAt(i);
			}
		}

		public static void updateCollision(MainPart self) {
			using (var lc1 = findChara(self.charaList, _item => _item.type == CharaType.Player || _item.type == CharaType.Slave))
			using (var lc2 = findChara(self.charaList, _item => _item.type == CharaType.Friend)) {
				var list1 = lc1.list;
				var list2 = lc2.list;
				for (var i1 = 0; i1 < list1.Count; i1++) {
					var c1 = list1[i1];
					for (var i2 = 0; i2 < list2.Count; i2++) {
						var friend = list2[i2];
						if (friend.type != CharaType.Friend) continue;
						var v = c1.transform.position - friend.transform.position;
						if (!isHit(c1, friend)) continue;
						addSlave(self, self.player.charaBase, friend);
					}
				}
			}

			using (var lc1 = findChara(self.charaList, _item => _item.type == CharaType.Player || _item.type == CharaType.Slave))
			using (var lc2 = findChara(self.charaList, _item => _item.type == CharaType.Enemy)) {
				var list1 = lc1.list;
				var list2 = lc2.list;
				for (var i1 = 0; i1 < list1.Count; i1++) {
					var c1 = list1[i1];
					for (var i2 = 0; i2 < list2.Count; i2++) {
						var friend = list2[i2];
						var v = c1.transform.position - friend.transform.position;
						if (c1.hp <= 0) continue;
						if (!isHit(c1, friend)) continue;
						c1.hp = 0;
						if (c1.type == CharaType.Slave) {
							removeSlave(self, self.player.charaBase, c1);
						}
						GameObject.Destroy(c1.gameObject);
					}
				}
			}

			using (var lc1 = findChara(self.charaList, _item => _item.type == CharaType.Ring))
			using (var lc2 = findChara(self.charaList, _item => _item.type == CharaType.Enemy)) {
				var list1 = lc1.list;
				var list2 = lc2.list;
				for (var i1 = 0; i1 < list1.Count; i1++) {
					var ring = list1[i1];
					for (var i2 = 0; i2 < list2.Count; i2++) {
						var enemy = list2[i2];
						var v = ring.transform.position - enemy.transform.position;
						if (enemy.hp <= 0) continue;
						if (!isHit(ring, enemy)) continue;
						enemy.hp = 0;
						GameObject.Destroy(enemy.gameObject);
					}
				}
			}
		}

		public static void removeSlave(MainPart self, CharaBase playerC, CharaBase friend) {
			if (friend.type != CharaType.Slave) {
				CharaBase.changeToSlave(friend);
			}
			var player = playerC.player();
			var listL = player.nodeListL;
			var listR = player.nodeListR;
			listL.RemoveAll(_node => _node.nodeId == friend.gameInstanceId);
			listR.RemoveAll(_node => _node.nodeId == friend.gameInstanceId);


			var tangle = 0f;
			UpdateRing(self.player.nodeListL, tangle, self.player.angleSpeed);
			UpdateRing(self.player.nodeListR, tangle, self.player.angleSpeed);
		}

		public static void addSlave(MainPart self, CharaBase playerC, CharaBase friend) {
			if (friend.type != CharaType.Slave) {
				CharaBase.changeToSlave(friend);
			}
			removeSlave(self, playerC, friend);
			var player = playerC.player();
			var listL = player.nodeListL;
			var listR = player.nodeListR;

			var nodeList = (listL.Count < listR.Count) ? listL : listR;
			nodeList.Add(new PlayerNode() {
				nodeId = friend.gameInstanceId,
			});
		}

		public static class StateFunc {
			public static readonly StateMachine<MainPart>.StateFunc reset_g = (self, ope) => {
				switch (ope) {
					case StateMachine.Operation.Update: {
							self.charaList.ForEach(_item => Object.Destroy(_item.gameObject));
							self.charaList.Clear();
							self.player = null;
							Random.InitState(65536);
							self.autoIncrement = 0;
							return StateMachine<MainPart>.Result.Change(init_g);
						}
				}
				return StateMachine<MainPart>.Result.Default;
			};

			public static readonly StateMachine<MainPart>.StateFunc init_g = (self, ope) => {
				switch (ope) {
					case StateMachine.Operation.Init: {
							var playerPrefab = self.appCore.assetData.getAsset<GameObject>("player");
							var friendPrefab = self.appCore.assetData.getAsset<GameObject>("friend");
							var enemyPrefab = self.appCore.assetData.getAsset<GameObject>("enemy");
							self.player = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity, self.transform).GetComponent<Player>();
							self.player.charaBase.gameInstanceId = self.createGameInstanceId();
							self.charaList.Add(self.player.charaBase);
							self.player.nodeListL.Clear();
							self.player.nodeListL.Add(new PlayerNode());
							self.player.nodeListR.Clear();
							self.player.nodeListR.Add(new PlayerNode());
							for (var i = 0; i < 2; i++) {
								var chara = GameObject.Instantiate(friendPrefab, new Vector3(3 * i, 0, 0), Quaternion.identity, self.transform).GetComponent<CharaBase>();
								chara.gameInstanceId = self.createGameInstanceId();
								self.charaList.Add(chara);
								addSlave(self, self.player.charaBase, chara);
							}
							for (var i = 0; i < 8; i++) {
								var chara = GameObject.Instantiate(friendPrefab, new Vector3(3 * i, 0, 10), Quaternion.identity, self.transform).GetComponent<CharaBase>();
								CharaBase.changeToFriend(chara);
								chara.gameInstanceId = self.createGameInstanceId();
								self.charaList.Add(chara);
							}

							for (var i = 0; i < 32; i++) {
								var chara = GameObject.Instantiate(enemyPrefab, new Vector3(2 * i, 0, 5), Quaternion.identity, self.transform).GetComponent<CharaBase>();
								chara.gameInstanceId = self.createGameInstanceId();
								self.charaList.Add(chara);
							}
							break;
						}
					case StateMachine.Operation.Update: {
							{
								// 歩行操作.
								var player = self.player;
								Vector3 v = Vector3.zero;
								if (Input.GetKey(KeyCode.UpArrow)) {
									v.z = 1f;
								}
								if (Input.GetKey(KeyCode.DownArrow)) {
									v.z = -1f;
								}
								var hasShift = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
								if (hasShift && Input.GetKey(KeyCode.LeftArrow)) {
									v.x = -1f;
								}
								if (hasShift && Input.GetKey(KeyCode.RightArrow)) {
									v.x = 1f;
								}
								if (v != Vector3.zero) {
									var pos = player.transform.position;
									var dir = player.transform.rotation * v;
									var delta = dir * player.walkSpeed * Time.deltaTime;
									pos += delta;
									player.transform.position = pos;
									player.charaBase.hasRun = true;
								}
							}
							{
								// 回転操作.
								float f = 0f;
								var hasShift = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
								if (!hasShift && Input.GetKey(KeyCode.LeftArrow)) {
									f = -1f;
								}
								if (!hasShift && Input.GetKey(KeyCode.RightArrow)) {
									f = 1f;
								}
								if (f != 0f) {
									var angles = self.player.transform.rotation.eulerAngles;
									var delta = f * self.player.angleSpeed * Time.deltaTime;
									angles.y += delta;
									self.player.transform.rotation = Quaternion.Euler(0f, angles.y, 0f);
									self.player.charaBase.hasRot = true;
								}
							}
							{
								// 開閉操作.
								float f = 0f;
								if (Input.GetKey(KeyCode.Z)) {
									f = -1f;
								}
								if (Input.GetKey(KeyCode.X)) {
									f = 1f;
								}

								var tangle = 0f;
								var itemAngleMax = 360f / (self.player.nodeListL.Count + self.player.nodeListR.Count - 1);
								var itemAngleMin = 0f;
								if (f != 0f) {
									tangle = f < 0 ? itemAngleMax : itemAngleMin;
									UpdateRing(self.player.nodeListL, tangle, self.player.angleSpeed);
									UpdateRing(self.player.nodeListR, tangle, self.player.angleSpeed);
								}
							}
							{
								if (Input.GetKeyDown(KeyCode.R)) {
									return StateMachine<MainPart>.Result.Change(reset_g);
								}
							}


							using (var lc = findChara(self.charaList, _item => _item.type == CharaType.Slave)) {
								if (IsCloseRing(self.player)) {
									var bounds = GetBounds(self, self.player, lc.list);
									if (self.ring == null) {
										var ringPrefab = self.appCore.assetData.getAsset<GameObject>("ring");
										self.ring = GameObject.Instantiate(ringPrefab, bounds.center, Quaternion.identity).GetComponent<CharaBase>();
										self.ring.gameInstanceId = self.createGameInstanceId();
										self.charaList.Add(self.ring);
									} else {
										self.ring.transform.position = bounds.center;
									}
								} else {
									if (self.ring != null) {
										GameObject.Destroy(self.ring.gameObject);
									}
								}
								TraceNode(self.player, lc.list);
								TracePlayer(self, self.cameraAnchor, self.player, lc.list);
							}
							self.charaList.ForEach(_item => CharaBase.updateAnim(_item));
							updateCollision(self);
							if (self.player.charaBase.hp <= 0) {
									return StateMachine<MainPart>.Result.Change(dead_g);
							}
							break;
						}
				}
				return StateMachine<MainPart>.Result.Default;
			};
			public static readonly StateMachine<MainPart>.StateFunc dead_g = (self, ope) => {
				switch (ope) {
					case StateMachine.Operation.Init: {
							break;
						}
					case StateMachine.Operation.Update: {
							{
								if (Input.GetKeyDown(KeyCode.R)) {
									return StateMachine<MainPart>.Result.Change(reset_g);
								}
							}
							if (self.ring != null) {
								GameObject.Destroy(self.ring.gameObject);
							}
							self.charaList.ForEach(_item => CharaBase.updateAnim(_item));
							break;
						}
				}
				return StateMachine<MainPart>.Result.Default;
			};
		}
	}
}
