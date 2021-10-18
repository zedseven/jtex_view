using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Jtex
{
	/// <summary>
	/// A PictureBox inheritor that allows the interpolation mode to be changed on-the-fly.
	/// <br />
	/// Original credit goes to https://stackoverflow.com/a/13484101/6003488.
	/// </summary>
	public class InterpolationPictureBox : PictureBox
	{
		/// <summary>
		/// The interpolation mode to use.
		/// </summary>
		public InterpolationMode InterpolationMode
		{
			get => _interpolationMode;
			set
			{
				_interpolationMode = value;
				Invalidate();
			}
		}
		private InterpolationMode _interpolationMode;

		protected override void OnPaint(PaintEventArgs pe)
		{
			pe.Graphics.InterpolationMode = InterpolationMode;
			base.OnPaint(pe);
		}
	}
}
