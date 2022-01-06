using System;
using System.Buffers.Binary;
using Echo.Core;

namespace Echo.Concrete
{
    public readonly ref partial struct BitVectorSpan
    {
        // Note on performance:
        //
        // For most operations here, LLE of certain operations is relatively slow. We can often avoid having to do
        // full LLE however if the values involved are fully known and are of primitive sizes (e.g. a normal int32).
        // It is computationally cheaper on average to check whether this is the case and then use the native hardware
        // operations, than to always use full LLE on all operations. Hence, this implementation follows this strategy. 

        /// <summary>
        /// Interprets the bit vector as a signed 8 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public sbyte I8
        {
            get => unchecked((sbyte) Bits[0]);
            set => Bits[0] = unchecked((byte) value);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 8 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public byte U8
        {
            get => Bits[0];
            set => Bits[0] = value;
        }
        
        /// <summary>
        /// Interprets the bit vector as a signed 16 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public short I16
        {
            get => BinaryPrimitives.ReadInt16LittleEndian(Bits);
            set => BinaryPrimitives.WriteInt16LittleEndian(Bits, value);
        }
        
        /// <summary>
        /// Interprets the bit vector as an unsigned 16 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public ushort U16
        {
            get => BinaryPrimitives.ReadUInt16LittleEndian(Bits);
            set => BinaryPrimitives.WriteUInt16LittleEndian(Bits, value);
        }
        
        /// <summary>
        /// Interprets the bit vector as a signed 32 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public int I32
        {
            get => BinaryPrimitives.ReadInt32LittleEndian(Bits);
            set => BinaryPrimitives.WriteInt32LittleEndian(Bits, value);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 32 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public uint U32
        {
            get => BinaryPrimitives.ReadUInt32LittleEndian(Bits);
            set => BinaryPrimitives.WriteUInt32LittleEndian(Bits, value);
        }
        
        /// <summary>
        /// Interprets the bit vector as a signed 64 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public long I64
        {
            get => BinaryPrimitives.ReadInt64LittleEndian(Bits);
            set => BinaryPrimitives.WriteInt64LittleEndian(Bits, value);
        }

        /// <summary>
        /// Interprets the bit vector as an unsigned 64 bit integer, and gets or sets the immediate value for it.
        /// </summary>
        public ulong U64
        {
            get => BinaryPrimitives.ReadUInt64LittleEndian(Bits);
            set => BinaryPrimitives.WriteUInt64LittleEndian(Bits, value);
        }

        /// <summary>
        /// Interprets the bit vector as an integer and adds a second integer to it. 
        /// </summary>
        /// <param name="other">The integer to add.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        /// <returns>The value of the carry bit after the addition completed.</returns>
        public Trilean IntegerAdd(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerAddLle(other);

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 + other.U8);
                    U8 = new8;
                    return new8 < old8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 + other.U16);
                    U16 = new16;
                    return new16 < old16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 + other.U32;
                    U32 = new32;
                    return new32 < old32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 + other.U64;
                    U64 = new64;
                    return new64 < old64;
                
                default:
                    return IntegerAddLle(other);
            }
        }
        
        private Trilean IntegerAddLle(BitVectorSpan other)
        {
            var carry = Trilean.False;

            for (int i = 0; i < Count; i++)
            {
                var a = this[i];
                var b = other[i];

                // Implement full-adder logic.
                var s = a ^ b ^ carry;
                var c = (carry & (a ^ b)) | (a & b);

                this[i] = s;
                carry = c;
            }

            return carry;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and increments it by one. 
        /// </summary>
        /// <returns>The value of the carry bit after the increment operation completed.</returns>
        public Trilean IntegerIncrement()
        {
            if (Count > 64 || !IsFullyKnown)
                return IntegerIncrementLle();

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 + 1);
                    U8 = new8;
                    return new8 < old8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 + 1);
                    U16 = new16;
                    return new16 < old16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 + 1;
                    U32 = new32;
                    return new32 < old32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 + 1;
                    U64 = new64;
                    return new64 < old64;
                
