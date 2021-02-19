using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USF4_Stage_Tool
{
	public partial class InputBox : Form
	{
		public string EnteredValue = string.Empty;
		public InputBox()
		{
			InitializeComponent();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			AssignValue();
		}

		private void InputBox_Shown(object sender, EventArgs e)
		{
			EnteredValue = string.Empty;
			tbValue.Text = string.Empty;
			tbValue.Focus();
		}

		private new void Close()
		{
			ActiveForm.Close();
		}

		private void AssignValue()
		{
			EnteredValue = tbValue.Text;
			ActiveForm.Close();
		}

		private void tbValue_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{ 
				AssignValue(); 
			}
			if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}
	}
}
