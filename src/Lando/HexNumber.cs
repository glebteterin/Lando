using System;

namespace Lando
{
	public class HexNumber
	{
		public static HexNumber Zero { get { return new HexNumber(0); } }
		public static HexNumber One { get { return new HexNumber(1); } }
		public static HexNumber Two { get { return new HexNumber(2); } }
		public static HexNumber Three { get { return new HexNumber(3); } }
		public static HexNumber Four { get { return new HexNumber(4); } }
		public static HexNumber Five { get { return new HexNumber(5); } }
		public static HexNumber Six { get { return new HexNumber(6); } }
		public static HexNumber Seven { get { return new HexNumber(7); } }
		public static HexNumber Eight { get { return new HexNumber(8); } }
		public static HexNumber Nine { get { return new HexNumber(9); } }
		public static HexNumber Ten { get { return new HexNumber(10); } }
		public static HexNumber Eleven { get { return new HexNumber(11); } }
		public static HexNumber Twelve { get { return new HexNumber(12); } }
		public static HexNumber Thirteen { get { return new HexNumber(13); } }
		public static HexNumber Fourteen { get { return new HexNumber(14); } }
		public static HexNumber Fifteen { get { return new HexNumber(15); } }

		internal byte Value { get; private set; }

		public HexNumber(Int16 decNumber)
		{
			if (0 > decNumber || decNumber > 255)
				throw new ArgumentOutOfRangeException("decNumber", "Parameter should be between 0 and 255");

			Value = Convert.ToByte(decNumber);
		}
	}
}