using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osk42 {

	public static class StateMachine {
		public enum Operation {
			Init,
			Update,
			Exit,
		}
	}

	public class StateMachine<T> {

		public struct Result {
			public StateFunc nextFunc;
			public static Result Default => new Result();
			public static Result Change(StateFunc nextFunc) {
				return new Result() {
					nextFunc = nextFunc
				};
			}
		}

		public delegate Result StateFunc(T t, StateMachine.Operation ope);

		StateFunc func_ = (_t, _ope) => Result.Default;
		StateFunc nextFunc_;
		public float time;

		public void SwitchState(StateFunc func) {
			nextFunc_ = func;
		}

		public void Update(T t) {
			if (nextFunc_ != null) {
				func_(t, StateMachine.Operation.Exit);
				func_ = nextFunc_;
				time = 0f;
				func_(t, StateMachine.Operation.Init);
			}
			var result = func_(t, StateMachine.Operation.Update);
			nextFunc_ = result.nextFunc;
			time += Time.deltaTime;
		}
	}
}
