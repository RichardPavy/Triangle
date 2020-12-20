namespace Dialect
{
    public partial class Fsm<TChar>
    {
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
    }
}
