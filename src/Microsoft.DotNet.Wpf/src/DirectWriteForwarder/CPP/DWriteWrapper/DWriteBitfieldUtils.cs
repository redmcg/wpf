namespace MS.Internal.Text.TextInterface
{
	internal class DWriteBitfieldUtils
	{
		internal static bool ShapingText_IsShapedAlone(uint shapingTextProps)
		{
			return (shapingTextProps & 1) == 1;
		}
	}
}
