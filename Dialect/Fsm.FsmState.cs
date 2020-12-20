namespace Dialect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public partial class Fsm<TChar>
    {
        public struct FsmState : IEquatable<FsmState>
        {
            private static long NextId = 0;

            public readonly long Id;
            public readonly List<Transition> Transitions;
            public readonly List<FsmState> EpsTransitions;

            private FsmState(
                IEnumerable<Transition> transitions = null,
                IEnumerable<FsmState> epsTransitions = null)
            {
                this.Id = Interlocked.Increment(ref NextId);
                this.Transitions = transitions?.ToList() ?? new List<Transition>();
                this.EpsTransitions = epsTransitions?.ToList() ?? new List<FsmState>();
            }

            public static FsmState Create(
                IEnumerable<Transition> transitions = null,
                IEnumerable<FsmState> epsTransitions = null)
            {
                return new FsmState(transitions, epsTransitions);
            }

            public bool Equals(FsmState other)
            {
                return this.Id == other.Id;
            }

            public override bool Equals(object other)
            {
                return other is FsmState otherState && Equals(otherState);
            }

            public override int GetHashCode()
            {
                return NextId.GetHashCode();
            }
        }
    }
}
