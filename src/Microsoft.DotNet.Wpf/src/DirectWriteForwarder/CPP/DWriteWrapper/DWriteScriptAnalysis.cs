using System.Runtime.InteropServices;

namespace MS.Internal.Text.TextInterface
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DWriteScriptAnalysis
    {        
		public ushort Script;
		public DWriteScriptShapes Shapes;

        public DWriteScriptAnalysis(ushort script, DWriteScriptShapes shapes)
        {
			Script = script;
			Shapes = shapes;
        }
    }
}
