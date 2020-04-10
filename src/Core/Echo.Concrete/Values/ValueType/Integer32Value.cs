using System;
using System.Collections;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a (partially) known concrete 32 bit integral value.
    /// </summary>
    public class Integer32Value : IntegerValue
    {
        /// <summary>
        /// Wraps an unsigned 32 bit integer into a fully concrete and known instance of <see cref="Integer32Value"/>.
        /// </summary>
        /// <param name="value">The 32 bit integer to wrap.</param>
        /// <returns>The concrete 32 bit integer.</returns>
        public static implicit operator Integer32Value(ushort value)
        {
            return new Integer32Value(value);
        }

        /// <summary>
        /// Wraps a signed 32 bit integer into a fully concrete and known instance of <see cref="Integer32Value"/>.
        /// </summary>
        /// <param name="value">The 32 bit integer to wrap.</param>
        /// <returns>The concrete 32 bit integer.</returns>
        public static implicit operator Integer32Value(short value)
        {
            return new Integer32Value(value);
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 32 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        /// <returns>The 32 bit integer.</returns>
        public static implicit operator Integer32Value(string bitString)
        {
            return new Integer32Value(bitString);
        }
        
        /// <summary>
        /// Represents the bitmask that is used for a fully known concrete 32 bit integral value. 
        /// </summary>
        public const uint FullyKnownMask = 0xFFFFFFFF;
        
        private uint _value;
        
        /// <summary>
        /// Creates a new, fully known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        public Integer32Value(int value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, fully known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        public Integer32Value(uint value)
            : this(value, FullyKnownMask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer32Value(int value, uint mask)
            : this(unchecked((uint) value), mask)
        {
        }

        /// <summary>
        /// Creates a new, partially known concrete 32 bit integral value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        /// <param name="mask">The bit mask indicating the bits that are known.</param>
        public Integer32Value(uint value, uint mask)
        {
            _value = value;
            Mask = mask;
        }

        /// <summary>
        /// Parses a (partially) known bit string into an 32 bit integer.
        /// </summary>
        /// <param name="bitString">The bit string to parse.</param>
        public Integer32Value(string bitString)
        {
            SetBits(bitString);
        }

        /// <inheritdoc />
        public override bool IsKnown => Mask == FullyKnownMask;

        /// <inheritdoc />
        public override int Size => sizeof(uint);

        /// <summary>
        /// Gets the signed representation of this 32 bit value.
        /// </summary>
        public int I32
        {
            get => unchecked((int) U32);
            set => U32 = unchecked((uint) value);
        }

        /// <summary>
        /// Gets the unsigned representation of this 32 bit value.
        /// </summary>
        public uint U32
        {
            get => _value & Mask;
            set => _value = value;
        }

        /// <summary>
        /// Gets a value indicating which bits in the integer are known.
        /// If bit at location <c>i</c> equals 1, bit <c>i</c> in <see cref="I32"/> and <see cref="U32"/> is known,
        /// and unknown otherwise.  
        /// </summary>
        public uint Mask
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override bool? GetBit(int index)
        {
            if (index < 0 || index >= 32)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ((Mask >> index) & 1) == 1 ? ((U32 >> index) & 1) == 1 : (bool?) null;
        }

        /// <inheritdoc />
        public override void SetBit(int index, bool? value)
        {
            if (index < 0 || index >= 32)
                throw new ArgumentOutOfRangeException(nameof(index));

            uint mask = 1u << index;
            
            if (value.HasValue)
            {
                Mask |= mask;
                U32 = (U32 & ~mask) | ((value.Value ? 1u : 0u) << index);
            }
            else
            {
                Mask &= ~mask;
            }
        }

        /// <inheritdoc />
        public override BitArray GetBits() => new BitArray(BitConverter.GetBytes(U32));

        /// <inheritdoc />
        public override BitArray GetMask() => new BitArray(BitConverter.GetBytes(Mask));

        /// <inheritdoc />
        public override void SetBits(BitArray bits, BitArray mask)
        {
            if (bits.Count != 32 || mask.Count != 32)
                throw new ArgumentException("Number of bits is not 32.");
            var buffer = new byte[4];
            bits.CopyTo(buffer, 0);
            U32 = BitConverter.ToUInt32(buffer, 0);
            mask.CopyTo(buffer, 0);
            Mask = BitConverter.ToUInt32(buffer, 0);
        }
        
        /// <inheritdoc />
        public override IValue Copy() => new Integer32Value(U32, Mask);

        /// <inheritdoc />
        public override void Not()
        {
            U32 = ~U32;
        }

        /// <inheritdoc />
        public override void And(IntegerValue other)
        {
            if (other is Integer32Value int32)
            {
                unchecked
                {
                    U32 = U32 & int32.U32;
                    Mask = ~(~Mask | ~int32.Mask);
                }
                
                return;
            }
            
            base.And(other);
        }

        /// <inheritdoc />
        public override void Or(IntegerValue other)
        {
            if (other is Integer32Value int32)
            {
                unchecked
                {
                    U32 = U32 | int32.U32;
                    Mask = ~(~Mask | ~int32.Mask);
                }

                return;
            }
            
            base.Or(other);
        }

        /// <inheritdoc />
        public override void Xor(IntegerValue other)
        {
            if (other is Integer32Value int32)
            {
                unchecked
                {
                    U32 = U32 ^ int32.U32;
                    Mask = ~(~Mask | ~int32.Mask);
                }
                
                return;
            }
            
            base.And(other);
        }
        
        /// <inheritdoc />
        public override void Add(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 += int32.U32;
            else
                base.Add(other);
        }

        /// <inheritdoc />
        public override void Subtract(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 -= int32.U32;
            else
                base.Add(other);
        }

        /// <inheritdoc />
        public override void Multiply(IntegerValue other)
        {
            if (IsKnown && other.IsKnown && other is Integer32Value int32)
                U32 *= int32.U32;
            else
                base.Multiply(other);
        }
    }
}