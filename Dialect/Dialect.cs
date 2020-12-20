using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dialect
{
    public class Fsm<TChar>
    {
        public readonly FsmState initial;
        public readonly FsmState final;

        private Fsm(FsmState initial, FsmState final)
        {
            this.initial = initial;
            this.final = final;
        }

        public static Fsm<TChar> Sequence(IEnumerable<TChar> sequence)
        {
            FsmState initial = FsmState.Create();
            FsmState final = FsmState.Create();

            FsmState current = initial;
            foreach (TChar c in sequence)
            {
                var next = FsmState.Create();
                current.Transitions.Add(new Transition(c, next));
                current = next;
            }

            current.EpsTransitions.Add(final);
            return new Fsm<TChar>(initial, final);
        }

        public Fsm<TChar> Then(Fsm<TChar> then)
        {
            Fsm<TChar> self = Clone();
            then = then.Clone();
            self.final.EpsTransitions.Add(then.initial);
            return new Fsm<TChar>(self.initial, then.final);
        }

        public Fsm<TChar> Or(Fsm<TChar> other)
        {
            Fsm<TChar> self = Clone();
            other = other.Clone();
            FsmState initial = FsmState.Create();
            FsmState final = FsmState.Create();
            initial.EpsTransitions.Add(self.initial);
            initial.EpsTransitions.Add(other.initial);
            self.final.EpsTransitions.Add(final);
            other.final.EpsTransitions.Add(final);
            return new Fsm<TChar>(initial, final);
        }

        public Fsm<TChar> Star()
        {
            Fsm<TChar> self = Clone();
            self.final.EpsTransitions.Add(self.initial);
            self.initial.EpsTransitions.Add(self.final);
            return this;
        }

        public Fsm<TChar> Clone()
        {
            return this.Map(c => c);
        }

        public Fsm<TChar2> Map<TChar2>(Func<TChar, TChar2> charFn)
        {
            Dictionary<Fsm<TChar>.FsmState, Fsm<TChar2>.FsmState> map =
                new Dictionary<Fsm<TChar>.FsmState, Fsm<TChar2>.FsmState>();
            return new Fsm<TChar2>(
                MapState(this.initial, charFn, map),
                MapState(this.final, charFn, map));
        }

        private Fsm<TChar2>.FsmState MapState<TChar2>(
            FsmState from,
            Func<TChar, TChar2> charFn,
            Dictionary<Fsm<TChar>.FsmState, Fsm<TChar2>.FsmState> map)
        {
            Fsm<TChar2>.FsmState result;
            if (map.TryGetValue(from, out result))
            {
                return result;
            }

            result = map[from] = new Fsm<TChar2>.FsmState();

            foreach (var transition in from.Transitions)
            {
                result.Transitions.Add(new Fsm<TChar2>.Transition(
                    charFn(transition.Char),
                    MapState(transition.FsmState, charFn, map)));
            }

            return result;
        }

        public struct Transition
        {
            public readonly TChar Char;
            public readonly FsmState FsmState;

            public Transition(TChar @char, FsmState fsmState)
            {
                this.Char = @char;
                this.FsmState = fsmState;
            }
        }

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
