// Aggregated and sourced from https://github.com/theraot/Theraot

// Copyright (c) Microsoft. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CA1036 // Override methods on comparable types
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1066 // Implement IEquatable when overriding Object.Equals
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2231 // Overload operator equals on overriding value type Equals
#pragma warning disable CC0074 // Make field readonly
#pragma warning disable RCS1212 // Remove redundant assignment.
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace System
{
	namespace Collections
	{
		public interface IStructuralComparable
		{
			int CompareTo(object other, IComparer comparer);
		}

		public interface IStructuralEquatable
		{
			bool Equals(object other, IEqualityComparer comparer);

			int GetHashCode(IEqualityComparer comparer);
		}
	}

	namespace Numerics
	{
		internal static class NumericsHelpers
		{
			public static uint CombineHash(uint u1, uint u2)
			{
				return ((u1 << 7) | (u1 >> 25)) ^ u2;
			}

			public static int CombineHash(int n1, int n2)
			{
				return (int)CombineHash((uint)n1, (uint)n2);
			}

			// Do an in-place two's complement. "Dangerous" because it causes
			// a mutation and needs to be used with care for immutable types.
			public static void DangerousMakeTwosComplement(uint[] d)
			{
				if (!(d?.Length > 0))
				{
					return;
				}

				d[0] = ~d[0] + 1;

				var i = 1;
				// first do complement and +1 as long as carry is needed
				for (; d[i - 1] == 0 && i < d.Length; i++)
				{
					ref var current = ref d[i];
					current = ~current + 1;
				}

				// now ones complement is sufficient
				for (; i < d.Length; i++)
				{
					ref var current = ref d[i];
					current = ~current;
				}
			}
		}
	}

	public static class Tuple
	{
		public static Tuple<T1> Create<T1>(T1 item1)
		{
			return new Tuple<T1>(item1);
		}

		public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
		{
			return new Tuple<T1, T2>(item1, item2);
		}

		public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
		{
			return new Tuple<T1, T2, T3>(item1, item2, item3);
		}

		public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
		}

		public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
		{
			return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
		{
			return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
		{
			return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
		{
			return new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(item1, item2, item3, item4, item5, item6, item7, new Tuple<T8>(item8));
		}
	}

	[Serializable]
	public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable
	{
		public Tuple(T1 item1)
		{
			Item1 = item1;
		}

		public T1 Item1 { get; }

		int IComparable.CompareTo(object obj)
		{
			return CompareTo(obj, Comparer<object>.Default);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return CompareTo(other, comparer);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			return other is Tuple<T1> tuple && comparer.Equals(Item1, tuple.Item1);
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(Item1);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "({0})", Item1);
		}

		private int CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is Tuple<T1> tuple))
			{
				throw new ArgumentException(string.Empty, nameof(other));
			}

			return comparer.Compare(Item1, tuple.Item1);
		}
	}

	[Serializable]
	public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable
	{
		public Tuple(T1 item1, T2 item2)
		{
			Item1 = item1;
			Item2 = item2;
		}

		public T1 Item1 { get; }

		public T2 Item2 { get; }

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return CompareTo(other, comparer);
		}

		int IComparable.CompareTo(object obj)
		{
			return CompareTo(obj, Comparer<object>.Default);
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is Tuple<T1, T2> tuple))
			{
				return false;
			}

			return comparer.Equals(Item1, tuple.Item1)
				   && comparer.Equals(Item2, tuple.Item2);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			var hash = comparer.GetHashCode(Item1);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item2);
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "({0}, {1})", Item1, Item2);
		}

		private int CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is Tuple<T1, T2> tuple))
			{
				throw new ArgumentException(string.Empty, nameof(other));
			}

			var result = comparer.Compare(Item1, tuple.Item1);
			if (result == 0)
			{
				result = comparer.Compare(Item2, tuple.Item2);
			}

			return result;
		}
	}

	[Serializable]
	public class Tuple<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable
	{
		public Tuple(T1 item1, T2 item2, T3 item3)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
		}

		public T1 Item1 { get; }

		public T2 Item2 { get; }

		public T3 Item3 { get; }

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return CompareTo(other, comparer);
		}

		int IComparable.CompareTo(object obj)
		{
			return CompareTo(obj, Comparer<object>.Default);
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is Tuple<T1, T2, T3> tuple))
			{
				return false;
			}

			return comparer.Equals(Item1, tuple.Item1)
				   && comparer.Equals(Item2, tuple.Item2)
				   && comparer.Equals(Item3, tuple.Item3);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			var hash = comparer.GetHashCode(Item1);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item2);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item3);
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2})", Item1, Item2, Item3);
		}

		private int CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is Tuple<T1, T2, T3> tuple))
			{
				throw new ArgumentException(string.Empty, nameof(other));
			}

			var result = comparer.Compare(Item1, tuple.Item1);
			if (result == 0)
			{
				result = comparer.Compare(Item2, tuple.Item2);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item3, tuple.Item3);
			}

			return result;
		}
	}

	[Serializable]
	public class Tuple<T1, T2, T3, T4> : IStructuralEquatable, IStructuralComparable, IComparable
	{
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
		}

		public T1 Item1 { get; }

		public T2 Item2 { get; }

		public T3 Item3 { get; }

		public T4 Item4 { get; }

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return CompareTo(other, comparer);
		}

		int IComparable.CompareTo(object obj)
		{
			return CompareTo(obj, Comparer<object>.Default);
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is Tuple<T1, T2, T3, T4> tuple))
			{
				return false;
			}

			return comparer.Equals(Item1, tuple.Item1)
				   && comparer.Equals(Item2, tuple.Item2)
				   && comparer.Equals(Item3, tuple.Item3)
				   && comparer.Equals(Item4, tuple.Item4);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			var hash = comparer.GetHashCode(Item1);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item2);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item3);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item4);
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3})", Item1, Item2, Item3, Item4);
		}

		private int CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is Tuple<T1, T2, T3, T4> tuple))
			{
				throw new ArgumentException(string.Empty, nameof(other));
			}

			var result = comparer.Compare(Item1, tuple.Item1);
			if (result == 0)
			{
				result = comparer.Compare(Item2, tuple.Item2);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item3, tuple.Item3);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item4, tuple.Item4);
			}

			return result;
		}
	}

	[Serializable]
	public class Tuple<T1, T2, T3, T4, T5> : IStructuralEquatable, IStructuralComparable, IComparable
	{
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
		}

		public T1 Item1 { get; }

		public T2 Item2 { get; }

		public T3 Item3 { get; }

		public T4 Item4 { get; }

		public T5 Item5 { get; }

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return CompareTo(other, comparer);
		}

		int IComparable.CompareTo(object obj)
		{
			return CompareTo(obj, Comparer<object>.Default);
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is Tuple<T1, T2, T3, T4, T5> tuple))
			{
				return false;
			}

			return comparer.Equals(Item1, tuple.Item1)
				   && comparer.Equals(Item2, tuple.Item2)
				   && comparer.Equals(Item3, tuple.Item3)
				   && comparer.Equals(Item4, tuple.Item4)
				   && comparer.Equals(Item5, tuple.Item5);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			var hash = comparer.GetHashCode(Item1);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item2);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item3);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item4);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item5);
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3}, {4})", Item1, Item2, Item3, Item4, Item5);
		}

		private int CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is Tuple<T1, T2, T3, T4, T5> tuple))
			{
				throw new ArgumentException(string.Empty, nameof(other));
			}

			var result = comparer.Compare(Item1, tuple.Item1);
			if (result == 0)
			{
				result = comparer.Compare(Item2, tuple.Item2);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item3, tuple.Item3);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item4, tuple.Item4);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item5, tuple.Item5);
			}

			return result;
		}
	}

	[Serializable]
	public class Tuple<T1, T2, T3, T4, T5, T6> : IStructuralEquatable, IStructuralComparable, IComparable
	{
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
		}

		public T1 Item1 { get; }

		public T2 Item2 { get; }

		public T3 Item3 { get; }

		public T4 Item4 { get; }

		public T5 Item5 { get; }

		public T6 Item6 { get; }

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return CompareTo(other, comparer);
		}

		int IComparable.CompareTo(object obj)
		{
			return CompareTo(obj, Comparer<object>.Default);
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is Tuple<T1, T2, T3, T4, T5, T6> tuple))
			{
				return false;
			}

			return comparer.Equals(Item1, tuple.Item1)
				   && comparer.Equals(Item2, tuple.Item2)
				   && comparer.Equals(Item3, tuple.Item3)
				   && comparer.Equals(Item4, tuple.Item4)
				   && comparer.Equals(Item5, tuple.Item5)
				   && comparer.Equals(Item6, tuple.Item6);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			var hash = comparer.GetHashCode(Item1);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item2);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item3);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item4);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item5);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item6);
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3}, {4}, {5})", Item1, Item2, Item3, Item4, Item5, Item6);
		}

		private int CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is Tuple<T1, T2, T3, T4, T5, T6> tuple))
			{
				throw new ArgumentException(string.Empty, nameof(other));
			}

			var result = comparer.Compare(Item1, tuple.Item1);
			if (result == 0)
			{
				result = comparer.Compare(Item2, tuple.Item2);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item3, tuple.Item3);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item4, tuple.Item4);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item5, tuple.Item5);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item6, tuple.Item6);
			}

			return result;
		}
	}

	[Serializable]
	public class Tuple<T1, T2, T3, T4, T5, T6, T7> : IStructuralEquatable, IStructuralComparable, IComparable
	{
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
		}

		public T1 Item1 { get; }

		public T2 Item2 { get; }

		public T3 Item3 { get; }

		public T4 Item4 { get; }

		public T5 Item5 { get; }

		public T6 Item6 { get; }

		public T7 Item7 { get; }

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return CompareTo(other, comparer);
		}

		int IComparable.CompareTo(object obj)
		{
			return CompareTo(obj, Comparer<object>.Default);
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is Tuple<T1, T2, T3, T4, T5, T6, T7> tuple))
			{
				return false;
			}

			return comparer.Equals(Item1, tuple.Item1)
				   && comparer.Equals(Item2, tuple.Item2)
				   && comparer.Equals(Item3, tuple.Item3)
				   && comparer.Equals(Item4, tuple.Item4)
				   && comparer.Equals(Item5, tuple.Item5)
				   && comparer.Equals(Item6, tuple.Item6)
				   && comparer.Equals(Item7, tuple.Item7);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			var hash = comparer.GetHashCode(Item1);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item2);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item3);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item4);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item5);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item6);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item7);
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3}, {4}, {5}, {6})", Item1, Item2, Item3, Item4, Item5, Item6, Item7);
		}

		private int CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is Tuple<T1, T2, T3, T4, T5, T6, T7> tuple))
			{
				throw new ArgumentException(string.Empty, nameof(other));
			}

			var result = comparer.Compare(Item1, tuple.Item1);
			if (result == 0)
			{
				result = comparer.Compare(Item2, tuple.Item2);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item3, tuple.Item3);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item4, tuple.Item4);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item5, tuple.Item5);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item6, tuple.Item6);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item7, tuple.Item7);
			}

			return result;
		}
	}

	[Serializable]
	public class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IStructuralEquatable, IStructuralComparable, IComparable
	{
		public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
		{
			CheckType(rest);
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Rest = rest;
		}

		public T1 Item1 { get; }

		public T2 Item2 { get; }

		public T3 Item3 { get; }

		public T4 Item4 { get; }

		public T5 Item5 { get; }

		public T6 Item6 { get; }

		public T7 Item7 { get; }

		public TRest Rest { get; }

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			return CompareTo(other, comparer);
		}

		int IComparable.CompareTo(object obj)
		{
			return CompareTo(obj, Comparer<object>.Default);
		}

		public override bool Equals(object obj)
		{
			return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (!(other is Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple))
			{
				return false;
			}

			return comparer.Equals(Item1, tuple.Item1)
				   && comparer.Equals(Item2, tuple.Item2)
				   && comparer.Equals(Item3, tuple.Item3)
				   && comparer.Equals(Item4, tuple.Item4)
				   && comparer.Equals(Item5, tuple.Item5)
				   && comparer.Equals(Item6, tuple.Item6)
				   && comparer.Equals(Item7, tuple.Item7)
				   && comparer.Equals(Rest, tuple.Rest);
		}

		public override int GetHashCode()
		{
			return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			var hash = comparer.GetHashCode(Item1);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item2);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item3);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item4);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item5);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item6);
			hash = (hash << 5) - hash + comparer.GetHashCode(Item7);
			hash = (hash << 5) - hash + comparer.GetHashCode(Rest);
			return hash;
		}

		public override string ToString()
		{
			var restString = Rest!.ToString();
			return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", Item1, Item2, Item3, Item4, Item5, Item6, Item7, restString.Substring(1, restString.Length - 2));
		}

		private static void CheckType(TRest rest)
		{
			if (rest == null || !typeof(TRest).IsGenericType)
			{
				throw new ArgumentException("The last element of an eight element tuple must be a Tuple.", nameof(rest));
			}

			var type = typeof(TRest).GetGenericTypeDefinition();
			if
			(
				type == typeof(Tuple<>)
				|| type == typeof(Tuple<,>)
				|| type == typeof(Tuple<,,>)
				|| type == typeof(Tuple<,,,>)
				|| type == typeof(Tuple<,,,,>)
				|| type == typeof(Tuple<,,,,,>)
				|| type == typeof(Tuple<,,,,,,>)
				|| type == typeof(Tuple<,,,,,,,>)
			)
			{
				return;
			}

			throw new ArgumentException("The last element of an eight element tuple must be a Tuple.", nameof(rest));
		}

		private int CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple))
			{
				throw new ArgumentException(string.Empty, nameof(other));
			}

			var result = comparer.Compare(Item1, tuple.Item1);
			if (result == 0)
			{
				result = comparer.Compare(Item2, tuple.Item2);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item3, tuple.Item3);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item4, tuple.Item4);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item5, tuple.Item5);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item6, tuple.Item6);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item7, tuple.Item7);
			}

			if (result == 0)
			{
				result = comparer.Compare(Item7, tuple.Item7);
			}

			return result;
		}
	}

	/// <summary>
	/// Helper so we can call some tuple methods recursively without knowing the underlying types.
	/// </summary>
	internal interface ITupleInternal
	{
		int Size { get; }

		int GetHashCode(IEqualityComparer comparer);

		string ToStringEnd();
	}

	/// <summary>
	/// The ValueTuple types (from arity 0 to 8) comprise the runtime implementation that underlies tuples in C# and struct tuples in F#.
	/// Aside from created via language syntax, they are most easily created via the ValueTuple.Create factory methods.
	/// The System.ValueTuple types differ from the System.Tuple types in that:
	/// - they are structs rather than classes,
	/// - they are mutable rather than readonly, and
	/// - their members (such as Item1, Item2, etc) are fields rather than properties.
	/// </summary>
	public struct ValueTuple
		: IEquatable<ValueTuple>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple>, ITupleInternal
	{
		int ITupleInternal.Size => 0;

		/// <summary>Creates a new struct 0-tuple.</summary>
		/// <returns>A 0-tuple.</returns>
		public static ValueTuple Create()
		{
			return default;
		}

		/// <summary>Creates a new struct 1-tuple, or singleton.</summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <returns>A 1-tuple (singleton) whose value is (item1).</returns>
		public static ValueTuple<T1> Create<T1>(T1 item1)
		{
			return new ValueTuple<T1>(item1);
		}

		/// <summary>Creates a new struct 2-tuple, or pair.</summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <returns>A 2-tuple (pair) whose value is (item1, item2).</returns>
		public static ValueTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
		{
			return new ValueTuple<T1, T2>(item1, item2);
		}

		/// <summary>Creates a new struct 3-tuple, or triple.</summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <returns>A 3-tuple (triple) whose value is (item1, item2, item3).</returns>
		public static ValueTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
		{
			return new ValueTuple<T1, T2, T3>(item1, item2, item3);
		}

		/// <summary>Creates a new struct 4-tuple, or quadruple.</summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <returns>A 4-tuple (quadruple) whose value is (item1, item2, item3, item4).</returns>
		public static ValueTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			return new ValueTuple<T1, T2, T3, T4>(item1, item2, item3, item4);
		}

		/// <summary>Creates a new struct 5-tuple, or quintuple.</summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <returns>A 5-tuple (quintuple) whose value is (item1, item2, item3, item4, item5).</returns>
		public static ValueTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
		{
			return new ValueTuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
		}

		/// <summary>Creates a new struct 6-tuple, or sextuple.</summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <returns>A 6-tuple (sextuple) whose value is (item1, item2, item3, item4, item5, item6).</returns>
		public static ValueTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
		{
			return new ValueTuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
		}

		/// <summary>Creates a new struct 7-tuple, or septuple.</summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <param name="item7">The value of the seventh component of the tuple.</param>
		/// <returns>A 7-tuple (septuple) whose value is (item1, item2, item3, item4, item5, item6, item7).</returns>
		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
		{
			return new ValueTuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
		}

		/// <summary>Creates a new struct 8-tuple, or octuple.</summary>
		/// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
		/// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
		/// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
		/// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
		/// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
		/// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
		/// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
		/// <typeparam name="T8">The type of the eighth component of the tuple.</typeparam>
		/// <param name="item1">The value of the first component of the tuple.</param>
		/// <param name="item2">The value of the second component of the tuple.</param>
		/// <param name="item3">The value of the third component of the tuple.</param>
		/// <param name="item4">The value of the fourth component of the tuple.</param>
		/// <param name="item5">The value of the fifth component of the tuple.</param>
		/// <param name="item6">The value of the sixth component of the tuple.</param>
		/// <param name="item7">The value of the seventh component of the tuple.</param>
		/// <param name="item8">The value of the eighth component of the tuple.</param>
		/// <returns>An 8-tuple (octuple) whose value is (item1, item2, item3, item4, item5, item6, item7, item8).</returns>
		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
		{
			return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>>(item1, item2, item3, item4, item5, item6, item7, Create(item8));
		}

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple other)
		{
			return 0;
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			return 0;
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			return 0;
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="ValueTuple"/>.</returns>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple;
		}

		/// <inheritdoc />
		/// <summary>Returns a value indicating whether this instance is equal to a specified value.</summary>
		/// <param name="other">An instance to compare to this instance.</param>
		/// <returns>true if <paramref name="other" /> has the same value as this instance; otherwise, false.</returns>
		public bool Equals(ValueTuple other)
		{
			return true;
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			return other is ValueTuple;
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return 0;
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return 0;
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return 0;
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>()</c>.
		/// </remarks>
		public override string ToString()
		{
			return "()";
		}

		string ITupleInternal.ToStringEnd()
		{
			return ")";
		}

		internal static int CombineHashCodes(int h1, int h2)
		{
			return NumericsHelpers.CombineHash(h1, h2);
		}

		internal static int CombineHashCodes(int h1, int h2, int h3)
		{
			return NumericsHelpers.CombineHash(CombineHashCodes(h1, h2), h3);
		}

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
		{
			return NumericsHelpers.CombineHash(CombineHashCodes(h1, h2, h3), h4);
		}

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
		{
			return NumericsHelpers.CombineHash(CombineHashCodes(h1, h2, h3, h4), h5);
		}

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6)
		{
			return NumericsHelpers.CombineHash(CombineHashCodes(h1, h2, h3, h4, h5), h6);
		}

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7)
		{
			return NumericsHelpers.CombineHash(CombineHashCodes(h1, h2, h3, h4, h5, h6), h7);
		}

		internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8)
		{
			return NumericsHelpers.CombineHash(CombineHashCodes(h1, h2, h3, h4, h5, h6, h7), h8);
		}
	}

	/// <summary>Represents a 1-tuple, or singleton, as a value type.</summary>
	/// <typeparam name="T1">The type of the tuple's only component.</typeparam>
	public struct ValueTuple<T1>
		: IEquatable<ValueTuple<T1>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1>>, ITupleInternal
	{
		/// <summary>
		/// The current <see cref="ValueTuple{T1}"/> instance's first component.
		/// </summary>
		public T1 Item1;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueTuple{T1}"/> value type.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		public ValueTuple(T1 item1)
		{
			Item1 = item1;
		}

		int ITupleInternal.Size => 1;

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple<T1> other)
		{
			return Comparer<T1>.Default.Compare(Item1, other.Item1);
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple<T1>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			var objTuple = (ValueTuple<T1>)obj;

			return Comparer<T1>.Default.Compare(Item1, objTuple.Item1);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple<T1>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			var objTuple = (ValueTuple<T1>)other;

			return comparer.Compare(Item1, objTuple.Item1);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1}"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
		/// <list type="bullet">
		///     <item><description>It is a <see cref="ValueTuple{T1}"/> value type.</description></item>
		///     <item><description>Its components are of the same types as those of the current instance.</description></item>
		///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
		/// </list>
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1> valueTuple && Equals(valueTuple);
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1}" />
		/// instance is equal to a specified <see cref="ValueTuple{T1}" />.
		/// </summary>
		/// <param name="other">The tuple to compare with this instance.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified tuple; otherwise, <see langword="false" />.</returns>
		/// <remarks>
		/// The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its field
		/// is equal to that of the current instance, using the default comparer for that field's type.
		/// </remarks>
		public bool Equals(ValueTuple<T1> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is ValueTuple<T1>))
			{
				return false;
			}

			var objTuple = (ValueTuple<T1>)other;

			return comparer.Equals(Item1, objTuple.Item1);
		}

		/// <summary>
		/// Returns the hash code for the current <see cref="ValueTuple{T1}"/> instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return EqualityComparer<T1>.Default.GetHashCode(Item1);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(Item1);
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return comparer.GetHashCode(Item1);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple{T1}"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple{T1}"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>(Item1)</c>,
		/// where <c>Item1</c> represents the value of <see cref="Item1"/>. If the field is <see langword="null"/>,
		/// it is represented as <see cref="string.Empty"/>.
		/// </remarks>
		public override string ToString()
		{
			return "(" + (Item1 == null ? string.Empty : Item1.ToString()) + ")";
		}

		string ITupleInternal.ToStringEnd()
		{
			return (Item1 == null ? string.Empty : Item1.ToString()) + ")";
		}
	}

	/// <summary>
	/// Represents a 2-tuple, or pair, as a value type.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
	[StructLayout(LayoutKind.Auto)]
	public struct ValueTuple<T1, T2>
		: IEquatable<ValueTuple<T1, T2>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2>>, ITupleInternal
	{
		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2}"/> instance's first component.
		/// </summary>
		public T1 Item1;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2}"/> instance's first component.
		/// </summary>
		public T2 Item2;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueTuple{T1, T2}"/> value type.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		/// <param name="item2">The value of the tuple's second component.</param>
		public ValueTuple(T1 item1, T2 item2)
		{
			Item1 = item1;
			Item2 = item2;
		}

		int ITupleInternal.Size => 2;

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple<T1, T2> other)
		{
			var c = Comparer<T1>.Default.Compare(Item1, other.Item1);
			return c != 0 ? c : Comparer<T2>.Default.Compare(Item2, other.Item2);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2}"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		///
		/// <remarks>
		/// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
		/// <list type="bullet">
		///     <item><description>It is a <see cref="ValueTuple{T1, T2}"/> value type.</description></item>
		///     <item><description>Its components are of the same types as those of the current instance.</description></item>
		///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
		/// </list>
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1, T2> valueTuple && Equals(valueTuple);
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2}" /> instance is equal to a specified <see cref="ValueTuple{T1, T2}" />.
		/// </summary>
		/// <param name="other">The tuple to compare with this instance.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified tuple; otherwise, <see langword="false" />.</returns>
		/// <remarks>
		/// The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its fields
		/// are equal to that of the current instance, using the default comparer for that field's type.
		/// </remarks>
		public bool Equals(ValueTuple<T1, T2> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
				   && EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
		}

		/// <summary>
		/// Returns the hash code for the current <see cref="ValueTuple{T1, T2}"/> instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return ValueTuple.CombineHashCodes
			(
				EqualityComparer<T1>.Default.GetHashCode(Item1),
				EqualityComparer<T2>.Default.GetHashCode(Item2)
			);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2}"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple{T1, T2}"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>(Item1, Item2)</c>,
		/// where <c>Item1</c> and <c>Item2</c> represent the values of the <see cref="Item1"/>
		/// and <see cref="Item2"/> fields. If either field value is <see langword="null"/>,
		/// it is represented as <see cref="string.Empty"/>.
		/// </remarks>
		public override string ToString()
		{
			return "(" + (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ")";
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple<T1, T2>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			return CompareTo((ValueTuple<T1, T2>)obj);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple<T1, T2>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			var objTuple = (ValueTuple<T1, T2>)other;

			var c = comparer.Compare(Item1, objTuple.Item1);
			return c != 0 ? c : comparer.Compare(Item2, objTuple.Item2);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2}"/> instance is equal to a specified object based on a specified comparison method.
		/// </summary>
		/// <param name="other">The object to compare with this instance.</param>
		/// <param name="comparer">An object that defines the method to use to evaluate whether the two objects are equal.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		///
		/// <remarks>
		/// <para>
		/// This member is an explicit interface member implementation. It can be used only when the
		///  <see cref="ValueTuple{T1, T2}"/> instance is cast to an <see cref="IStructuralEquatable"/> interface.
		/// </para>
		/// <para>
		/// The <see cref="IEqualityComparer.Equals(object,object)"/> implementation is called only if <c>other</c> is not <see langword="null"/>,
		///  and if it can be successfully cast (in C#) or converted (in Visual Basic) to a <see cref="ValueTuple{T1, T2}"/>
		///  whose components are of the same types as those of the current instance. The IStructuralEquatable.Equals(Object, IEqualityComparer) method
		///  first passes the <see cref="Item1"/> values of the <see cref="ValueTuple{T1, T2}"/> objects to be compared to the
		///  <see cref="IEqualityComparer.Equals(object,object)"/> implementation. If this method call returns <see langword="true"/>, the method is
		///  called again and passed the <see cref="Item2"/> values of the two <see cref="ValueTuple{T1, T2}"/> instances.
		/// </para>
		/// </remarks>
		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is ValueTuple<T1, T2>))
			{
				return false;
			}

			var objTuple = (ValueTuple<T1, T2>)other;

			return comparer.Equals(Item1, objTuple.Item1)
				   && comparer.Equals(Item2, objTuple.Item2);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		private int GetHashCodeCore(IEqualityComparer comparer)
		{
			return ValueTuple.CombineHashCodes
			(
				comparer.GetHashCode(Item1),
				comparer.GetHashCode(Item2)
			);
		}

		string ITupleInternal.ToStringEnd()
		{
			return (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ")";
		}
	}

	/// <summary>
	/// Represents a 3-tuple, or triple, as a value type.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
	[StructLayout(LayoutKind.Auto)]
	public struct ValueTuple<T1, T2, T3>
		: IEquatable<ValueTuple<T1, T2, T3>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2, T3>>, ITupleInternal
	{
		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3}"/> instance's first component.
		/// </summary>
		public T1 Item1;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3}"/> instance's second component.
		/// </summary>
		public T2 Item2;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3}"/> instance's third component.
		/// </summary>
		public T3 Item3;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3}"/> value type.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		/// <param name="item2">The value of the tuple's second component.</param>
		/// <param name="item3">The value of the tuple's third component.</param>
		public ValueTuple(T1 item1, T2 item2, T3 item3)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
		}

		int ITupleInternal.Size => 3;

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple<T1, T2, T3> other)
		{
			var c = Comparer<T1>.Default.Compare(Item1, other.Item1);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T2>.Default.Compare(Item2, other.Item2);
			return c != 0 ? c : Comparer<T3>.Default.Compare(Item3, other.Item3);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3}"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
		/// <list type="bullet">
		///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3}"/> value type.</description></item>
		///     <item><description>Its components are of the same types as those of the current instance.</description></item>
		///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
		/// </list>
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1, T2, T3> valueTuple && Equals(valueTuple);
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3}" />
		/// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3}" />.
		/// </summary>
		/// <param name="other">The tuple to compare with this instance.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified tuple; otherwise, <see langword="false" />.</returns>
		/// <remarks>
		/// The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its fields
		/// are equal to that of the current instance, using the default comparer for that field's type.
		/// </remarks>
		public bool Equals(ValueTuple<T1, T2, T3> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
				   && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
				   && EqualityComparer<T3>.Default.Equals(Item3, other.Item3);
		}

		/// <summary>
		/// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3}"/> instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return ValueTuple.CombineHashCodes
			(
				EqualityComparer<T1>.Default.GetHashCode(Item1),
				EqualityComparer<T2>.Default.GetHashCode(Item2),
				EqualityComparer<T3>.Default.GetHashCode(Item3)
			);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3}"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3}"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>(Item1, Item2, Item3)</c>.
		/// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
		/// </remarks>
		public override string ToString()
		{
			return "(" + (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ")";
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple<T1, T2, T3>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			return CompareTo((ValueTuple<T1, T2, T3>)obj);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple<T1, T2, T3>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			var objTuple = (ValueTuple<T1, T2, T3>)other;

			var c = comparer.Compare(Item1, objTuple.Item1);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item2, objTuple.Item2);
			return c != 0 ? c : comparer.Compare(Item3, objTuple.Item3);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is ValueTuple<T1, T2, T3>))
			{
				return false;
			}

			var objTuple = (ValueTuple<T1, T2, T3>)other;

			return comparer.Equals(Item1, objTuple.Item1)
				   && comparer.Equals(Item2, objTuple.Item2)
				   && comparer.Equals(Item3, objTuple.Item3);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		private int GetHashCodeCore(IEqualityComparer comparer)
		{
			return ValueTuple.CombineHashCodes
			(
				comparer.GetHashCode(Item1),
				comparer.GetHashCode(Item2),
				comparer.GetHashCode(Item3)
			);
		}

		string ITupleInternal.ToStringEnd()
		{
			return (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ")";
		}
	}

	/// <summary>
	/// Represents a 4-tuple, or quadruple, as a value type.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
	[StructLayout(LayoutKind.Auto)]
	public struct ValueTuple<T1, T2, T3, T4>
		: IEquatable<ValueTuple<T1, T2, T3, T4>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2, T3, T4>>, ITupleInternal
	{
		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance's first component.
		/// </summary>
		public T1 Item1;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance's second component.
		/// </summary>
		public T2 Item2;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance's third component.
		/// </summary>
		public T3 Item3;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance's fourth component.
		/// </summary>
		public T4 Item4;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4}"/> value type.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		/// <param name="item2">The value of the tuple's second component.</param>
		/// <param name="item3">The value of the tuple's third component.</param>
		/// <param name="item4">The value of the tuple's fourth component.</param>
		public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
		}

		int ITupleInternal.Size => 4;

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple<T1, T2, T3, T4> other)
		{
			var c = Comparer<T1>.Default.Compare(Item1, other.Item1);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T2>.Default.Compare(Item2, other.Item2);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T3>.Default.Compare(Item3, other.Item3);
			return c != 0 ? c : Comparer<T4>.Default.Compare(Item4, other.Item4);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
		/// <list type="bullet">
		///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4}"/> value type.</description></item>
		///     <item><description>Its components are of the same types as those of the current instance.</description></item>
		///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
		/// </list>
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1, T2, T3, T4> valueTuple && Equals(valueTuple);
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4}" />
		/// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4}" />.
		/// </summary>
		/// <param name="other">The tuple to compare with this instance.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified tuple; otherwise, <see langword="false" />.</returns>
		/// <remarks>
		/// The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its fields
		/// are equal to that of the current instance, using the default comparer for that field's type.
		/// </remarks>
		public bool Equals(ValueTuple<T1, T2, T3, T4> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
				   && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
				   && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
				   && EqualityComparer<T4>.Default.Equals(Item4, other.Item4);
		}

		/// <summary>
		/// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4}"/> instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return ValueTuple.CombineHashCodes
			(
				EqualityComparer<T1>.Default.GetHashCode(Item1),
				EqualityComparer<T2>.Default.GetHashCode(Item2),
				EqualityComparer<T3>.Default.GetHashCode(Item3),
				EqualityComparer<T4>.Default.GetHashCode(Item4)
			);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4}"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4}"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4)</c>.
		/// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
		/// </remarks>
		public override string ToString()
		{
			return "(" + (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ")";
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple<T1, T2, T3, T4>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			return CompareTo((ValueTuple<T1, T2, T3, T4>)obj);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple<T1, T2, T3, T4>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4>)other;

			var c = comparer.Compare(Item1, objTuple.Item1);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item2, objTuple.Item2);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item3, objTuple.Item3);
			return c != 0 ? c : comparer.Compare(Item4, objTuple.Item4);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is ValueTuple<T1, T2, T3, T4>))
			{
				return false;
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4>)other;

			return comparer.Equals(Item1, objTuple.Item1)
				   && comparer.Equals(Item2, objTuple.Item2)
				   && comparer.Equals(Item3, objTuple.Item3)
				   && comparer.Equals(Item4, objTuple.Item4);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		private int GetHashCodeCore(IEqualityComparer comparer)
		{
			return ValueTuple.CombineHashCodes
			(
				comparer.GetHashCode(Item1),
				comparer.GetHashCode(Item2),
				comparer.GetHashCode(Item3),
				comparer.GetHashCode(Item4)
			);
		}

		string ITupleInternal.ToStringEnd()
		{
			return (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ")";
		}
	}

	/// <summary>
	/// Represents a 5-tuple, or quintuple, as a value type.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
	/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
	[StructLayout(LayoutKind.Auto)]
	public struct ValueTuple<T1, T2, T3, T4, T5>
		: IEquatable<ValueTuple<T1, T2, T3, T4, T5>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5>>, ITupleInternal
	{
		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's first component.
		/// </summary>
		public T1 Item1;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's second component.
		/// </summary>
		public T2 Item2;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's third component.
		/// </summary>
		public T3 Item3;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's fourth component.
		/// </summary>
		public T4 Item4;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance's fifth component.
		/// </summary>
		public T5 Item5;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> value type.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		/// <param name="item2">The value of the tuple's second component.</param>
		/// <param name="item3">The value of the tuple's third component.</param>
		/// <param name="item4">The value of the tuple's fourth component.</param>
		/// <param name="item5">The value of the tuple's fifth component.</param>
		public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
		}

		int ITupleInternal.Size => 5;

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple<T1, T2, T3, T4, T5> other)
		{
			var c = Comparer<T1>.Default.Compare(Item1, other.Item1);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T2>.Default.Compare(Item2, other.Item2);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T3>.Default.Compare(Item3, other.Item3);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T4>.Default.Compare(Item4, other.Item4);
			return c != 0 ? c : Comparer<T5>.Default.Compare(Item5, other.Item5);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
		/// <list type="bullet">
		///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> value type.</description></item>
		///     <item><description>Its components are of the same types as those of the current instance.</description></item>
		///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
		/// </list>
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1, T2, T3, T4, T5> valueTuple && Equals(valueTuple);
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5}" />
		/// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5}" />.
		/// </summary>
		/// <param name="other">The tuple to compare with this instance.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified tuple; otherwise, <see langword="false" />.</returns>
		/// <remarks>
		/// The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its fields
		/// are equal to that of the current instance, using the default comparer for that field's type.
		/// </remarks>
		public bool Equals(ValueTuple<T1, T2, T3, T4, T5> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
				   && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
				   && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
				   && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
				   && EqualityComparer<T5>.Default.Equals(Item5, other.Item5);
		}

		/// <summary>
		/// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return ValueTuple.CombineHashCodes
			(
				EqualityComparer<T1>.Default.GetHashCode(Item1),
				EqualityComparer<T2>.Default.GetHashCode(Item2),
				EqualityComparer<T3>.Default.GetHashCode(Item3),
				EqualityComparer<T4>.Default.GetHashCode(Item4),
				EqualityComparer<T5>.Default.GetHashCode(Item5)
			);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5}"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5)</c>.
		/// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
		/// </remarks>
		public override string ToString()
		{
			return "(" + (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ", " + (Item5 == null ? string.Empty : Item5.ToString()) + ")";
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple<T1, T2, T3, T4, T5>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			return CompareTo((ValueTuple<T1, T2, T3, T4, T5>)obj);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple<T1, T2, T3, T4, T5>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4, T5>)other;

			var c = comparer.Compare(Item1, objTuple.Item1);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item2, objTuple.Item2);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item3, objTuple.Item3);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item4, objTuple.Item4);
			return c != 0 ? c : comparer.Compare(Item5, objTuple.Item5);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is ValueTuple<T1, T2, T3, T4, T5>))
			{
				return false;
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4, T5>)other;

			return comparer.Equals(Item1, objTuple.Item1)
				   && comparer.Equals(Item2, objTuple.Item2)
				   && comparer.Equals(Item3, objTuple.Item3)
				   && comparer.Equals(Item4, objTuple.Item4)
				   && comparer.Equals(Item5, objTuple.Item5);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		private int GetHashCodeCore(IEqualityComparer comparer)
		{
			return ValueTuple.CombineHashCodes
			(
				comparer.GetHashCode(Item1),
				comparer.GetHashCode(Item2),
				comparer.GetHashCode(Item3),
				comparer.GetHashCode(Item4),
				comparer.GetHashCode(Item5)
			);
		}

		string ITupleInternal.ToStringEnd()
		{
			return (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ", " + (Item5 == null ? string.Empty : Item5.ToString()) + ")";
		}
	}

	/// <summary>
	/// Represents a 6-tuple, or sextuple, as a value type.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
	/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
	/// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
	[StructLayout(LayoutKind.Auto)]
	public struct ValueTuple<T1, T2, T3, T4, T5, T6>
		: IEquatable<ValueTuple<T1, T2, T3, T4, T5, T6>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5, T6>>, ITupleInternal
	{
		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's first component.
		/// </summary>
		public T1 Item1;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's second component.
		/// </summary>
		public T2 Item2;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's third component.
		/// </summary>
		public T3 Item3;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's fourth component.
		/// </summary>
		public T4 Item4;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's fifth component.
		/// </summary>
		public T5 Item5;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance's sixth component.
		/// </summary>
		public T6 Item6;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> value type.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		/// <param name="item2">The value of the tuple's second component.</param>
		/// <param name="item3">The value of the tuple's third component.</param>
		/// <param name="item4">The value of the tuple's fourth component.</param>
		/// <param name="item5">The value of the tuple's fifth component.</param>
		/// <param name="item6">The value of the tuple's sixth component.</param>
		public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
		}

		int ITupleInternal.Size => 6;

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple<T1, T2, T3, T4, T5, T6> other)
		{
			var c = Comparer<T1>.Default.Compare(Item1, other.Item1);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T2>.Default.Compare(Item2, other.Item2);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T3>.Default.Compare(Item3, other.Item3);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T4>.Default.Compare(Item4, other.Item4);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T5>.Default.Compare(Item5, other.Item5);
			return c != 0 ? c : Comparer<T6>.Default.Compare(Item6, other.Item6);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
		/// <list type="bullet">
		///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> value type.</description></item>
		///     <item><description>Its components are of the same types as those of the current instance.</description></item>
		///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
		/// </list>
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1, T2, T3, T4, T5, T6> valueTuple && Equals(valueTuple);
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}" />
		/// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}" />.
		/// </summary>
		/// <param name="other">The tuple to compare with this instance.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified tuple; otherwise, <see langword="false" />.</returns>
		/// <remarks>
		/// The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its fields
		/// are equal to that of the current instance, using the default comparer for that field's type.
		/// </remarks>
		public bool Equals(ValueTuple<T1, T2, T3, T4, T5, T6> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
				   && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
				   && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
				   && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
				   && EqualityComparer<T5>.Default.Equals(Item5, other.Item5)
				   && EqualityComparer<T6>.Default.Equals(Item6, other.Item6);
		}

		/// <summary>
		/// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return ValueTuple.CombineHashCodes
			(
				EqualityComparer<T1>.Default.GetHashCode(Item1),
				EqualityComparer<T2>.Default.GetHashCode(Item2),
				EqualityComparer<T3>.Default.GetHashCode(Item3),
				EqualityComparer<T4>.Default.GetHashCode(Item4),
				EqualityComparer<T5>.Default.GetHashCode(Item5),
				EqualityComparer<T6>.Default.GetHashCode(Item6)
			);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5, Item6)</c>.
		/// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
		/// </remarks>
		public override string ToString()
		{
			return "(" + (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ", " + (Item5 == null ? string.Empty : Item5.ToString()) + ", " + (Item6 == null ? string.Empty : Item6.ToString()) + ")";
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple<T1, T2, T3, T4, T5, T6>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			return CompareTo((ValueTuple<T1, T2, T3, T4, T5, T6>)obj);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple<T1, T2, T3, T4, T5, T6>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4, T5, T6>)other;

			var c = comparer.Compare(Item1, objTuple.Item1);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item2, objTuple.Item2);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item3, objTuple.Item3);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item4, objTuple.Item4);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item5, objTuple.Item5);
			return c != 0 ? c : comparer.Compare(Item6, objTuple.Item6);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is ValueTuple<T1, T2, T3, T4, T5, T6>))
			{
				return false;
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4, T5, T6>)other;

			return comparer.Equals(Item1, objTuple.Item1)
				   && comparer.Equals(Item2, objTuple.Item2)
				   && comparer.Equals(Item3, objTuple.Item3)
				   && comparer.Equals(Item4, objTuple.Item4)
				   && comparer.Equals(Item5, objTuple.Item5)
				   && comparer.Equals(Item6, objTuple.Item6);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		private int GetHashCodeCore(IEqualityComparer comparer)
		{
			return ValueTuple.CombineHashCodes
			(
				comparer.GetHashCode(Item1),
				comparer.GetHashCode(Item2),
				comparer.GetHashCode(Item3),
				comparer.GetHashCode(Item4),
				comparer.GetHashCode(Item5),
				comparer.GetHashCode(Item6)
			);
		}

		string ITupleInternal.ToStringEnd()
		{
			return (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ", " + (Item5 == null ? string.Empty : Item5.ToString()) + ", " + (Item6 == null ? string.Empty : Item6.ToString()) + ")";
		}
	}

	/// <summary>
	/// Represents a 7-tuple, or septuple, as a value type.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
	/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
	/// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
	/// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
	[StructLayout(LayoutKind.Auto)]
	public struct ValueTuple<T1, T2, T3, T4, T5, T6, T7>
		: IEquatable<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>, ITupleInternal
	{
		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's first component.
		/// </summary>
		public T1 Item1;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's second component.
		/// </summary>
		public T2 Item2;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's third component.
		/// </summary>
		public T3 Item3;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's fourth component.
		/// </summary>
		public T4 Item4;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's fifth component.
		/// </summary>
		public T5 Item5;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's sixth component.
		/// </summary>
		public T6 Item6;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance's seventh component.
		/// </summary>
		public T7 Item7;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> value type.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		/// <param name="item2">The value of the tuple's second component.</param>
		/// <param name="item3">The value of the tuple's third component.</param>
		/// <param name="item4">The value of the tuple's fourth component.</param>
		/// <param name="item5">The value of the tuple's fifth component.</param>
		/// <param name="item6">The value of the tuple's sixth component.</param>
		/// <param name="item7">The value of the tuple's seventh component.</param>
		public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
		{
			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
		}

		int ITupleInternal.Size => 7;

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple<T1, T2, T3, T4, T5, T6, T7> other)
		{
			var c = Comparer<T1>.Default.Compare(Item1, other.Item1);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T2>.Default.Compare(Item2, other.Item2);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T3>.Default.Compare(Item3, other.Item3);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T4>.Default.Compare(Item4, other.Item4);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T5>.Default.Compare(Item5, other.Item5);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T6>.Default.Compare(Item6, other.Item6);
			return c != 0 ? c : Comparer<T7>.Default.Compare(Item7, other.Item7);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
		/// <list type="bullet">
		///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> value type.</description></item>
		///     <item><description>Its components are of the same types as those of the current instance.</description></item>
		///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
		/// </list>
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1, T2, T3, T4, T5, T6, T7> valueTuple && Equals(valueTuple);
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}" />
		/// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}" />.
		/// </summary>
		/// <param name="other">The tuple to compare with this instance.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified tuple; otherwise, <see langword="false" />.</returns>
		/// <remarks>
		/// The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its fields
		/// are equal to that of the current instance, using the default comparer for that field's type.
		/// </remarks>
		public bool Equals(ValueTuple<T1, T2, T3, T4, T5, T6, T7> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
				   && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
				   && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
				   && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
				   && EqualityComparer<T5>.Default.Equals(Item5, other.Item5)
				   && EqualityComparer<T6>.Default.Equals(Item6, other.Item6)
				   && EqualityComparer<T7>.Default.Equals(Item7, other.Item7);
		}

		/// <summary>
		/// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return ValueTuple.CombineHashCodes
			(
				EqualityComparer<T1>.Default.GetHashCode(Item1),
				EqualityComparer<T2>.Default.GetHashCode(Item2),
				EqualityComparer<T3>.Default.GetHashCode(Item3),
				EqualityComparer<T4>.Default.GetHashCode(Item4),
				EqualityComparer<T5>.Default.GetHashCode(Item5),
				EqualityComparer<T6>.Default.GetHashCode(Item6),
				EqualityComparer<T7>.Default.GetHashCode(Item7)
			);
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5, Item6, Item7)</c>.
		/// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
		/// </remarks>
		public override string ToString()
		{
			return "(" + (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ", " + (Item5 == null ? string.Empty : Item5.ToString()) + ", " + (Item6 == null ? string.Empty : Item6.ToString()) + ", " + (Item7 == null ? string.Empty : Item7.ToString()) + ")";
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple<T1, T2, T3, T4, T5, T6, T7>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			return CompareTo((ValueTuple<T1, T2, T3, T4, T5, T6, T7>)obj);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple<T1, T2, T3, T4, T5, T6, T7>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4, T5, T6, T7>)other;

			var c = comparer.Compare(Item1, objTuple.Item1);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item2, objTuple.Item2);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item3, objTuple.Item3);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item4, objTuple.Item4);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item5, objTuple.Item5);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item6, objTuple.Item6);
			return c != 0 ? c : comparer.Compare(Item7, objTuple.Item7);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is ValueTuple<T1, T2, T3, T4, T5, T6, T7>))
			{
				return false;
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4, T5, T6, T7>)other;

			return comparer.Equals(Item1, objTuple.Item1)
				   && comparer.Equals(Item2, objTuple.Item2)
				   && comparer.Equals(Item3, objTuple.Item3)
				   && comparer.Equals(Item4, objTuple.Item4)
				   && comparer.Equals(Item5, objTuple.Item5)
				   && comparer.Equals(Item6, objTuple.Item6)
				   && comparer.Equals(Item7, objTuple.Item7);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		private int GetHashCodeCore(IEqualityComparer comparer)
		{
			return ValueTuple.CombineHashCodes
			(
				comparer.GetHashCode(Item1),
				comparer.GetHashCode(Item2),
				comparer.GetHashCode(Item3),
				comparer.GetHashCode(Item4),
				comparer.GetHashCode(Item5),
				comparer.GetHashCode(Item6),
				comparer.GetHashCode(Item7)
			);
		}

		string ITupleInternal.ToStringEnd()
		{
			return (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ", " + (Item5 == null ? string.Empty : Item5.ToString()) + ", " + (Item6 == null ? string.Empty : Item6.ToString()) + ", " + (Item7 == null ? string.Empty : Item7.ToString()) + ")";
		}
	}

	/// <summary>
	/// Represents an 8-tuple, or octuple, as a value type.
	/// </summary>
	/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
	/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
	/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
	/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
	/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
	/// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
	/// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
	/// <typeparam name="TRest">The type of the tuple's eighth component.</typeparam>
	[StructLayout(LayoutKind.Auto)]
	public struct ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>
		: IEquatable<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>, ITupleInternal
		where TRest : struct
	{
		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's first component.
		/// </summary>
		public T1 Item1;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's second component.
		/// </summary>
		public T2 Item2;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's third component.
		/// </summary>
		public T3 Item3;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's fourth component.
		/// </summary>
		public T4 Item4;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's fifth component.
		/// </summary>
		public T5 Item5;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's sixth component.
		/// </summary>
		public T6 Item6;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's seventh component.
		/// </summary>
		public T7 Item7;

		/// <summary>
		/// The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance's eighth component.
		/// </summary>
		public TRest Rest;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> value type.
		/// </summary>
		/// <param name="item1">The value of the tuple's first component.</param>
		/// <param name="item2">The value of the tuple's second component.</param>
		/// <param name="item3">The value of the tuple's third component.</param>
		/// <param name="item4">The value of the tuple's fourth component.</param>
		/// <param name="item5">The value of the tuple's fifth component.</param>
		/// <param name="item6">The value of the tuple's sixth component.</param>
		/// <param name="item7">The value of the tuple's seventh component.</param>
		/// <param name="rest">The value of the tuple's eight component.</param>
		public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
		{
			if (!(rest is ITupleInternal))
			{
				throw new ArgumentException("The TRest type argument of ValueTuple`8 must be a ValueTuple.");
			}

			Item1 = item1;
			Item2 = item2;
			Item3 = item3;
			Item4 = item4;
			Item5 = item5;
			Item6 = item6;
			Item7 = item7;
			Rest = rest;
		}

		int ITupleInternal.Size => !(Rest is ITupleInternal rest) ? 8 : 7 + rest.Size;

		/// <inheritdoc />
		/// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
		/// <param name="other">An instance to compare.</param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and <paramref name="other" />.
		/// Returns less than zero if this instance is less than <paramref name="other" />, zero if this
		/// instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
		/// than <paramref name="other" />.
		/// </returns>
		public int CompareTo(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> other)
		{
			var c = Comparer<T1>.Default.Compare(Item1, other.Item1);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T2>.Default.Compare(Item2, other.Item2);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T3>.Default.Compare(Item3, other.Item3);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T4>.Default.Compare(Item4, other.Item4);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T5>.Default.Compare(Item5, other.Item5);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T6>.Default.Compare(Item6, other.Item6);
			if (c != 0)
			{
				return c;
			}

			c = Comparer<T7>.Default.Compare(Item7, other.Item7);
			return c != 0 ? c : Comparer<TRest>.Default.Compare(Rest, other.Rest);
		}

		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance is equal to a specified object.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true"/> if the current instance is equal to the specified object; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// The <paramref name="obj"/> parameter is considered to be equal to the current instance under the following conditions:
		/// <list type="bullet">
		///     <item><description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> value type.</description></item>
		///     <item><description>Its components are of the same types as those of the current instance.</description></item>
		///     <item><description>Its components are equal to those of the current instance. Equality is determined by the default object equality comparer for each component.</description></item>
		/// </list>
		/// </remarks>
		public override bool Equals(object obj)
		{
			return obj is ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple && Equals(tuple);
		}

		/// <inheritdoc />
		/// <summary>
		/// Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" />
		/// instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" />.
		/// </summary>
		/// <param name="other">The tuple to compare with this instance.</param>
		/// <returns><see langword="true" /> if the current instance is equal to the specified tuple; otherwise, <see langword="false" />.</returns>
		/// <remarks>
		/// The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its fields
		/// are equal to that of the current instance, using the default comparer for that field's type.
		/// </remarks>
		public bool Equals(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> other)
		{
			return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
				   && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
				   && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
				   && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
				   && EqualityComparer<T5>.Default.Equals(Item5, other.Item5)
				   && EqualityComparer<T6>.Default.Equals(Item6, other.Item6)
				   && EqualityComparer<T7>.Default.Equals(Item7, other.Item7)
				   && EqualityComparer<TRest>.Default.Equals(Rest, other.Rest);
		}

		/// <summary>
		/// Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			// We want to have a limited hash in this case.  We'll use the last 8 elements of the tuple
			if (!(Rest is ITupleInternal rest))
			{
				return ValueTuple.CombineHashCodes
				(
					EqualityComparer<T1>.Default.GetHashCode(Item1),
					EqualityComparer<T2>.Default.GetHashCode(Item2),
					EqualityComparer<T3>.Default.GetHashCode(Item3),
					EqualityComparer<T4>.Default.GetHashCode(Item4),
					EqualityComparer<T5>.Default.GetHashCode(Item5),
					EqualityComparer<T6>.Default.GetHashCode(Item6),
					EqualityComparer<T7>.Default.GetHashCode(Item7)
				);
			}

			var size = rest.Size;
			if (size >= 8)
			{
				return rest.GetHashCode();
			}

			// In this case, the rest member has less than 8 elements so we need to combine some our elements with the elements in rest
			switch (8 - size)
			{
			case 1:
				return ValueTuple.CombineHashCodes
				(
					EqualityComparer<T7>.Default.GetHashCode(Item7),
					rest.GetHashCode()
				);

			case 2:
				return ValueTuple.CombineHashCodes
				(
					EqualityComparer<T6>.Default.GetHashCode(Item6),
					EqualityComparer<T7>.Default.GetHashCode(Item7),
					rest.GetHashCode()
				);

			case 3:
				return ValueTuple.CombineHashCodes
				(
					EqualityComparer<T5>.Default.GetHashCode(Item5),
					EqualityComparer<T6>.Default.GetHashCode(Item6),
					EqualityComparer<T7>.Default.GetHashCode(Item7),
					rest.GetHashCode()
				);

			case 4:
				return ValueTuple.CombineHashCodes
				(
					EqualityComparer<T4>.Default.GetHashCode(Item4),
					EqualityComparer<T5>.Default.GetHashCode(Item5),
					EqualityComparer<T6>.Default.GetHashCode(Item6),
					EqualityComparer<T7>.Default.GetHashCode(Item7),
					rest.GetHashCode()
				);

			case 5:
				return ValueTuple.CombineHashCodes
				(
					EqualityComparer<T3>.Default.GetHashCode(Item3),
					EqualityComparer<T4>.Default.GetHashCode(Item4),
					EqualityComparer<T5>.Default.GetHashCode(Item5),
					EqualityComparer<T6>.Default.GetHashCode(Item6),
					EqualityComparer<T7>.Default.GetHashCode(Item7),
					rest.GetHashCode()
				);

			case 6:
				return ValueTuple.CombineHashCodes
				(
					EqualityComparer<T2>.Default.GetHashCode(Item2),
					EqualityComparer<T3>.Default.GetHashCode(Item3),
					EqualityComparer<T4>.Default.GetHashCode(Item4),
					EqualityComparer<T5>.Default.GetHashCode(Item5),
					EqualityComparer<T6>.Default.GetHashCode(Item6),
					EqualityComparer<T7>.Default.GetHashCode(Item7),
					rest.GetHashCode()
				);

			case 7:
			case 8:
				return ValueTuple.CombineHashCodes
				(
					EqualityComparer<T1>.Default.GetHashCode(Item1),
					EqualityComparer<T2>.Default.GetHashCode(Item2),
					EqualityComparer<T3>.Default.GetHashCode(Item3),
					EqualityComparer<T4>.Default.GetHashCode(Item4),
					EqualityComparer<T5>.Default.GetHashCode(Item5),
					EqualityComparer<T6>.Default.GetHashCode(Item6),
					EqualityComparer<T7>.Default.GetHashCode(Item7),
					rest.GetHashCode()
				);

			default:
				Debug.Fail("Missed all cases for computing ValueTuple hash code");
				return -1;
			}
		}

		/// <summary>
		/// Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance.
		/// </summary>
		/// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> instance.</returns>
		/// <remarks>
		/// The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5, Item6, Item7, Rest)</c>.
		/// If any field value is <see langword="null"/>, it is represented as <see cref="string.Empty"/>.
		/// </remarks>
		public override string ToString()
		{
			return !(Rest is ITupleInternal rest) ? $"({(Item1 == null ? string.Empty : Item1.ToString())}, {(Item2 == null ? string.Empty : Item2.ToString())}, {(Item3 == null ? string.Empty : Item3.ToString())}, {(Item4 == null ? string.Empty : Item4.ToString())}, {(Item5 == null ? string.Empty : Item5.ToString())}, {(Item6 == null ? string.Empty : Item6.ToString())}, {(Item7 == null ? string.Empty : Item7.ToString())}, {Rest})" : "(" + (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ", " + (Item5 == null ? string.Empty : Item5.ToString()) + ", " + (Item6 == null ? string.Empty : Item6.ToString()) + ", " + (Item7 == null ? string.Empty : Item7.ToString()) + ", " + rest.ToStringEnd();
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj == null)
			{
				return 1;
			}

			if (!(obj is ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(obj));
			}

			return CompareTo((ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>)obj);
		}

		int IStructuralComparable.CompareTo(object other, IComparer comparer)
		{
			if (other == null)
			{
				return 1;
			}

			if (!(other is ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>))
			{
				throw new ArgumentException("The parameter should be a ValueTuple type of appropriate arity.", nameof(other));
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>)other;

			var c = comparer.Compare(Item1, objTuple.Item1);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item2, objTuple.Item2);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item3, objTuple.Item3);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item4, objTuple.Item4);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item5, objTuple.Item5);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item6, objTuple.Item6);
			if (c != 0)
			{
				return c;
			}

			c = comparer.Compare(Item7, objTuple.Item7);
			return c != 0 ? c : comparer.Compare(Rest, objTuple.Rest);
		}

		bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
		{
			if (other == null || !(other is ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>))
			{
				return false;
			}

			var objTuple = (ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>)other;

			return comparer.Equals(Item1, objTuple.Item1)
				   && comparer.Equals(Item2, objTuple.Item2)
				   && comparer.Equals(Item3, objTuple.Item3)
				   && comparer.Equals(Item4, objTuple.Item4)
				   && comparer.Equals(Item5, objTuple.Item5)
				   && comparer.Equals(Item6, objTuple.Item6)
				   && comparer.Equals(Item7, objTuple.Item7)
				   && comparer.Equals(Rest, objTuple.Rest);
		}

		int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		int ITupleInternal.GetHashCode(IEqualityComparer comparer)
		{
			return GetHashCodeCore(comparer);
		}

		private int GetHashCodeCore(IEqualityComparer comparer)
		{
			// We want to have a limited hash in this case.  We'll use the last 8 elements of the tuple
			if (!(Rest is ITupleInternal rest))
			{
				return ValueTuple.CombineHashCodes
				(
					comparer.GetHashCode(Item1), comparer.GetHashCode(Item2), comparer.GetHashCode(Item3),
					comparer.GetHashCode(Item4), comparer.GetHashCode(Item5), comparer.GetHashCode(Item6),
					comparer.GetHashCode(Item7)
				);
			}

			var size = rest.Size;
			if (size >= 8)
			{
				return rest.GetHashCode(comparer);
			}

			// In this case, the rest member has less than 8 elements so we need to combine some our elements with the elements in rest
			switch (8 - size)
			{
			case 1:
				return ValueTuple.CombineHashCodes(comparer.GetHashCode(Item7), rest.GetHashCode(comparer));

			case 2:
				return ValueTuple.CombineHashCodes(comparer.GetHashCode(Item6), comparer.GetHashCode(Item7), rest.GetHashCode(comparer));

			case 3:
				return ValueTuple.CombineHashCodes
				(
					comparer.GetHashCode(Item5), comparer.GetHashCode(Item6), comparer.GetHashCode(Item7),
					rest.GetHashCode(comparer)
				);

			case 4:
				return ValueTuple.CombineHashCodes
				(
					comparer.GetHashCode(Item4), comparer.GetHashCode(Item5), comparer.GetHashCode(Item6),
					comparer.GetHashCode(Item7), rest.GetHashCode(comparer)
				);

			case 5:
				return ValueTuple.CombineHashCodes
				(
					comparer.GetHashCode(Item3), comparer.GetHashCode(Item4), comparer.GetHashCode(Item5),
					comparer.GetHashCode(Item6), comparer.GetHashCode(Item7), rest.GetHashCode(comparer)
				);

			case 6:
				return ValueTuple.CombineHashCodes
				(
					comparer.GetHashCode(Item2), comparer.GetHashCode(Item3), comparer.GetHashCode(Item4),
					comparer.GetHashCode(Item5), comparer.GetHashCode(Item6), comparer.GetHashCode(Item7),
					rest.GetHashCode(comparer)
				);

			case 7:
			case 8:
				return ValueTuple.CombineHashCodes
				(
					comparer.GetHashCode(Item1), comparer.GetHashCode(Item2), comparer.GetHashCode(Item3),
					comparer.GetHashCode(Item4), comparer.GetHashCode(Item5), comparer.GetHashCode(Item6),
					comparer.GetHashCode(Item7), rest.GetHashCode(comparer)
				);

			default:
				Debug.Fail("Missed all cases for computing ValueTuple hash code");
				return -1;
			}
		}

		string ITupleInternal.ToStringEnd()
		{
			return !(Rest is ITupleInternal rest) ? $"{(Item1 == null ? string.Empty : Item1.ToString())}, {(Item2 == null ? string.Empty : Item2.ToString())}, {(Item3 == null ? string.Empty : Item3.ToString())}, {(Item4 == null ? string.Empty : Item4.ToString())}, {(Item5 == null ? string.Empty : Item5.ToString())}, {(Item6 == null ? string.Empty : Item6.ToString())}, {(Item7 == null ? string.Empty : Item7.ToString())}, {Rest})" : (Item1 == null ? string.Empty : Item1.ToString()) + ", " + (Item2 == null ? string.Empty : Item2.ToString()) + ", " + (Item3 == null ? string.Empty : Item3.ToString()) + ", " + (Item4 == null ? string.Empty : Item4.ToString()) + ", " + (Item5 == null ? string.Empty : Item5.ToString()) + ", " + (Item6 == null ? string.Empty : Item6.ToString()) + ", " + (Item7 == null ? string.Empty : Item7.ToString()) + ", " + rest.ToStringEnd();
		}
	}

	/// <summary>
	///     Provides extension methods for <see cref="Tuple" /> instances to interop with C# tuples features (deconstruction
	///     syntax, converting from and to <see cref="ValueTuple" />).
	/// </summary>
	public static class TupleExtensions
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1>(
			this Tuple<T1> value,
			out T1 item1)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2>(
			this Tuple<T1, T2> value,
			out T1 item1, out T2 item2)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3>(
			this Tuple<T1, T2, T3> value,
			out T1 item1, out T2 item2, out T3 item3)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4>(
			this Tuple<T1, T2, T3, T4> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5>(
			this Tuple<T1, T2, T3, T4, T5> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6>(
			this Tuple<T1, T2, T3, T4, T5, T6> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13, out T14 item14)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
			item14 = value.Rest.Item7;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15>>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13, out T14 item14, out T15 item15)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
			item14 = value.Rest.Item7;
			item15 = value.Rest.Rest.Item1;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16>>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13, out T14 item14, out T15 item15, out T16 item16)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
			item14 = value.Rest.Item7;
			item15 = value.Rest.Rest.Item1;
			item16 = value.Rest.Rest.Item2;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17>>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13, out T14 item14, out T15 item15, out T16 item16, out T17 item17)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
			item14 = value.Rest.Item7;
			item15 = value.Rest.Rest.Item1;
			item16 = value.Rest.Rest.Item2;
			item17 = value.Rest.Rest.Item3;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18>>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13, out T14 item14, out T15 item15, out T16 item16, out T17 item17, out T18 item18)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
			item14 = value.Rest.Item7;
			item15 = value.Rest.Rest.Item1;
			item16 = value.Rest.Rest.Item2;
			item17 = value.Rest.Rest.Item3;
			item18 = value.Rest.Rest.Item4;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19>>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13, out T14 item14, out T15 item15, out T16 item16, out T17 item17, out T18 item18, out T19 item19)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
			item14 = value.Rest.Item7;
			item15 = value.Rest.Rest.Item1;
			item16 = value.Rest.Rest.Item2;
			item17 = value.Rest.Rest.Item3;
			item18 = value.Rest.Rest.Item4;
			item19 = value.Rest.Rest.Item5;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19, T20>>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13, out T14 item14, out T15 item15, out T16 item16, out T17 item17, out T18 item18, out T19 item19, out T20 item20)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
			item14 = value.Rest.Item7;
			item15 = value.Rest.Rest.Item1;
			item16 = value.Rest.Rest.Item2;
			item17 = value.Rest.Rest.Item3;
			item18 = value.Rest.Rest.Item4;
			item19 = value.Rest.Rest.Item5;
			item20 = value.Rest.Rest.Item6;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Deconstruct<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>(
			this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19, T20, T21>>> value,
			out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out T8 item8, out T9 item9, out T10 item10, out T11 item11, out T12 item12, out T13 item13, out T14 item14, out T15 item15, out T16 item16, out T17 item17, out T18 item18, out T19 item19, out T20 item20, out T21 item21)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			item1 = value.Item1;
			item2 = value.Item2;
			item3 = value.Item3;
			item4 = value.Item4;
			item5 = value.Item5;
			item6 = value.Item6;
			item7 = value.Item7;
			item8 = value.Rest.Item1;
			item9 = value.Rest.Item2;
			item10 = value.Rest.Item3;
			item11 = value.Rest.Item4;
			item12 = value.Rest.Item5;
			item13 = value.Rest.Item6;
			item14 = value.Rest.Item7;
			item15 = value.Rest.Rest.Item1;
			item16 = value.Rest.Rest.Item2;
			item17 = value.Rest.Rest.Item3;
			item18 = value.Rest.Rest.Item4;
			item19 = value.Rest.Rest.Item5;
			item20 = value.Rest.Rest.Item6;
			item21 = value.Rest.Rest.Item7;
		}

		public static Tuple<T1>
			ToTuple<T1>(
				this ValueTuple<T1> value)
		{
			return Tuple.Create(value.Item1);
		}

		public static Tuple<T1, T2>
			ToTuple<T1, T2>(
				this ValueTuple<T1, T2> value)
		{
			return Tuple.Create(value.Item1, value.Item2);
		}

		public static Tuple<T1, T2, T3>
			ToTuple<T1, T2, T3>(
				this ValueTuple<T1, T2, T3> value)
		{
			return Tuple.Create(value.Item1, value.Item2, value.Item3);
		}

		public static Tuple<T1, T2, T3, T4>
			ToTuple<T1, T2, T3, T4>(
				this ValueTuple<T1, T2, T3, T4> value)
		{
			return Tuple.Create(value.Item1, value.Item2, value.Item3, value.Item4);
		}

		public static Tuple<T1, T2, T3, T4, T5>
			ToTuple<T1, T2, T3, T4, T5>(
				this ValueTuple<T1, T2, T3, T4, T5> value)
		{
			return Tuple.Create(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6>
			ToTuple<T1, T2, T3, T4, T5, T6>(
				this ValueTuple<T1, T2, T3, T4, T5, T6> value)
		{
			return Tuple.Create(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7>
			ToTuple<T1, T2, T3, T4, T5, T6, T7>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7> value)
		{
			return Tuple.Create(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				Tuple.Create(value.Rest.Item1)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				Tuple.Create(value.Rest.Item1, value.Rest.Item2)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				Tuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				Tuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				Tuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				Tuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				Tuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15>>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15>>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLongRef
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					Tuple.Create(value.Rest.Rest.Item1)
				)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16>>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16>>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLongRef
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					Tuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2)
				)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17>>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17>>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLongRef
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					Tuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3)
				)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18>>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17, T18>>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLongRef
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					Tuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3, value.Rest.Rest.Item4)
				)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19>>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17, T18, T19>>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLongRef
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					Tuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3, value.Rest.Rest.Item4, value.Rest.Rest.Item5)
				)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19, T20>>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17, T18, T19, T20>>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLongRef
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					Tuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3, value.Rest.Rest.Item4, value.Rest.Rest.Item5, value.Rest.Rest.Item6)
				)
			);
		}

		public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19, T20, T21>>>
			ToTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>(
				this ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17, T18, T19, T20, T21>>> value)
		{
			return CreateLongRef
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLongRef
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					Tuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3, value.Rest.Rest.Item4, value.Rest.Rest.Item5, value.Rest.Rest.Item6, value.Rest.Rest.Item7)
				)
			);
		}

		public static ValueTuple<T1>
			ToValueTuple<T1>(
				this Tuple<T1> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return ValueTuple.Create(value.Item1);
		}

		public static ValueTuple<T1, T2>
			ToValueTuple<T1, T2>(
				this Tuple<T1, T2> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return ValueTuple.Create(value.Item1, value.Item2);
		}

		public static ValueTuple<T1, T2, T3>
			ToValueTuple<T1, T2, T3>(
				this Tuple<T1, T2, T3> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return ValueTuple.Create(value.Item1, value.Item2, value.Item3);
		}

		public static ValueTuple<T1, T2, T3, T4>
			ToValueTuple<T1, T2, T3, T4>(
				this Tuple<T1, T2, T3, T4> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return ValueTuple.Create(value.Item1, value.Item2, value.Item3, value.Item4);
		}

		public static ValueTuple<T1, T2, T3, T4, T5>
			ToValueTuple<T1, T2, T3, T4, T5>(
				this Tuple<T1, T2, T3, T4, T5> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return ValueTuple.Create(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6>
			ToValueTuple<T1, T2, T3, T4, T5, T6>(
				this Tuple<T1, T2, T3, T4, T5, T6> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return ValueTuple.Create(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return ValueTuple.Create(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				ValueTuple.Create(value.Rest.Item1)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				ValueTuple.Create(value.Rest.Item1, value.Rest.Item2)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				ValueTuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				ValueTuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				ValueTuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				ValueTuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				ValueTuple.Create(value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15>>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15>>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLong
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					ValueTuple.Create(value.Rest.Rest.Item1)
				)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16>>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16>>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLong
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					ValueTuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2)
				)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17>>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17>>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLong
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					ValueTuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3)
				)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17, T18>>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18>>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLong
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					ValueTuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3, value.Rest.Rest.Item4)
				)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17, T18, T19>>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19>>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLong
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					ValueTuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3, value.Rest.Rest.Item4, value.Rest.Rest.Item5)
				)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17, T18, T19, T20>>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19, T20>>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLong
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					ValueTuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3, value.Rest.Rest.Item4, value.Rest.Rest.Item5, value.Rest.Rest.Item6)
				)
			);
		}

		public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8, T9, T10, T11, T12, T13, T14, ValueTuple<T15, T16, T17, T18, T19, T20, T21>>>
			ToValueTuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>(
				this Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8, T9, T10, T11, T12, T13, T14, Tuple<T15, T16, T17, T18, T19, T20, T21>>> value)
		{
			if (value == null)
			{
				throw new NullReferenceException(nameof(value));
			}
			return CreateLong
			(
				value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7,
				CreateLong
				(
					value.Rest.Item1, value.Rest.Item2, value.Rest.Item3, value.Rest.Item4, value.Rest.Item5, value.Rest.Item6, value.Rest.Item7,
					ValueTuple.Create(value.Rest.Rest.Item1, value.Rest.Rest.Item2, value.Rest.Rest.Item3, value.Rest.Rest.Item4, value.Rest.Rest.Item5, value.Rest.Rest.Item6, value.Rest.Rest.Item7)
				)
			);
		}

		private static ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> CreateLong<T1, T2, T3, T4, T5, T6, T7, TRest>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
			where TRest : struct
		{
			return new ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(item1, item2, item3, item4, item5, item6, item7, rest);
		}

		private static Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> CreateLongRef<T1, T2, T3, T4, T5, T6, T7, TRest>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
		{
			return new Tuple<T1, T2, T3, T4, T5, T6, T7, TRest>(item1, item2, item3, item4, item5, item6, item7, rest);
		}
	}
}