                default:
                    return IntegerIncrementLle();
            }
        }

        private Trilean IntegerIncrementLle()
        {
            // Optimized version of full-adder that does not require allocation of another vector, and short circuits
            // after carry does not have any effect any more. 

            var carry = Trilean.True;

            for (int i = 0; i < Count && carry != Trilean.False; i++)
            {
                var a = this[i];

                // Implement reduced adder logic.
                var s = a ^ carry;
                var c = carry & a;

                this[i] = s;
                carry = c;
            }

            return carry;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and negates it according to the two's complement semantics.
        /// </summary>
        public void IntegerNegate()
        {
            Not();
            IntegerIncrement();
        }

        /// <summary>
        /// Interprets the bit vector as an integer and subtracts a second integer from it.
        /// </summary>
        /// <param name="other">The integer to subtract.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        /// <returns>The value of the borrow bit after the subtraction completed.</returns>
        public Trilean IntegerSubtract(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerSubtractLle(other);

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 - other.U8);
                    U8 = new8;
                    return new8 > old8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 - other.U16);
                    U16 = new16;
                    return new16 > old16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 - other.U32;
                    U32 = new32;
                    return new32 > old32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 - other.U64;
                    U64 = new64;
                    return new64 > old64;
                
                default:
                    return IntegerSubtractLle(other);
            }
        }

        private Trilean IntegerSubtractLle(BitVectorSpan other)
        {
            var borrow = Trilean.False;

            for (int i = 0; i < Count; i++)
            {
                var a = this[i];
                var b = other[i];

                // Implement full-subtractor logic.
                var d = a ^ b ^ borrow;
                var bOut = (!a & borrow) | (!a & b) | (b & borrow);

                this[i] = d;
                borrow = bOut;
            }

            return borrow;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and decrements it by one. 
        /// </summary>
        /// <returns>The value of the carry bit after the decrement operation completed.</returns>
        public Trilean IntegerDecrement()
        {
            if (Count > 64 || !IsFullyKnown)
                return IntegerDecrementLle();

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 - 1);
                    U8 = new8;
                    return new8 > old8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 -  1);
                    U16 = new16;
                    return new16 > old16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 - 1;
                    U32 = new32;
                    return new32 > old32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 - 1;
                    U64 = new64;
                    return new64 > old64;
                
                default:
                    return IntegerDecrementLle();
            }
        }

        private Trilean IntegerDecrementLle()
        {
            // Optimized version of full-subtractor that does not require allocation of another vector, and short
            // circuits after borrow does not have any effect any more.
            
            var borrow = Trilean.True;

            for (int i = 0; i < Count && borrow != Trilean.False; i++)
            {
                var a = this[i];

                // Implement reduced subtractor logic.
                var d = a ^ borrow;
                var bOut = !a & borrow;

                this[i] = d;
                borrow = bOut;
            }

            return borrow;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and multiplies it by a second integer.
        /// </summary>
        /// <param name="other">The integer to multiply the current integer with.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        /// <returns>A value indicating whether the result was truncated.</returns>
        public Trilean IntegerMultiply(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            if (Count > 64 || !IsFullyKnown || !other.IsFullyKnown)
                return IntegerMultiplyLle(other);

            switch (Count)
            {
                case 8:
                    byte old8 = U8;
                    byte new8 = (byte) (old8 * other.U8);
                    U8 = new8;
                    return other.U8 != 0 && old8 > byte.MaxValue / other.U8;

                case 16:
                    ushort old16 = U16;
                    ushort new16 = (ushort) (old16 * other.U16);
                    U16 = new16;
                    return other.U16 != 0 && old16 > ushort.MaxValue / other.U16;

                case 32:
                    uint old32 = U32;
                    uint new32 = old32 * other.U32;
                    U32 = new32;
                    return other.U32 != 0 && old32 > uint.MaxValue / other.U32;
                
                case 64:
                    ulong old64 = U64;
                    ulong new64 = old64 * other.U64;
                    U64 = new64;
                    return other.U64 != 0 && old64 > ulong.MaxValue / other.U64;
                
                default:
                    return IntegerMultiplyLle(other);
            }
        }

        private Trilean IntegerMultiplyLle(BitVectorSpan other)
        {
            // We implement the standard long multiplication algo by adding and shifting, but instead of storing all
            // intermediate results, we can precompute the two possible intermediate results, shift them, and add them
            // to an end result to preserve time and memory.
            
            // Since there are three possible values in a trilean, there are three possible intermediate results.

            // 1) First possible intermediate result is the current value multiplied by 0. Adding zeroes to a number is
            // equivalent to the identity operation, which means it is redundant to compute this.

            // 2) Second possibility is the current value multiplied by 1.
            var multipliedByOne = GetTemporaryBitVector(0, Count);
            CopyTo(multipliedByOne);

            // 3) Third possibility is thee current value multiplied by ?. This is effectively marking all set bits unknown.
            var multipliedByUnknown = GetTemporaryBitVector(1, Count);
            CopyTo(multipliedByUnknown);

            var mask = multipliedByUnknown.KnownMask;
            mask.Not();
            mask.Or(multipliedByUnknown.Bits);
            mask.Not();

            // Clear all bits, so we can use ourselves as a result bit vector.
            Clear();
            
            var carry = Trilean.False;

            // Perform addition-shift algorithm.
            int lastShiftByOne = 0;
            int lastShiftByUnknown = 0;
            for (int i = 0; i < Count; i++)
            {
                var bit = other[i];

                if (!bit.IsKnown)
                {
                    multipliedByUnknown.ShiftLeft(i - lastShiftByUnknown);
                    carry |= IntegerAdd(multipliedByUnknown);
                    lastShiftByUnknown = i;
                }
                else if (bit.ToBoolean())
                {
                    multipliedByOne.ShiftLeft(i - lastShiftByOne);
                    carry |= IntegerAdd(multipliedByOne);
                    lastShiftByOne = i;
                }
            }
            
            return carry;
        }
    }
}