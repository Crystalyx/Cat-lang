using Cat.Structure;

namespace Cat.Primitives
{
    public class CatByte : CatPrimitiveObject
    {
        public byte _value;
        public CatByte(object value) : base("byte")
        {
            switch (value)
            {
                case string s:
                    _value = byte.Parse(s);
                    break;
                case CatByte b:
                    _value = b._value;
                    break;
                default:
                    _value = (byte) value;
                    break;
            }
        }

        public override string ToString()
        {
            return _value+"";
        }
    }
}