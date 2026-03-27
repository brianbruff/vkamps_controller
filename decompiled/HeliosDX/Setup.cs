using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Media;
using System.Windows.Forms;
using HeliosDX.Properties;

namespace HeliosDX;

public class Setup : Form
{
	private bool isMousePress;

	private Point _clickPoint;

	private Point _formStartPoint;

	private SoundPlayer sp;

	private IContainer components;

	private Button button1;

	private ComboBox comboBox_ports;

	private SerialPort serialPort;

	private RadioButton radioButton1;

	private RadioButton radioButton2;

	private TextBox textBox1;

	private Panel panel1;

	private Panel panel2;

	private Button exit_button;

	private Label op_label;

	private CheckBox checkBox1;

	private CheckBox checkBox2;

	private Panel panel3;

	private RadioButton rb_p_3;

	private RadioButton rb_p_2;

	private Label label6;

	private RadioButton rb_p_1;

	private Panel panel6;

	private TextBox tb_a_7;

	private TextBox tb_a_6;

	private TextBox tb_a_5;

	private TextBox tb_a_4;

	private TextBox tb_a_3;

	private TextBox tb_a_2;

	private TextBox tb_a_1;

	private Label l_a_7;

	private Label l_a_6;

	private Label l_a_5;

	private Label l_a_4;

	private Label l_a_3;

	private Label l_a_2;

	private Label l_a_1;

	private Label label9;

	private Panel panel5;

	private RadioButton rb_c_6;

	private RadioButton rb_c_5;

	private RadioButton rb_c_4;

	private RadioButton rb_c_3;

	private RadioButton rb_c_2;

	private Label label8;

	private RadioButton rb_c_1;

	private Panel panel4;

	private RadioButton rb_v_4;

	private RadioButton rb_v_3;

	private RadioButton rb_v_2;

	private Label label7;

	private RadioButton rb_v_1;

	private TextBox tb_a_8;

	private Label l_a_8;

	private CheckBox SoundCheck;

	private CheckBox inputCheck;

	private Label label1;

	private Panel panel8;

	private Panel panel7;

	private LinkLabel linkLabel1;

	public Setup()
	{
		InitializeComponent();
	}

