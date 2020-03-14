namespace Serialization
{
    using System;

    public class TagAttribute : Attribute
    {
        public readonly int Tag;

        public TagAttribute(int tag)
        {
            this.Tag = tag;
        }
    }
}
