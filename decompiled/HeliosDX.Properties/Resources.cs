using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;

namespace HeliosDX.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("HeliosDX.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static UnmanagedMemoryStream Alert => ResourceManager.GetStream("Alert", resourceCulture);

	public static Bitmap Current => (Bitmap)ResourceManager.GetObject("Current", resourceCulture);

	public static Bitmap Current2 => (Bitmap)ResourceManager.GetObject("Current2", resourceCulture);

	public static UnmanagedMemoryStream Error => ResourceManager.GetStream("Error", resourceCulture);

	public static Bitmap Input => (Bitmap)ResourceManager.GetObject("Input", resourceCulture);

	public static Bitmap Power => (Bitmap)ResourceManager.GetObject("Power", resourceCulture);

	public static Bitmap Reverse => (Bitmap)ResourceManager.GetObject("Reverse", resourceCulture);

	public static Bitmap Reverse2 => (Bitmap)ResourceManager.GetObject("Reverse2", resourceCulture);

	public static Bitmap zr => (Bitmap)ResourceManager.GetObject("zr", resourceCulture);

	internal Resources()
	{
	}
}