	private void Setup_Load(object sender, EventArgs e)
	{
		sp = new SoundPlayer(Resources.Alert);
		string[] array = File.ReadAllLines("save.txt");
		if (array.Length != 0)
		{
			if (bool.Parse(array[8]))
			{
				base.TopMost = true;
				checkBox1.Checked = true;
			}
			else
			{
				base.TopMost = false;
				checkBox1.Checked = false;
			}
			if (array[10] == "F")
			{
				checkBox2.Checked = true;
			}
			else
			{
				checkBox2.Checked = false;
			}
			if (array[4] == "LAN")
			{
				radioButton2.Checked = true;
			}
			else
			{
				radioButton1.Checked = true;
			}
			if (array[5] != "")
			{
				textBox1.Text = array[5];
			}
			else
			{
				textBox1.Text = "";
			}
			switch (array[13])
			{
			case "600":
				rb_p_1.Checked = true;
				break;
			case "1200":
				rb_p_2.Checked = true;
				break;
			case "2400":
				rb_p_3.Checked = true;
				break;
			}
			switch (array[14])
			{
			case "48":
				rb_v_1.Checked = true;
				break;
			case "50":
				rb_v_2.Checked = true;
				break;
			case "53.5":
				rb_v_3.Checked = true;
				break;
			case "58.3":
				rb_v_4.Checked = true;
				break;
			}
			switch (array[15])
			{
			case "0":
				rb_c_1.Checked = true;
				break;
			case "1":
				rb_c_2.Checked = true;
				break;
			case "2":
				rb_c_3.Checked = true;
				break;
			case "3":
				rb_c_4.Checked = true;
				break;
			case "4":
				rb_c_5.Checked = true;
				break;
			case "5":
				rb_c_6.Checked = true;
				break;
			}
			string[] array2 = array[16].Split(',');
			tb_a_1.Text = array2[0];
			tb_a_2.Text = array2[1];
			tb_a_3.Text = array2[2];
			tb_a_4.Text = array2[3];
			tb_a_5.Text = array2[4];
			tb_a_6.Text = array2[5];
			tb_a_7.Text = array2[6];
			tb_a_8.Text = array2[7];
			if (array[17] == "False")
			{
				SoundCheck.Checked = false;
			}
			else
			{
				SoundCheck.Checked = true;
			}
			if (array[18] == "False")
			{
				inputCheck.Checked = false;
			}
			else
			{
				inputCheck.Checked = true;
			}
		}
		ComboBox.ObjectCollection items = comboBox_ports.Items;
		object[] portNames = SerialPort.GetPortNames();
		items.AddRange(portNames);
		if (comboBox_ports.Items.Count <= 0)
		{
			return;
		}
		comboBox_ports.SelectedIndex = 0;
		try
		{
			if (array.Length == 0)
			{
				return;
			}
			for (int i = 0; i < comboBox_ports.Items.Count; i++)
			{
				if (comboBox_ports.Items[i].ToString() == array[0])
				{
					comboBox_ports.SelectedIndex = i;
					break;
				}
			}
		}
		catch
		{
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		if (SoundCheck.Checked)
		{
			sp.Play();
		}
		if (((!radioButton1.Checked || !(comboBox_ports.Text != "")) && (!radioButton2.Checked || !(textBox1.Text != ""))) || !(tb_a_1.Text != "") || !(tb_a_2.Text != "") || !(tb_a_3.Text != "") || !(tb_a_4.Text != "") || !(tb_a_5.Text != "") || !(tb_a_6.Text != "") || !(tb_a_7.Text != "") || !(tb_a_8.Text != ""))
		{
			return;
		}
		try
		{
			string[] array = File.ReadAllLines("save.txt");
			using StreamWriter streamWriter = new StreamWriter("save.txt", append: false);
			if (array.Length != 0)
			{
				array[0] = ((comboBox_ports.Items.Count > 0) ? comboBox_ports.Text.ToString() : "");
				array[4] = (radioButton1.Checked ? "USB" : "LAN");
				array[5] = textBox1.Text;
				array[8] = checkBox1.Checked.ToString();
				if (checkBox2.Checked)
				{
					array[10] = "F";
				}
				else
				{
					array[10] = "C";
				}
				if (rb_p_1.Checked)
				{
					array[13] = "600";
				}
				else if (rb_p_2.Checked)
				{
					array[13] = "1200";
				}
				else if (rb_p_3.Checked)
				{
					array[13] = "2400";
				}
				if (rb_v_1.Checked)
				{
					array[14] = "48";
				}
				else if (rb_v_2.Checked)
				{
					array[14] = "50";
				}
				else if (rb_v_3.Checked)
				{
					array[14] = "53.5";
				}
				else if (rb_v_4.Checked)
				{
					array[14] = "58.3";
				}
				if (rb_c_1.Checked)
				{
					array[15] = "0";
				}
				else if (rb_c_2.Checked)
				{
					array[15] = "1";
				}
				else if (rb_c_3.Checked)
				{
					array[15] = "2";
				}
				else if (rb_c_4.Checked)
				{
					array[15] = "3";
				}
				else if (rb_c_5.Checked)
				{
					array[15] = "4";
				}
				else if (rb_c_6.Checked)
				{
					array[15] = "5";
				}
				string[] array2 = new string[8] { tb_a_1.Text, tb_a_2.Text, tb_a_3.Text, tb_a_4.Text, tb_a_5.Text, tb_a_6.Text, tb_a_7.Text, tb_a_8.Text };
				string text = array2[0];
				for (int i = 1; i < 8; i++)
				{
					text = text + "," + array2[i];
				}
				array[16] = text;
				if (SoundCheck.Checked)
				{
					array[17] = "True";
				}
				else
				{
					array[17] = "False";
				}
				if (inputCheck.Checked)
				{
					array[18] = "True";
				}
				else
				{
					array[18] = "False";
				}
				for (int j = 0; j < array.Length; j++)
				{
					streamWriter.WriteLine(array[j]);
				}
			}
			streamWriter.Close();
		}
		catch
		{
			return;
		}
		Close();
	}

	private void checkBox1_CheckedChanged(object sender, EventArgs e)
	{
		if (checkBox1.Checked)
		{
			base.TopMost = true;
		}
		else
		{
			base.TopMost = false;
		}
	}

	private void tb_a_1_KeyPress(object sender, KeyPressEventArgs e)
	{
		if ((e.KeyChar <= '0' || e.KeyChar >= '4') && e.KeyChar != '\b')
		{
			e.Handled = true;
		}
	}

	private void tb_a_1_Click(object sender, EventArgs e)
	{
		if (SoundCheck.Checked)
		{
			sp.Play();
		}
		((TextBox)sender).Text = "";
	}

	private void rb_c_1_CheckedChanged(object sender, EventArgs e)
	{
		if (SoundCheck.Checked)
		{
			sp.Play();
		}
	}

	private void panel2_MouseDown(object sender, MouseEventArgs e)
	{
		isMousePress = true;
		_clickPoint = Cursor.Position;
		_formStartPoint = base.Location;
	}

	private void panel2_MouseMove(object sender, MouseEventArgs e)
	{
		if (isMousePress)
		{
			Point point = new Point(Cursor.Position.X - _clickPoint.X, Cursor.Position.Y - _clickPoint.Y);
			base.Location = new Point(_formStartPoint.X + point.X, _formStartPoint.Y + point.Y);
		}
	}

	private void panel2_MouseUp(object sender, MouseEventArgs e)
	{
		isMousePress = false;
		_clickPoint = Point.Empty;
	}

	private void exit_button_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		linkLabel1.LinkVisited = true;
		Process.Start("https://rz1zr.ru");
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeliosDX.Setup));
		this.button1 = new System.Windows.Forms.Button();
		this.comboBox_ports = new System.Windows.Forms.ComboBox();
		this.serialPort = new System.IO.Ports.SerialPort(this.components);
		this.radioButton1 = new System.Windows.Forms.RadioButton();
		this.radioButton2 = new System.Windows.Forms.RadioButton();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.panel1 = new System.Windows.Forms.Panel();
		this.panel8 = new System.Windows.Forms.Panel();
		this.panel7 = new System.Windows.Forms.Panel();
		this.label1 = new System.Windows.Forms.Label();
		this.inputCheck = new System.Windows.Forms.CheckBox();
		this.SoundCheck = new System.Windows.Forms.CheckBox();
		this.panel6 = new System.Windows.Forms.Panel();
		this.tb_a_8 = new System.Windows.Forms.TextBox();
		this.l_a_8 = new System.Windows.Forms.Label();
		this.tb_a_7 = new System.Windows.Forms.TextBox();
		this.tb_a_6 = new System.Windows.Forms.TextBox();
		this.tb_a_5 = new System.Windows.Forms.TextBox();
		this.tb_a_4 = new System.Windows.Forms.TextBox();
		this.tb_a_3 = new System.Windows.Forms.TextBox();
		this.tb_a_2 = new System.Windows.Forms.TextBox();
		this.tb_a_1 = new System.Windows.Forms.TextBox();
		this.l_a_7 = new System.Windows.Forms.Label();
		this.l_a_6 = new System.Windows.Forms.Label();
		this.l_a_5 = new System.Windows.Forms.Label();
		this.l_a_4 = new System.Windows.Forms.Label();
		this.l_a_3 = new System.Windows.Forms.Label();
		this.l_a_2 = new System.Windows.Forms.Label();
		this.l_a_1 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.panel5 = new System.Windows.Forms.Panel();
		this.rb_c_6 = new System.Windows.Forms.RadioButton();
		this.rb_c_5 = new System.Windows.Forms.RadioButton();
		this.rb_c_4 = new System.Windows.Forms.RadioButton();
		this.rb_c_3 = new System.Windows.Forms.RadioButton();
		this.rb_c_2 = new System.Windows.Forms.RadioButton();
		this.label8 = new System.Windows.Forms.Label();
		this.rb_c_1 = new System.Windows.Forms.RadioButton();
		this.panel4 = new System.Windows.Forms.Panel();
		this.rb_v_4 = new System.Windows.Forms.RadioButton();
		this.rb_v_3 = new System.Windows.Forms.RadioButton();
		this.rb_v_2 = new System.Windows.Forms.RadioButton();
		this.label7 = new System.Windows.Forms.Label();
		this.rb_v_1 = new System.Windows.Forms.RadioButton();
		this.panel3 = new System.Windows.Forms.Panel();
		this.rb_p_3 = new System.Windows.Forms.RadioButton();
		this.rb_p_2 = new System.Windows.Forms.RadioButton();
		this.label6 = new System.Windows.Forms.Label();
		this.rb_p_1 = new System.Windows.Forms.RadioButton();
		this.checkBox2 = new System.Windows.Forms.CheckBox();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		this.panel2 = new System.Windows.Forms.Panel();
		this.linkLabel1 = new System.Windows.Forms.LinkLabel();
		this.exit_button = new System.Windows.Forms.Button();
		this.op_label = new System.Windows.Forms.Label();
		this.panel1.SuspendLayout();
		this.panel6.SuspendLayout();
		this.panel5.SuspendLayout();
		this.panel4.SuspendLayout();
		this.panel3.SuspendLayout();
		this.panel2.SuspendLayout();
		base.SuspendLayout();
		this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.button1.Location = new System.Drawing.Point(135, 386);
		this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(100, 49);
		this.button1.TabIndex = 10;
		this.button1.Text = "ОК";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.comboBox_ports.Anchor = System.Windows.Forms.AnchorStyles.None;
		this.comboBox_ports.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.comboBox_ports.ForeColor = System.Drawing.Color.FromArgb(0, 64, 0);
		this.comboBox_ports.FormattingEnabled = true;
		this.comboBox_ports.Location = new System.Drawing.Point(136, 80);
		this.comboBox_ports.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.comboBox_ports.Name = "comboBox_ports";
		this.comboBox_ports.Size = new System.Drawing.Size(100, 33);
		this.comboBox_ports.TabIndex = 11;
		this.comboBox_ports.Text = "COM5";
		this.radioButton1.Checked = true;
		this.radioButton1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.radioButton1.ForeColor = System.Drawing.Color.FromArgb(0, 192, 0);
		this.radioButton1.Location = new System.Drawing.Point(46, 83);
		this.radioButton1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.radioButton1.Name = "radioButton1";
		this.radioButton1.Size = new System.Drawing.Size(92, 31);
		this.radioButton1.TabIndex = 15;
		this.radioButton1.TabStop = true;
		this.radioButton1.Text = "COM";
		this.radioButton1.UseVisualStyleBackColor = true;
		this.radioButton1.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.radioButton2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.radioButton2.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
		this.radioButton2.Location = new System.Drawing.Point(285, 83);
		this.radioButton2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.radioButton2.Name = "radioButton2";
		this.radioButton2.Size = new System.Drawing.Size(80, 31);
		this.radioButton2.TabIndex = 16;
		this.radioButton2.Text = "LAN";
		this.radioButton2.UseVisualStyleBackColor = true;
		this.radioButton2.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.textBox1.ForeColor = System.Drawing.Color.FromArgb(128, 64, 0);
		this.textBox1.Location = new System.Drawing.Point(363, 80);
		this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(156, 33);
		this.textBox1.TabIndex = 20;
		this.textBox1.Text = "192.168.0.55";
		this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel1.Controls.Add(this.panel8);
		this.panel1.Controls.Add(this.panel7);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Controls.Add(this.inputCheck);
		this.panel1.Controls.Add(this.SoundCheck);
		this.panel1.Controls.Add(this.panel6);
		this.panel1.Controls.Add(this.panel5);
		this.panel1.Controls.Add(this.panel4);
		this.panel1.Controls.Add(this.panel3);
		this.panel1.Controls.Add(this.checkBox2);
		this.panel1.Controls.Add(this.checkBox1);
		this.panel1.Controls.Add(this.panel2);
		this.panel1.Controls.Add(this.button1);
		this.panel1.Controls.Add(this.textBox1);
		this.panel1.Controls.Add(this.comboBox_ports);
		this.panel1.Controls.Add(this.radioButton1);
		this.panel1.Controls.Add(this.radioButton2);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.panel1.MaximumSize = new System.Drawing.Size(900, 465);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(900, 465);
		this.panel1.TabIndex = 23;
		this.panel8.BackColor = System.Drawing.Color.Black;
		this.panel8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel8.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
		this.panel8.Location = new System.Drawing.Point(0, 65);
		this.panel8.Margin = new System.Windows.Forms.Padding(0);
		this.panel8.Name = "panel8";
		this.panel8.Size = new System.Drawing.Size(900, 0);
		this.panel8.TabIndex = 91;
		this.panel7.BackColor = System.Drawing.Color.Black;
		this.panel7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel7.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
		this.panel7.Location = new System.Drawing.Point(0, 129);
		this.panel7.Margin = new System.Windows.Forms.Padding(0);
		this.panel7.Name = "panel7";
		this.panel7.Size = new System.Drawing.Size(900, 0);
		this.panel7.TabIndex = 90;
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.label1.ForeColor = System.Drawing.Color.DodgerBlue;
		this.label1.Location = new System.Drawing.Point(280, 155);
		this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(61, 25);
		this.label1.TabIndex = 89;
		this.label1.Text = "Other";
		this.inputCheck.BackColor = System.Drawing.Color.Black;
		this.inputCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.inputCheck.ForeColor = System.Drawing.Color.DodgerBlue;
		this.inputCheck.Location = new System.Drawing.Point(286, 289);
		this.inputCheck.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.inputCheck.Name = "inputCheck";
		this.inputCheck.Size = new System.Drawing.Size(183, 38);
		this.inputCheck.TabIndex = 88;
		this.inputCheck.Text = "Input Indicator";
		this.inputCheck.UseVisualStyleBackColor = false;
		this.SoundCheck.BackColor = System.Drawing.Color.Black;
		this.SoundCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.SoundCheck.ForeColor = System.Drawing.Color.DodgerBlue;
		this.SoundCheck.Location = new System.Drawing.Point(286, 255);
		this.SoundCheck.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.SoundCheck.Name = "SoundCheck";
		this.SoundCheck.Size = new System.Drawing.Size(123, 29);
		this.SoundCheck.TabIndex = 87;
		this.SoundCheck.Text = "Sound";
		this.SoundCheck.UseVisualStyleBackColor = false;
		this.SoundCheck.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.panel6.Controls.Add(this.tb_a_8);
		this.panel6.Controls.Add(this.l_a_8);
		this.panel6.Controls.Add(this.tb_a_7);
		this.panel6.Controls.Add(this.tb_a_6);
		this.panel6.Controls.Add(this.tb_a_5);
		this.panel6.Controls.Add(this.tb_a_4);
		this.panel6.Controls.Add(this.tb_a_3);
		this.panel6.Controls.Add(this.tb_a_2);
		this.panel6.Controls.Add(this.tb_a_1);
		this.panel6.Controls.Add(this.l_a_7);
		this.panel6.Controls.Add(this.l_a_6);
		this.panel6.Controls.Add(this.l_a_5);
		this.panel6.Controls.Add(this.l_a_4);
		this.panel6.Controls.Add(this.l_a_3);
		this.panel6.Controls.Add(this.l_a_2);
		this.panel6.Controls.Add(this.l_a_1);
		this.panel6.Controls.Add(this.label9);
		this.panel6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.panel6.Location = new System.Drawing.Point(716, 142);
		this.panel6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.panel6.Name = "panel6";
		this.panel6.Size = new System.Drawing.Size(140, 295);
		this.panel6.TabIndex = 86;
		this.tb_a_8.BackColor = System.Drawing.Color.DodgerBlue;
		this.tb_a_8.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tb_a_8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.tb_a_8.ForeColor = System.Drawing.SystemColors.Info;
		this.tb_a_8.Location = new System.Drawing.Point(20, 260);
		this.tb_a_8.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tb_a_8.MaxLength = 1;
		this.tb_a_8.Name = "tb_a_8";
		this.tb_a_8.Size = new System.Drawing.Size(50, 23);
		this.tb_a_8.TabIndex = 95;
		this.tb_a_8.Text = "1";
		this.tb_a_8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tb_a_8.Click += new System.EventHandler(tb_a_1_Click);
		this.tb_a_8.KeyPress += new System.Windows.Forms.KeyPressEventHandler(tb_a_1_KeyPress);
		this.l_a_8.AutoSize = true;
		this.l_a_8.ForeColor = System.Drawing.Color.DodgerBlue;
		this.l_a_8.Location = new System.Drawing.Point(80, 260);
		this.l_a_8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.l_a_8.Name = "l_a_8";
		this.l_a_8.Size = new System.Drawing.Size(23, 25);
		this.l_a_8.TabIndex = 94;
		this.l_a_8.Text = "6";
		this.tb_a_7.BackColor = System.Drawing.Color.DodgerBlue;
		this.tb_a_7.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tb_a_7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.tb_a_7.ForeColor = System.Drawing.SystemColors.Info;
		this.tb_a_7.Location = new System.Drawing.Point(20, 229);
		this.tb_a_7.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tb_a_7.MaxLength = 1;
		this.tb_a_7.Name = "tb_a_7";
		this.tb_a_7.Size = new System.Drawing.Size(50, 23);
		this.tb_a_7.TabIndex = 93;
		this.tb_a_7.Text = "1";
		this.tb_a_7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tb_a_7.Click += new System.EventHandler(tb_a_1_Click);
		this.tb_a_7.KeyPress += new System.Windows.Forms.KeyPressEventHandler(tb_a_1_KeyPress);
		this.tb_a_6.BackColor = System.Drawing.Color.DodgerBlue;
		this.tb_a_6.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tb_a_6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.tb_a_6.ForeColor = System.Drawing.SystemColors.Info;
		this.tb_a_6.Location = new System.Drawing.Point(20, 200);
		this.tb_a_6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tb_a_6.MaxLength = 1;
		this.tb_a_6.Name = "tb_a_6";
		this.tb_a_6.Size = new System.Drawing.Size(50, 23);
		this.tb_a_6.TabIndex = 92;
		this.tb_a_6.Text = "1";
		this.tb_a_6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tb_a_6.Click += new System.EventHandler(tb_a_1_Click);
		this.tb_a_6.KeyPress += new System.Windows.Forms.KeyPressEventHandler(tb_a_1_KeyPress);
		this.tb_a_5.BackColor = System.Drawing.Color.DodgerBlue;
		this.tb_a_5.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tb_a_5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.tb_a_5.ForeColor = System.Drawing.SystemColors.Info;
		this.tb_a_5.Location = new System.Drawing.Point(20, 169);
		this.tb_a_5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tb_a_5.MaxLength = 1;
		this.tb_a_5.Name = "tb_a_5";
		this.tb_a_5.Size = new System.Drawing.Size(50, 23);
		this.tb_a_5.TabIndex = 91;
		this.tb_a_5.Text = "1";
		this.tb_a_5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tb_a_5.Click += new System.EventHandler(tb_a_1_Click);
		this.tb_a_5.KeyPress += new System.Windows.Forms.KeyPressEventHandler(tb_a_1_KeyPress);
		this.tb_a_4.BackColor = System.Drawing.Color.DodgerBlue;
		this.tb_a_4.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tb_a_4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.tb_a_4.ForeColor = System.Drawing.SystemColors.Info;
		this.tb_a_4.Location = new System.Drawing.Point(20, 140);
		this.tb_a_4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tb_a_4.MaxLength = 1;
		this.tb_a_4.Name = "tb_a_4";
		this.tb_a_4.Size = new System.Drawing.Size(50, 23);
		this.tb_a_4.TabIndex = 90;
		this.tb_a_4.Text = "1";
		this.tb_a_4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tb_a_4.Click += new System.EventHandler(tb_a_1_Click);
		this.tb_a_4.KeyPress += new System.Windows.Forms.KeyPressEventHandler(tb_a_1_KeyPress);
		this.tb_a_3.BackColor = System.Drawing.Color.DodgerBlue;
		this.tb_a_3.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tb_a_3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.tb_a_3.ForeColor = System.Drawing.SystemColors.Info;
		this.tb_a_3.Location = new System.Drawing.Point(20, 109);
		this.tb_a_3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tb_a_3.MaxLength = 1;
		this.tb_a_3.Name = "tb_a_3";
		this.tb_a_3.Size = new System.Drawing.Size(50, 23);
		this.tb_a_3.TabIndex = 89;
		this.tb_a_3.Text = "1";
		this.tb_a_3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tb_a_3.Click += new System.EventHandler(tb_a_1_Click);
		this.tb_a_3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(tb_a_1_KeyPress);
		this.tb_a_2.BackColor = System.Drawing.Color.DodgerBlue;
		this.tb_a_2.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tb_a_2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.tb_a_2.ForeColor = System.Drawing.SystemColors.Info;
		this.tb_a_2.Location = new System.Drawing.Point(20, 80);
		this.tb_a_2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tb_a_2.MaxLength = 1;
		this.tb_a_2.Name = "tb_a_2";
		this.tb_a_2.Size = new System.Drawing.Size(50, 23);
		this.tb_a_2.TabIndex = 88;
		this.tb_a_2.Text = "1";
		this.tb_a_2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tb_a_2.Click += new System.EventHandler(tb_a_1_Click);
		this.tb_a_2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(tb_a_1_KeyPress);
		this.tb_a_1.BackColor = System.Drawing.Color.DodgerBlue;
		this.tb_a_1.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.tb_a_1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.tb_a_1.ForeColor = System.Drawing.SystemColors.Info;
		this.tb_a_1.Location = new System.Drawing.Point(20, 49);
		this.tb_a_1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.tb_a_1.MaxLength = 1;
		this.tb_a_1.Name = "tb_a_1";
		this.tb_a_1.Size = new System.Drawing.Size(50, 23);
		this.tb_a_1.TabIndex = 87;
		this.tb_a_1.Text = "1";
		this.tb_a_1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.tb_a_1.Click += new System.EventHandler(tb_a_1_Click);
		this.tb_a_1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(tb_a_1_KeyPress);
		this.l_a_7.AutoSize = true;
		this.l_a_7.ForeColor = System.Drawing.Color.DodgerBlue;
		this.l_a_7.Location = new System.Drawing.Point(80, 229);
		this.l_a_7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.l_a_7.Name = "l_a_7";
		this.l_a_7.Size = new System.Drawing.Size(34, 25);
		this.l_a_7.TabIndex = 86;
		this.l_a_7.Text = "10";
		this.l_a_6.AutoSize = true;
		this.l_a_6.ForeColor = System.Drawing.Color.DodgerBlue;
		this.l_a_6.Location = new System.Drawing.Point(80, 200);
		this.l_a_6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.l_a_6.Name = "l_a_6";
		this.l_a_6.Size = new System.Drawing.Size(34, 25);
		this.l_a_6.TabIndex = 85;
		this.l_a_6.Text = "15";
		this.l_a_5.AutoSize = true;
		this.l_a_5.ForeColor = System.Drawing.Color.DodgerBlue;
		this.l_a_5.Location = new System.Drawing.Point(80, 169);
		this.l_a_5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.l_a_5.Name = "l_a_5";
		this.l_a_5.Size = new System.Drawing.Size(34, 25);
		this.l_a_5.TabIndex = 84;
		this.l_a_5.Text = "20";
		this.l_a_4.AutoSize = true;
		this.l_a_4.ForeColor = System.Drawing.Color.DodgerBlue;
		this.l_a_4.Location = new System.Drawing.Point(80, 140);
		this.l_a_4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.l_a_4.Name = "l_a_4";
		this.l_a_4.Size = new System.Drawing.Size(34, 25);
		this.l_a_4.TabIndex = 83;
		this.l_a_4.Text = "30";
		this.l_a_3.AutoSize = true;
		this.l_a_3.ForeColor = System.Drawing.Color.DodgerBlue;
		this.l_a_3.Location = new System.Drawing.Point(80, 109);
		this.l_a_3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.l_a_3.Name = "l_a_3";
		this.l_a_3.Size = new System.Drawing.Size(34, 25);
		this.l_a_3.TabIndex = 82;
		this.l_a_3.Text = "40";
		this.l_a_2.AutoSize = true;
		this.l_a_2.ForeColor = System.Drawing.Color.DodgerBlue;
		this.l_a_2.Location = new System.Drawing.Point(80, 80);
		this.l_a_2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.l_a_2.Name = "l_a_2";
		this.l_a_2.Size = new System.Drawing.Size(34, 25);
		this.l_a_2.TabIndex = 81;
		this.l_a_2.Text = "80";
		this.l_a_1.AutoSize = true;
		this.l_a_1.ForeColor = System.Drawing.Color.DodgerBlue;
		this.l_a_1.Location = new System.Drawing.Point(80, 49);
		this.l_a_1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.l_a_1.Name = "l_a_1";
		this.l_a_1.Size = new System.Drawing.Size(45, 25);
		this.l_a_1.TabIndex = 80;
		this.l_a_1.Text = "160";
		this.label9.AutoSize = true;
		this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.label9.ForeColor = System.Drawing.Color.DodgerBlue;
		this.label9.Location = new System.Drawing.Point(18, 15);
		this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(86, 25);
		this.label9.TabIndex = 79;
		this.label9.Text = "Antenna";
		this.panel5.Controls.Add(this.rb_c_6);
		this.panel5.Controls.Add(this.rb_c_5);
		this.panel5.Controls.Add(this.rb_c_4);
		this.panel5.Controls.Add(this.rb_c_3);
		this.panel5.Controls.Add(this.rb_c_2);
		this.panel5.Controls.Add(this.label8);
		this.panel5.Controls.Add(this.rb_c_1);
		this.panel5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.panel5.Location = new System.Drawing.Point(470, 142);
		this.panel5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.panel5.Name = "panel5";
		this.panel5.Size = new System.Drawing.Size(238, 295);
		this.panel5.TabIndex = 84;
		this.rb_c_6.AutoSize = true;
		this.rb_c_6.ForeColor = System.Drawing.Color.Green;
		this.rb_c_6.Location = new System.Drawing.Point(24, 255);
		this.rb_c_6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_c_6.Name = "rb_c_6";
		this.rb_c_6.Size = new System.Drawing.Size(191, 29);
		this.rb_c_6.TabIndex = 85;
		this.rb_c_6.Text = "Manual Switching";
		this.rb_c_6.UseVisualStyleBackColor = true;
		this.rb_c_6.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.rb_c_5.AutoSize = true;
		this.rb_c_5.ForeColor = System.Drawing.Color.Green;
		this.rb_c_5.Location = new System.Drawing.Point(24, 212);
		this.rb_c_5.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_c_5.Name = "rb_c_5";
		this.rb_c_5.Size = new System.Drawing.Size(171, 29);
		this.rb_c_5.TabIndex = 84;
		this.rb_c_5.Text = "Anan, SunSDR";
		this.rb_c_5.UseVisualStyleBackColor = true;
		this.rb_c_5.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.rb_c_4.AutoSize = true;
		this.rb_c_4.ForeColor = System.Drawing.Color.Green;
		this.rb_c_4.Location = new System.Drawing.Point(22, 171);
		this.rb_c_4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_c_4.Name = "rb_c_4";
		this.rb_c_4.Size = new System.Drawing.Size(167, 29);
		this.rb_c_4.TabIndex = 83;
		this.rb_c_4.Text = "Kenwood, Flex";
		this.rb_c_4.UseVisualStyleBackColor = true;
		this.rb_c_4.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.rb_c_3.AutoSize = true;
		this.rb_c_3.ForeColor = System.Drawing.Color.Green;
		this.rb_c_3.Location = new System.Drawing.Point(24, 132);
		this.rb_c_3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_c_3.Name = "rb_c_3";
		this.rb_c_3.Size = new System.Drawing.Size(93, 29);
		this.rb_c_3.TabIndex = 82;
		this.rb_c_3.Text = "Yaesu";
		this.rb_c_3.UseVisualStyleBackColor = true;
		this.rb_c_3.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.rb_c_2.AutoSize = true;
		this.rb_c_2.ForeColor = System.Drawing.Color.Green;
		this.rb_c_2.Location = new System.Drawing.Point(24, 88);
		this.rb_c_2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_c_2.Name = "rb_c_2";
		this.rb_c_2.Size = new System.Drawing.Size(79, 29);
		this.rb_c_2.TabIndex = 81;
		this.rb_c_2.Text = "Icom";
		this.rb_c_2.UseVisualStyleBackColor = true;
		this.rb_c_2.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.label8.AutoSize = true;
		this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.label8.ForeColor = System.Drawing.Color.Green;
		this.label8.Location = new System.Drawing.Point(18, 15);
		this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(54, 25);
		this.label8.TabIndex = 79;
		this.label8.Text = "CAT";
		this.rb_c_1.AutoSize = true;
		this.rb_c_1.Checked = true;
		this.rb_c_1.ForeColor = System.Drawing.Color.Green;
		this.rb_c_1.Location = new System.Drawing.Point(24, 46);
		this.rb_c_1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_c_1.Name = "rb_c_1";
		this.rb_c_1.Size = new System.Drawing.Size(62, 29);
		this.rb_c_1.TabIndex = 80;
		this.rb_c_1.TabStop = true;
		this.rb_c_1.Text = "RF";
		this.rb_c_1.UseVisualStyleBackColor = true;
		this.rb_c_1.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.panel4.Controls.Add(this.rb_v_4);
		this.panel4.Controls.Add(this.rb_v_3);
		this.panel4.Controls.Add(this.rb_v_2);
		this.panel4.Controls.Add(this.label7);
		this.panel4.Controls.Add(this.rb_v_1);
		this.panel4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.panel4.Location = new System.Drawing.Point(144, 142);
		this.panel4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.panel4.Name = "panel4";
		this.panel4.Size = new System.Drawing.Size(116, 189);
		this.panel4.TabIndex = 83;
		this.rb_v_4.AutoSize = true;
		this.rb_v_4.ForeColor = System.Drawing.Color.Silver;
		this.rb_v_4.Location = new System.Drawing.Point(26, 151);
		this.rb_v_4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_v_4.Name = "rb_v_4";
		this.rb_v_4.Size = new System.Drawing.Size(75, 29);
		this.rb_v_4.TabIndex = 83;
		this.rb_v_4.Text = "58.3";
		this.rb_v_4.UseVisualStyleBackColor = true;
		this.rb_v_4.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.rb_v_3.AutoSize = true;
		this.rb_v_3.ForeColor = System.Drawing.Color.Silver;
		this.rb_v_3.Location = new System.Drawing.Point(26, 115);
		this.rb_v_3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_v_3.Name = "rb_v_3";
		this.rb_v_3.Size = new System.Drawing.Size(75, 29);
		this.rb_v_3.TabIndex = 82;
		this.rb_v_3.Text = "53.5";
		this.rb_v_3.UseVisualStyleBackColor = true;
		this.rb_v_3.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.rb_v_2.AutoSize = true;
		this.rb_v_2.ForeColor = System.Drawing.Color.Silver;
		this.rb_v_2.Location = new System.Drawing.Point(26, 80);
		this.rb_v_2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_v_2.Name = "rb_v_2";
		this.rb_v_2.Size = new System.Drawing.Size(59, 29);
		this.rb_v_2.TabIndex = 81;
		this.rb_v_2.Text = "50";
		this.rb_v_2.UseVisualStyleBackColor = true;
		this.rb_v_2.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.label7.AutoSize = true;
		this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.label7.ForeColor = System.Drawing.Color.Silver;
		this.label7.Location = new System.Drawing.Point(18, 15);
		this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(79, 25);
		this.label7.TabIndex = 79;
		this.label7.Text = "Voltage";
		this.rb_v_1.AutoSize = true;
		this.rb_v_1.Checked = true;
		this.rb_v_1.ForeColor = System.Drawing.Color.Silver;
		this.rb_v_1.Location = new System.Drawing.Point(26, 45);
		this.rb_v_1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_v_1.Name = "rb_v_1";
		this.rb_v_1.Size = new System.Drawing.Size(59, 29);
		this.rb_v_1.TabIndex = 80;
		this.rb_v_1.TabStop = true;
		this.rb_v_1.Text = "48";
		this.rb_v_1.UseVisualStyleBackColor = true;
		this.rb_v_1.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.panel3.Controls.Add(this.rb_p_3);
		this.panel3.Controls.Add(this.rb_p_2);
		this.panel3.Controls.Add(this.label6);
		this.panel3.Controls.Add(this.rb_p_1);
		this.panel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.panel3.Location = new System.Drawing.Point(20, 142);
		this.panel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.panel3.Name = "panel3";
		this.panel3.Size = new System.Drawing.Size(110, 160);
		this.panel3.TabIndex = 81;
		this.rb_p_3.AutoSize = true;
		this.rb_p_3.ForeColor = System.Drawing.Color.DarkOrange;
		this.rb_p_3.Location = new System.Drawing.Point(24, 115);
		this.rb_p_3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_p_3.Name = "rb_p_3";
		this.rb_p_3.Size = new System.Drawing.Size(81, 29);
		this.rb_p_3.TabIndex = 82;
		this.rb_p_3.Text = "2400";
		this.rb_p_3.UseVisualStyleBackColor = true;
		this.rb_p_3.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.rb_p_2.AutoSize = true;
		this.rb_p_2.ForeColor = System.Drawing.Color.DarkOrange;
		this.rb_p_2.Location = new System.Drawing.Point(24, 80);
		this.rb_p_2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_p_2.Name = "rb_p_2";
		this.rb_p_2.Size = new System.Drawing.Size(81, 29);
		this.rb_p_2.TabIndex = 81;
		this.rb_p_2.Text = "1200";
		this.rb_p_2.UseVisualStyleBackColor = true;
		this.rb_p_2.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.label6.AutoSize = true;
		this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f);
		this.label6.ForeColor = System.Drawing.Color.DarkOrange;
		this.label6.Location = new System.Drawing.Point(18, 15);
		this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(67, 25);
		this.label6.TabIndex = 79;
		this.label6.Text = "Power";
		this.rb_p_1.AutoSize = true;
		this.rb_p_1.Checked = true;
		this.rb_p_1.ForeColor = System.Drawing.Color.DarkOrange;
		this.rb_p_1.Location = new System.Drawing.Point(24, 45);
		this.rb_p_1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.rb_p_1.Name = "rb_p_1";
		this.rb_p_1.Size = new System.Drawing.Size(70, 29);
		this.rb_p_1.TabIndex = 80;
		this.rb_p_1.TabStop = true;
		this.rb_p_1.Text = "600";
		this.rb_p_1.UseVisualStyleBackColor = true;
		this.rb_p_1.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.checkBox2.BackColor = System.Drawing.Color.Black;
		this.checkBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.checkBox2.ForeColor = System.Drawing.Color.DodgerBlue;
		this.checkBox2.Location = new System.Drawing.Point(285, 220);
		this.checkBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.checkBox2.Name = "checkBox2";
		this.checkBox2.Size = new System.Drawing.Size(159, 29);
		this.checkBox2.TabIndex = 78;
		this.checkBox2.Text = "Fahrenheit";
		this.checkBox2.UseVisualStyleBackColor = false;
		this.checkBox2.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.checkBox1.BackColor = System.Drawing.Color.Black;
		this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.checkBox1.ForeColor = System.Drawing.Color.DodgerBlue;
		this.checkBox1.Location = new System.Drawing.Point(285, 185);
		this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(184, 29);
		this.checkBox1.TabIndex = 77;
		this.checkBox1.Text = "Always on top";
		this.checkBox1.UseVisualStyleBackColor = false;
		this.checkBox1.CheckedChanged += new System.EventHandler(checkBox1_CheckedChanged);
		this.checkBox1.Click += new System.EventHandler(rb_c_1_CheckedChanged);
		this.panel2.BackColor = System.Drawing.Color.Black;
		this.panel2.Controls.Add(this.linkLabel1);
		this.panel2.Controls.Add(this.exit_button);
		this.panel2.Controls.Add(this.op_label);
		this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel2.Location = new System.Drawing.Point(0, 0);
		this.panel2.Margin = new System.Windows.Forms.Padding(0);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(898, 54);
		this.panel2.TabIndex = 76;
		this.panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(panel2_MouseDown);
		this.panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(panel2_MouseMove);
		this.panel2.MouseUp += new System.Windows.Forms.MouseEventHandler(panel2_MouseUp);
		this.linkLabel1.AutoSize = true;
		this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.linkLabel1.LinkColor = System.Drawing.Color.Fuchsia;
		this.linkLabel1.Location = new System.Drawing.Point(6, 5);
		this.linkLabel1.Name = "linkLabel1";
		this.linkLabel1.Size = new System.Drawing.Size(32, 25);
		this.linkLabel1.TabIndex = 16;
		this.linkLabel1.TabStop = true;
		this.linkLabel1.Text = "@";
		this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
		this.exit_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.exit_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.exit_button.ForeColor = System.Drawing.Color.DarkRed;
		this.exit_button.Location = new System.Drawing.Point(866, 5);
		this.exit_button.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		this.exit_button.Name = "exit_button";
		this.exit_button.Size = new System.Drawing.Size(30, 31);
		this.exit_button.TabIndex = 15;
		this.exit_button.Text = "Х";
		this.exit_button.UseVisualStyleBackColor = true;
		this.exit_button.Click += new System.EventHandler(exit_button_Click);
		this.op_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.op_label.ForeColor = System.Drawing.Color.Silver;
		this.op_label.Location = new System.Drawing.Point(363, 11);
		this.op_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.op_label.Name = "op_label";
		this.op_label.Size = new System.Drawing.Size(156, 31);
		this.op_label.TabIndex = 11;
		this.op_label.Text = "Setup";
		this.op_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		base.AutoScaleDimensions = new System.Drawing.SizeF(9f, 20f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(900, 465);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "Setup";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Setup";
		base.TopMost = true;
		base.Load += new System.EventHandler(Setup_Load);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.panel6.ResumeLayout(false);
		this.panel6.PerformLayout();
		this.panel5.ResumeLayout(false);
		this.panel5.PerformLayout();
		this.panel4.ResumeLayout(false);
		this.panel4.PerformLayout();
		this.panel3.ResumeLayout(false);
		this.panel3.PerformLayout();
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		base.ResumeLayout(false);
	}
}
