namespace Dialect
{
    using System;
    using System.Collections.Generic;

    public partial class Fsm<TChar>
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

            foreach (var epsTransition in from.EpsTransitions)
            {
                result.EpsTransitions.Add(MapState(epsTransition, charFn, map));
            }

            return result;
        }
    }
}
