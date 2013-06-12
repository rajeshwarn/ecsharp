﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loyc
{
	public class InvalidStateException : InvalidOperationException
	{
		public InvalidStateException() : base(Localize.From("This object is in an invalid state.")) { }
		public InvalidStateException(string msg) : base(msg) { }
		public InvalidStateException(string msg, Exception innerException) : base(msg, innerException) { }
	}

	public class ConcurrentModificationException : InvalidOperationException
	{
		public ConcurrentModificationException() : base(Localize.From("A concurrect access was detected during modification.")) { }
		public ConcurrentModificationException(string msg) : base(msg) { }
		public ConcurrentModificationException(string msg, Exception innerException) : base(msg, innerException) { }
	}

	public class ReadOnlyException : InvalidOperationException
	{
		public ReadOnlyException() : base(Localize.From("An attempt was made to modify a read-only object.")) { }
		public ReadOnlyException(string msg) : base(msg) { }
		public ReadOnlyException(string msg, Exception innerException) : base(msg, innerException) { }
	}

	public class EmptySequenceException : InvalidOperationException
	{
		public EmptySequenceException() : base(Localize.From("Failed to access the sequence because it is empty.")) { }
	}

	public static class CheckParam
	{
		public static void IsNotNull(string paramName, object arg)
		{
			if (arg == null)
				ThrowArgumentNull(paramName);
		}
		public static void IsNotNegative(string argName, int value)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(argName, Localize.From(@"Argument ""{0}"" value '{1}' should not be negative.", argName, value));
		}
		public static void Range(string paramName, int value, int min, int max)
		{
			if (value < min || value > max)
				ThrowOutOfRange(paramName, value, min, max);
		}
		public static void ThrowOutOfRange(string argName)
		{
			throw new ArgumentOutOfRangeException(argName);
		}
		public static void ThrowOutOfRange(string argName, int value, int min, int max)
		{
			throw new ArgumentOutOfRangeException(argName, Localize.From(@"Argument ""{0}"" value '{1}' is not within the expected range ({2}..{3})", argName, value, min, max)); 
		}
		public static void ThrowIndexOutOfRange(int index, int count)
		{
			throw new IndexOutOfRangeException(Localize.From(@"Index '{1}' is not within the expected range ({2}..{3})", index, 0, count-1)); 
		}
		public static void ThrowIndexOutOfRange(int index, int min, int max)
		{
			throw new IndexOutOfRangeException(Localize.From(@"Index '{1}' is not within the expected range ({2}..{3})", index, min, max)); 
		}
		public static void ThrowArgumentNull(string argName)
		{
			throw new ArgumentNullException(argName);
		}
		public static void Arg(string argName, bool condition)
		{
			if (!condition)
				throw new ArgumentException(string.Format("Invalid value for '{0}'", argName));
		}
	}
}

namespace Loyc.Collections
{
	using Loyc.Essentials;

	public class EnumerationException : InvalidOperationException
	{
		public EnumerationException() : base(Localize.From("The collection was modified after enumeration started.")) { }
		public EnumerationException(string msg) : base(msg) { }
		public EnumerationException(string msg, Exception innerException) : base(msg, innerException) { }
	}

	public class KeyAlreadyExistsException : InvalidOperationException
	{
		public KeyAlreadyExistsException() : base(Localize.From("The item or key being added already exists in the collection.")) { }
		public KeyAlreadyExistsException(string msg) : base(msg) { }
		public KeyAlreadyExistsException(string msg, Exception innerException) : base(msg, innerException) { }
	}

	public class EmptySequenceException : InvalidOperationException
	{
		public EmptySequenceException() : base(Localize.From("The sequence is empty and cannot be accessed.")) { }
		public EmptySequenceException(string msg) : base(msg) { }
		public EmptySequenceException(string msg, Exception innerException) : base(msg, innerException) { }
	}
}
