using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
	public enum DWriteBreakCondition
	{
		Neutral,
		CanBreak,
		MayNotBreak,
		MustBreak
	}

    public class LineBreakpoints
    {
		public LineBreakpoints(int range_start, int range_end)
		{
			RangeStart = range_start;
			DWriteLineBreakpoints = new byte[range_end - range_start];
		}

		public int RangeStart { get; set; }
		public int RangeEnd {
			get { return RangeStart + DWriteLineBreakpoints.Length; }
		}
		public byte[] DWriteLineBreakpoints { get; } // contents of the array are intentionally writeable

		public DWriteBreakCondition GetBreakConditionBefore(int idx)
		{
			return (DWriteBreakCondition)(DWriteLineBreakpoints[idx - RangeStart] & 0x3);
		}

		public DWriteBreakCondition GetBreakConditionAfter(int idx)
		{
			return (DWriteBreakCondition)((DWriteLineBreakpoints[idx - RangeStart] >> 2) & 0x3);
		}

		public void ForceSetBreakConditionBefore(int idx, DWriteBreakCondition condition)
		{
		 	idx += RangeStart;
			DWriteLineBreakpoints[idx] = (byte)((DWriteLineBreakpoints[idx] & 0xfc) | (int)condition);
			if (idx > 0)
				DWriteLineBreakpoints[idx-1] = (byte)((DWriteLineBreakpoints[idx-1] & 0xf3) | ((int)condition << 2));
		} 

		public bool SetBreakConditionBefore(int idx, DWriteBreakCondition condition)
		{
			var existing_condition = GetBreakConditionBefore(idx);

			if (existing_condition == DWriteBreakCondition.MayNotBreak ||
				existing_condition == DWriteBreakCondition.MustBreak)
				return false;
			
			if (existing_condition == DWriteBreakCondition.CanBreak &&
				(condition == DWriteBreakCondition.Neutral || condition == DWriteBreakCondition.MayNotBreak))
				return false;

			ForceSetBreakConditionBefore(idx, condition);
			return true;
		}

		public void ForceSetBreakConditionAfter(int idx, DWriteBreakCondition condition)
		{
		 	idx += RangeStart;
			DWriteLineBreakpoints[idx] = (byte)((DWriteLineBreakpoints[idx] & 0xf3) | ((int)condition << 2));
			if (idx < DWriteLineBreakpoints.Length - 1)
				DWriteLineBreakpoints[idx+1] = (byte)((DWriteLineBreakpoints[idx+1] & 0xfc) | (int)condition);
		} 

		public bool SetBreakConditionAfter(int idx, DWriteBreakCondition condition)
		{
			var existing_condition = GetBreakConditionAfter(idx);

			if (existing_condition == DWriteBreakCondition.MayNotBreak ||
				existing_condition == DWriteBreakCondition.MustBreak)
				return false;
			
			if (existing_condition == DWriteBreakCondition.CanBreak &&
				(condition == DWriteBreakCondition.Neutral || condition == DWriteBreakCondition.MayNotBreak))
				return false;

			ForceSetBreakConditionAfter(idx, condition);
			return true;
		}

		public bool IsWhitespace(int idx)
		{
			return ((DWriteLineBreakpoints[idx - RangeStart] >> 4) & 1) == 1;
		}

		public int WhitespaceLengthBefore(int idx)
		{
			int result = 0;
			while (idx > RangeStart && IsWhitespace(idx-1))
			{
				idx--;
				result++;
			}
			return result;
		}

		public bool IsSoftHyphen(int idx)
		{
			return ((DWriteLineBreakpoints[idx - RangeStart] >> 5) & 1) == 1;
		}
    }
}

