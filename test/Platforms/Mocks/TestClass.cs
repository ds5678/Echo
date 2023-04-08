﻿using System;
using System.IO;
using System.Net;
using InlineIL;

namespace Mocks
{
    // ReSharper disable once MemberCanBePrivate.Global
    public class TestClass
    {
        public static void StaticMethod(int i) => Console.WriteLine(i);
        public void InstanceMethod(int i) => Console.WriteLine(i);
        public static string GetConstantString() => "Hello, world!";
        public static string GetIsEvenString(int i) => i % 2 == 0 ? "even" : "odd";
        public static bool GetBoolean() => true;

        public static void NoLocalsNoArguments()
        {
        }

        public static void SingleArgument(int x)
        {
            Console.WriteLine(x);
        }

        public static void ValueTypeArgument(SimpleStruct s, int x)
        {
            Console.WriteLine(s);
            Console.WriteLine(x);
        }

        public static void RefTypeArgument(object s, int x)
        {
            Console.WriteLine(s);
            Console.WriteLine(x);
        }

        public static void MultipleArguments(int x, int y, int z)
        {
            Console.WriteLine(x);
            Console.WriteLine(y);
            Console.WriteLine(z);
        }

        public static void SingleLocal()
        {
            IL.DeclareLocals(new LocalVar(new TypeRef(typeof(int))));
            IL.Emit.Ldc_I4_0();
            IL.Emit.Stloc_0();
            IL.Emit.Ret();
        }

        public static void MultipleLocals()
        {
            IL.DeclareLocals(
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int)))
            );
            IL.Emit.Ret();
        }

        public static void MultipleLocalsNoInit()
        {
            IL.DeclareLocals(
                false,
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int)))
            );
            IL.Emit.Ret();
        }

        public static void MultipleLocalsMultipleArguments(int a, int b, int c)
        {
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(c);

            IL.DeclareLocals(
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int))),
                new LocalVar(new TypeRef(typeof(int)))
            );
            IL.Emit.Ret();
        }

        public static int SimpleTest()
        {
            int x = 0x1337; 
            x += DangerousMethod(10);
            x &= 0xFF00;
            Console.WriteLine(x >= 0x1000 ? "x >= 0x1000" : "x < 0x1000");
            return x;
        }

        public static int DangerousMethod(int y)
        {
            return y + 100;
        }
        
        public int UnhandledException() => throw new Exception("This is an unhandled exception.");

        public int TryFinally(bool @throw)
        {
            int result = 0;
            
            try
            {
                if (@throw)
                    throw new Exception("This is an unhandled exception.");
                result++;
            }
            finally
            {
                result += 100;
            }

            return result;
        }

        public int TryCatch(bool @throw)
        {
            int result;
            
            try
            {
                if (@throw)
                    throw new Exception("This is an handled exception.");
                result = 1;
            }
            catch (Exception ex)
            {
                result = 2;
            }

            return result;   
        }

        public int TryCatchFinally(bool @throw)
        {
            int result = 0;

            try
            {
                if (@throw)
                    throw new Exception("This is an handled exception.");
                result = 1;
            }
            catch (Exception ex)
            {
                result = 2;
            }
            finally
            {
                result += 100;
            }

            return result;   
        }

        public int TryCatchCatch(int exceptionType)
        {
            int result = 0;

            try
            {
                result = exceptionType switch
                {
                    0 => throw new IOException("This is a handled IOException."),
                    1 => throw new WebException("This is a handled WebException."),
                    2 => throw new ArgumentException("This is an unhandled ArgumentException"),
                    _ => 1
                };
            }
            catch (IOException ex)
            {
                result = 2;
            }
            catch (WebException ex)
            {
                result = 3;
            }

            return result;   
        }
        
        public int TryCatchSpecificAndGeneral(int exceptionType)
        {
            int result = 0;

            try
            {
                result = exceptionType switch
                {
                    0 => throw new EndOfStreamException("This is a handled EndOfStreamException."),
                    1 => throw new IOException("This is a handled IOException."),
                    2 => throw new ArgumentException("This is an unhandled ArgumentException"),
                    _ => 1
                };
            }
            catch (EndOfStreamException ex)
            {
                result = 2;
            }
            catch (IOException ex)
            {
                result = 3;
            }

            return result;   
        }

        public int TryCatchCatchFinally(int exceptionType)
        {
            int result = 0;

            try
            {
                result = exceptionType switch
                {
                    0 => throw new IOException("This is a handled IOException."),
                    1 => throw new WebException("This is a handled WebException."),
                    2 => throw new ArgumentException("This is an unhandled ArgumentException"),
                    _ => 1
                };
            }
            catch (IOException ex)
            {
                result = 2;
            }
            catch (WebException ex)
            {
                result = 3;
            }
            finally
            {
                result += 100;
            }
            
            return result;
        }

        public int TryCatchFilters(int exceptionType)
        {
            int result = 0;

            try
            {
                result = exceptionType switch
                {
                    0 => throw new IOException("This is handled exception 0"),
                    1 => throw new IOException("This is handled exception 1"),
                    2 => throw new IOException("This is unhandled exception 2"),
                    3 => throw new ArgumentException("This is unhandled other exception"),
                    _ => 1
                };
            }
            catch (IOException ex) when (ex.Message.Contains("0"))
            {
                result = 2;
            }
            catch (IOException ex) when (ex.Message.Contains("1"))
            {
                result = 3;
            }

            return result;
        }
    }
}