using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using HeliosDX.Properties;

namespace HeliosDX;

public class Main : Form
{
	private string ST = "";

	private int p1;

	private int p2;

	private int p3;

	private int p4;

	private int p5;

	private int p6;

	private int p7;

	private int p8;

	private int p9;

	private int p10;

	private int p11;

	private int p12;

	private int p1_1;

	private int p2_1;

	private int p4_1;

	private int p12_1;

	private bool isMode;

	private string mode = "USB";

	private bool isExit;

	private bool byPass;

	private bool isMousePress;

	private int last_p9;

	private bool F;

	private Point _clickPoint;

	private Point _formStartPoint;

	private Socket tcpClient;

	private Socket udpClient;

	private Thread TCPthread;

	private Thread UDPthread;

	private int antenna;

	private string[] ant = new string[8];

	private bool volts;

	private bool b_cooling;

	private Label[] label_o = new Label[7];

	private string[] o_600 = new string[7] { "25", "100", "200", "300", "400", "500", "600" };

	private string[] o_1200 = new string[7] { "50", "200", "400", "600", "800", "1000", "1200" };

	private string[] o_2400 = new string[7] { "100", "400", "800", "1200", "1600", "2000", "2400" };

	private Label[] label_r = new Label[4];

	private string[] r_600 = new string[4] { "2", "10", "25", "50" };

	private string[] r_1200 = new string[4] { "5", "20", "50", "100" };

	private string[] r_2400 = new string[4] { "10", "40", "100", "200" };

	private Label[] label_c = new Label[4];

	private string[] c_600 = new string[4] { "5", "10", "15", "20" };

	private string[] c_1200 = new string[4] { "10", "20", "30", "40" };

	private string[] c_2400 = new string[4] { "20", "40", "60", "80" };

	private int koef = 1200;

	private bool sound;

	private bool inputIndicator;

	private int sizeind1 = 380;

	private int sizeind2 = 285;

	private int sizeind3 = 285;

	private Image cur3 = Resources.Current;

	private Image cur2 = Resources.Current2;

	private Image ref3 = Resources.Reverse;

	private Image ref2 = Resources.Reverse2;

	private SoundPlayer sp;

	private SoundPlayer sp1;

	private int cat;

	private int band;

	private int lastBand;

	private const int WS_MINIMIZEBOX = 131072;

	private const int CS_DBLCLKS = 8;

	private IContainer components;

	private SerialPort serialPort;

	private Label label2;

	private Label label3;

	private Label label4;

	private Label label5;

	private Label label6;

	private System.Windows.Forms.Timer op_timer;

	private System.Windows.Forms.Timer rp_timer;

	private System.Windows.Forms.Timer cr_timer;

	private System.Windows.Forms.Timer in_timer;

	private PictureBox rp_indicator_2;

	private Label AIR_label;

	private Label label17;

	private Label label_r_4;

	private Label label_r_3;

	private Label label_r_2;

	private Label error_label;

	private Label rp_label_value;

	private Button setup_button;

	private Label rp_label;

	private Button reset_button;

	private Label label_r_1;

	private Button byPass_button;

	private Label label_o_7;

	private Panel panel1;

	private Label label23;

	private Label label9;

	private Button exit_button;

	private Label op_label;

	private Label op_label_value;

	private Label label_o_6;

	private Panel panel2;

	private Label label_o_5;

	private Label label_o_4;

	private Panel panel4;

	private Label label_o_3;

	private Label label_o_1;

	private Label rp_right_line;

	private PictureBox op_indicator_2;

	private Label op_left_line;

	private Label op_right_line;

	private PictureBox op_indicator;

	private PictureBox cr_indicator_2;

	private Label label_c_3;

	private Label label_c_4;

	private Label label_c_2;

	private Label current_label_value;

	private Label label45;

	private Label label_c_1;

	private Label cr_left_line;

	private Label cr_right_line;

	private PictureBox cr_indicator;

	private Label label29;

	private Label label_o_2;

	private PictureBox in_indicator_2;

	private Label in_right_line;

	private Label in_left_line;

	private Label in_label_value;

	private Label label19;

	private Label label25;

	private Label label21;

	private Button sleep_button;

	private Button cooling_button;

	private Panel antenna_button;

	private Label a_text;

	private Label label28;

	private Panel band_button;

	private Label label34;

	private Label band_label_value;

	private Panel panel8;

	private Label swr_label_value;

	private Label label27;

	private Panel panel9;

	private Label efficiecy_label_value;

	private Label label30;

	private Label rp_left_line;

	private Panel volts_button;

	private Label voltage_label_value;

	private Label volts_text;

	private Panel panel11;

	private Label t_label_value;

	private Label t_label;

	private PictureBox rp_indicator;

	private PictureBox in_indicator;

	private Panel panel5;

	private Label label26;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams obj = base.CreateParams;
			obj.Style |= 131072;
			obj.ClassStyle |= 8;
			return obj;
		}
	}

	public Main()
	{
		InitializeComponent();
		Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		label_o = new Label[7] { label_o_1, label_o_2, label_o_3, label_o_4, label_o_5, label_o_6, label_o_7 };
		label_r = new Label[4] { label_r_1, label_r_2, label_r_3, label_r_4 };
		label_c = new Label[4] { label_c_1, label_c_2, label_c_3, label_c_4 };
	}

	private void Main_Load(object sender, EventArgs e)
	{
		sp = new SoundPlayer(Resources.Error);
		sp1 = new SoundPlayer(Resources.Alert);
		try
		{
			string[] array = File.ReadAllLines("save.txt");
			antenna = Convert.ToInt32(array[11]);
			a_text.Text = antenna.ToString();
			volts = bool.Parse(array[12]);
			if (bool.Parse(array[8]))
			{
				base.TopMost = true;
			}
			else
			{
				base.TopMost = false;
			}
			base.Location = new Point(int.Parse(array[2]), int.Parse(array[3]));
			if (bool.Parse(array[9]))
			{
				byPass = true;
			}
			else
			{
				byPass = false;
			}
			if (byPass)
			{
				byPass_button.BackColor = Color.Blue;
				byPass_button.ForeColor = Color.White;
			}
			else
			{
				byPass_button.BackColor = Color.Black;
				byPass_button.ForeColor = Color.DodgerBlue;
			}
			if (array[4] == "USB")
			{
				mode = "USB";
				OpenPort();
			}
			else
			{
				mode = "LAN";
				OpenLAN();
			}
			if (array[10] == "F")
			{
				F = true;
				t_label.Text = "Temp °F";
				t_label_value.Text = (18 * p3 / 10 + 32).ToString();
			}
			else
			{
				F = false;
				t_label.Text = "Temp °C";
				t_label_value.Text = p3.ToString();
			}
			ant = array[16].Split(',');
			koef = Convert.ToInt32(array[13]);
			if (bool.Parse(array[17]))
			{
				sound = true;
			}
			else
			{
				sound = false;
			}
			if (bool.Parse(array[18]))
			{
				inputIndicator = true;
			}
			else
			{
				inputIndicator = false;
			}
			cat = Convert.ToInt32(array[15]);
			IndicatorUpdate(koef);
		}
		catch
		{
			using StreamWriter streamWriter = new StreamWriter("save.txt", append: false);
			streamWriter.WriteLine("");
			streamWriter.WriteLine("115200");
			streamWriter.WriteLine(base.Location.X.ToString());
			streamWriter.WriteLine(base.Location.Y.ToString());
			streamWriter.WriteLine("USB");
			streamWriter.WriteLine("192.168.0.55");
			streamWriter.WriteLine("5005");
			streamWriter.WriteLine("5010");
			streamWriter.WriteLine("False");
			streamWriter.WriteLine("False");
			streamWriter.WriteLine("C");
			streamWriter.WriteLine("1");
			streamWriter.WriteLine("False");
			streamWriter.WriteLine("1200");
			streamWriter.WriteLine("48");
			streamWriter.WriteLine("0");
			streamWriter.WriteLine("1,1,1,1,1,1,1,1");
			streamWriter.WriteLine("True");
			streamWriter.WriteLine("False");
			streamWriter.Close();
		}
		if (inputIndicator)
		{
			threeIndicators();
		}
		else
		{
			twoIndicators();
		}
	}

	private void threeIndicators()
	{
		sizeind1 = 247;
		sizeind2 = 186;
		sizeind3 = 186;
		rp_label.Location = new Point(10, 90);
		rp_label_value.Location = new Point(105, 87);
		label_r_1.Location = new Point(32, 140);
		label_r_2.Location = new Point(68, 140);
		label_r_3.Location = new Point(111, 140);
		label_r_4.Location = new Point(159, 140);
		rp_left_line.Location = new Point(10, 115);
		rp_left_line.Size = new Size(186, 25);
		rp_right_line.Location = new Point(11, 115);
		rp_right_line.Size = new Size(4, 25);
		rp_indicator_2.Location = new Point(9, 115);
		rp_indicator_2.Size = new Size(186, 25);
		rp_indicator.Location = new Point(9, 115);
		rp_indicator.Size = new Size(1, 25);
		rp_indicator.Image = ref3;
		rp_indicator_2.Image = ref3;
		label19.Visible = true;
		in_label_value.Visible = true;
		label26.Visible = true;
		label21.Visible = true;
		label25.Visible = true;
		label17.Visible = true;
		in_left_line.Visible = true;
		in_indicator.Visible = true;
		in_indicator_2.Visible = true;
		in_right_line.Visible = true;
		label45.Location = new Point(407, 90);
		current_label_value.Location = new Point(476, 87);
		label_c_1.Location = new Point(426, 140);
		label_c_2.Location = new Point(463, 140);
		label_c_3.Location = new Point(500, 140);
		label_c_4.Location = new Point(537, 140);
		cr_left_line.Location = new Point(404, 115);
		cr_left_line.Size = new Size(186, 25);
		cr_right_line.Location = new Point(405, 115);
		cr_right_line.Size = new Size(4, 25);
		cr_indicator_2.Location = new Point(403, 115);
		cr_indicator_2.Size = new Size(186, 25);
		cr_indicator.Location = new Point(403, 115);
		cr_indicator.Size = new Size(1, 25);
		cr_indicator.Image = cur3;
		cr_indicator_2.Image = cur3;
	}

	private void twoIndicators()
	{
		sizeind1 = 380;
		sizeind2 = 285;
		sizeind3 = 285;
		rp_label.Location = new Point(10, 90);
		rp_label_value.Location = new Point(105, 87);
		label_r_1.Location = new Point(38, 140);
		label_r_2.Location = new Point(71, 140);
		label_r_3.Location = new Point(114, 140);
		label_r_4.Location = new Point(158, 140);
		rp_left_line.Location = new Point(10, 115);
		rp_left_line.Size = new Size(285, 25);
		rp_right_line.Location = new Point(11, 115);
		rp_right_line.Size = new Size(10, 25);
		rp_indicator_2.Location = new Point(9, 115);
		rp_indicator_2.Size = new Size(285, 25);
		rp_indicator.Location = new Point(9, 115);
		rp_indicator.Size = new Size(1, 25);
		rp_indicator.Image = ref2;
		rp_indicator_2.Image = ref2;
		label45.Location = new Point(307, 90);
		current_label_value.Location = new Point(377, 87);
		label_c_1.Location = new Point(347, 140);
		label_c_2.Location = new Point(403, 140);
		label_c_3.Location = new Point(460, 140);
		label_c_4.Location = new Point(517, 140);
		cr_left_line.Location = new Point(304, 115);
		cr_left_line.Size = new Size(285, 25);
		cr_right_line.Location = new Point(305, 115);
		cr_right_line.Size = new Size(10, 25);
		cr_indicator_2.Location = new Point(303, 115);
		cr_indicator_2.Size = new Size(285, 25);
		cr_indicator.Location = new Point(303, 115);
		cr_indicator.Size = new Size(1, 25);
		cr_indicator.Image = cur2;
		cr_indicator_2.Image = cur2;
		label19.Visible = false;
		in_label_value.Visible = false;
		label26.Visible = false;
		label21.Visible = false;
		label25.Visible = false;
		label17.Visible = false;
		in_left_line.Visible = false;
		in_indicator.Visible = false;
		in_indicator_2.Visible = false;
		in_right_line.Visible = false;
	}

	private void OpenLAN()
	{
		string[] array = File.ReadAllLines("save.txt");
		label23.Visible = true;
		label9.ForeColor = Color.Red;
		label9.Text = "IP: " + array[5];
		TCPthread = new Thread(TCP_DataReceived);
		UDPthread = new Thread(UDP_DataReceived);
		TCPthread.Start();
		UDPthread.Start();
		op_timer.Enabled = false;
		rp_timer.Enabled = false;
		formUpdate();
	}

	private void OpenPort()
	{
		string[] array = File.ReadAllLines("save.txt");
		label9.ForeColor = Color.Red;
		label23.Visible = true;
		label9.Text = array[0];
		serialPort.Close();
		try
		{
			if (array.Length != 0)
			{
				string portName = array[0];
				string s = array[1];
				serialPort.PortName = portName;
				serialPort.BaudRate = int.Parse(s);
				serialPort.Parity = Parity.None;
				serialPort.StopBits = StopBits.One;
				serialPort.DataBits = 8;
				serialPort.Handshake = Handshake.None;
				serialPort.DtrEnable = false;
				serialPort.WriteTimeout = 200;
				serialPort.ReadTimeout = 200;
				serialPort.Open();
				serialPort.WriteLine("11");
				Thread.Sleep(500);
				if (serialPort.IsOpen)
				{
					serialPort.Write("99");
					serialPort.Close();
				}
				serialPort.Open();
				serialPort.WriteLine("11");
				isMode = true;
				label9.ForeColor = Color.Green;
				label23.Visible = false;
			}
		}
		catch (Exception ex)
		{
			isMode = false;
			serialPort.Close();
			label23.Visible = false;
			MessageBox.Show("Ошибка в открытии порта! " + ex.Message);
		}
		op_timer.Enabled = false;
		rp_timer.Enabled = false;
		ST = "0,0,0,0,0,0,0,0";
		formUpdate();
	}

	private void TCP_DataReceived()
	{
		try
		{
			string[] array = File.ReadAllLines("save.txt");
			tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			tcpClient.Connect(IPAddress.Parse(array[5]), int.Parse(array[6]));
			Console.WriteLine("Отправляю сообщение...");
			tcpClient.Send(Encoding.UTF8.GetBytes("11"), SocketFlags.None);
			Console.WriteLine($"Подключение по TCP к {tcpClient.RemoteEndPoint} установленно");
			isMode = true;
			Invoke((MethodInvoker)delegate
			{
				label9.ForeColor = Color.Green;
				label23.Visible = false;
			});
		}
		catch
		{
			MessageBox.Show("Connection could not be established");
			Invoke((MethodInvoker)delegate
			{
				label9.ForeColor = Color.Red;
				label23.Visible = false;
			});
			return;
		}
		while (true)
		{
			if (isMode)
			{
				try
				{
					byte[] array2 = new byte[512];
					int count = tcpClient.Receive(array2, SocketFlags.None);
					string text = Encoding.UTF8.GetString(array2, 0, count);
					Console.WriteLine("Message received: " + text);
					if (text.Length > 0)
					{
						string[] array3 = text.Split(',');
						p3 = int.Parse(array3[0]);
						p5 = int.Parse(array3[1]);
						p6 = int.Parse(array3[2]);
						p7 = int.Parse(array3[3]);
						p8 = int.Parse(array3[4]);
						p9 = int.Parse(array3[5]);
						p10 = int.Parse(array3[6]);
						p11 = int.Parse(array3[7]);
					}
					Invoke((MethodInvoker)delegate
					{
						label9.ForeColor = Color.Green;
						label23.Visible = false;
						formUpdate();
					});
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					break;
				}
			}
			Thread.Sleep(10);
		}
	}

	private void UDP_DataReceived()
	{
		try
		{
			string[] array = File.ReadAllLines("save.txt");
			udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			udpClient.Connect(IPAddress.Parse(array[5]), int.Parse(array[7]));
			udpClient.Send(Encoding.UTF8.GetBytes("11"), SocketFlags.None);
			Console.WriteLine($"Подключение по UDP к {udpClient.RemoteEndPoint} установлено");
		}
		catch
		{
			MessageBox.Show("Не удалось установить соединение по UDP!");
			return;
		}
		while (true)
		{
			if (isMode)
			{
				try
				{
					byte[] array2 = new byte[512];
					int count = udpClient.Receive(array2, SocketFlags.None);
					string text = Encoding.UTF8.GetString(array2, 0, count);
					Console.WriteLine("Принято сообщение: " + text);
					string[] array3 = text.Split(',');
					p1 = int.Parse(array3[0]);
					p2 = int.Parse(array3[1]);
					p4 = int.Parse(array3[2]);
					p12 = int.Parse(array3[3]);
					Invoke((MethodInvoker)delegate
					{
						formUpdate1();
					});
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
					break;
				}
			}
			Thread.Sleep(10);
		}
	}

	private void formUpdate1()
	{
		if (p1 == 0 || p9 == 0)
		{
			op_indicator.Size = new Size(1, op_indicator.Size.Height);
			p1 = 0;
			p1_1 = 0;
		}
		else
		{
			Console.WriteLine(p1);
			op_indicator.Size = new Size(Map(p1, 0, 776, 0, 580), op_indicator.Size.Height);
		}
		if (p1_1 <= Map(p1, 0, 776, 0, 580))
		{
			p1_1 = Map(p1, 0, 776, 0, 580);
			op_timer.Stop();
			op_timer.Enabled = true;
			double value = 0.0;
			if (koef == 600)
			{
				value = p1 * p1 / 952;
			}
			else if (koef == 1200)
			{
				value = p1 * p1 / 476;
			}
			else if (koef == 2400)
			{
				value = p1 * p1 / 238;
			}
			op_label_value.Text = Convert.ToInt32(value).ToString();
		}
		op_left_line.Size = new Size(p1_1 - 5, op_left_line.Size.Height);
		op_right_line.Location = new Point(op_left_line.Location.X + p1_1 - 1, op_right_line.Location.Y);
		op_right_line.Size = new Size(580 - p1_1 + 1, op_right_line.Size.Height);
		if (p2 == 0 || p9 == 0)
		{
			rp_indicator.Size = new Size(1, rp_indicator.Size.Height);
			p2 = 0;
			p2_1 = 0;
		}
		else
		{
			rp_indicator.Size = new Size(Map(p2, 0, sizeind1, 0, sizeind2), rp_indicator.Size.Height);
		}
		if (p2_1 <= Map(p2, 0, sizeind1, 0, sizeind2))
		{
			p2_1 = Map(p2, 0, sizeind1, 0, sizeind2);
			rp_timer.Stop();
			rp_timer.Enabled = true;
			double value2 = 0.0;
			if (koef == 600)
			{
				value2 = p2 * p2 / 952;
			}
			else if (koef == 1200)
			{
				value2 = p2 * p2 / 476;
			}
			else if (koef == 2400)
			{
				value2 = p2 * p2 / 238;
			}
			rp_label_value.Text = Convert.ToInt32(value2).ToString();
		}
		rp_left_line.Size = new Size(p2_1 - 4, rp_left_line.Size.Height);
		rp_right_line.Location = new Point(rp_left_line.Location.X + p2_1 - 1, rp_right_line.Location.Y);
		rp_right_line.Size = new Size(sizeind3 - p2_1 + 1, rp_right_line.Size.Height);
		if (p4 == 0 || p9 == 0)
		{
			cr_indicator.Size = new Size(1, cr_indicator.Size.Height);
			p4 = 0;
			p4_1 = 0;
			current_label_value.Text = $"{Convert.ToDouble(p4) / 10.0: 0.0}";
		}
		else
		{
			cr_indicator.Size = new Size(Map(p4, 0, sizeind1, 0, sizeind2), cr_indicator.Size.Height);
		}
		if (p4_1 <= Map(p4, 0, sizeind1, 0, sizeind2))
		{
			p4_1 = Map(p4, 0, sizeind1, 0, sizeind2);
			cr_timer.Stop();
			cr_timer.Enabled = true;
			double num = 0.0;
			if (koef == 600 && !inputIndicator)
			{
				num = (double)p4 * 66.0 / 1000.0;
			}
			else if (koef == 600 && inputIndicator)
			{
				num = (double)p4 * 102.0 / 1000.0;
			}
			else if (koef == 1200 && !inputIndicator)
			{
				num = (double)p4 * 132.0 / 1000.0;
			}
			else if (koef == 1200 && inputIndicator)
			{
				num = (double)p4 * 204.0 / 1000.0;
			}
			else if (koef == 2400 && !inputIndicator)
			{
				num = (double)p4 * 264.0 / 1000.0;
			}
			else if (koef == 2400 && inputIndicator)
			{
				num = (double)p4 * 408.0 / 1000.0;
			}
			current_label_value.Text = $"{num: 0.0}";
		}
		cr_left_line.Size = new Size(p4_1 - 4, cr_left_line.Size.Height);
		cr_right_line.Location = new Point(cr_left_line.Location.X + p4_1 - 1, cr_right_line.Location.Y);
		cr_right_line.Size = new Size(sizeind3 - p4_1 + 1, cr_right_line.Size.Height);
		if (inputIndicator)
		{
			if (p12 == 0 || p9 == 0)
			{
				in_indicator.Size = new Size(1, in_indicator.Size.Height);
				p12 = 0;
				p12_1 = 0;
			}
			else
			{
				in_indicator.Size = new Size(Map(p12, 0, 247, 0, 186), in_indicator.Size.Height);
			}
			if (p12_1 <= Map(p12, 0, 247, 0, 186))
			{
				p12_1 = Map(p12, 0, 247, 0, 186);
				in_timer.Stop();
				in_timer.Enabled = true;
				in_label_value.Text = (p12 * p12 / 540).ToString();
			}
			in_left_line.Size = new Size(p12_1 - 4, in_left_line.Size.Height);
			in_right_line.Location = new Point(in_left_line.Location.X + p12_1 - 1, in_right_line.Location.Y);
			in_right_line.Size = new Size(186 - p12_1 + 1, in_right_line.Size.Height);
		}
		swr_label_value.Text = ((p1_1 == 0 || p2_1 == 0 || p1_1 <= p2_1) ? "1.00" : (((double)(p1_1 + p2_1) / (double)(p1_1 - p2_1) < 10.0) ? Math.Round((double)(p1_1 + p2_1) / (double)(p1_1 - p2_1), 2).ToString() : "9.99"));
		int num2 = 0;
		if (p5 / 10 * p4_1 / 10 == 0)
		{
			num2 = 0;
		}
		else if (koef == 600 && !inputIndicator)
		{
			num2 = p1 * p1 / 952 * 100 / (p5 / 10 * (p4 * 66 / 100) / 10);
		}
		else if (koef == 600 && inputIndicator)
		{
			num2 = p1 * p1 / 952 * 100 / (p5 / 10 * (p4 * 102 / 100) / 10);
		}
		else if (koef == 1200 && !inputIndicator)
		{
			num2 = p1 * p1 / 476 * 100 / (p5 / 10 * (p4 * 132 / 100) / 10);
		}
		else if (koef == 1200 && inputIndicator)
		{
			num2 = p1 * p1 / 476 * 100 / (p5 / 10 * (p4 * 204 / 100) / 10);
		}
		else if (koef == 2400 && !inputIndicator)
		{
			num2 = p1 * p1 / 238 * 100 / (p5 / 10 * (p4 * 264 / 100) / 10);
		}
		else if (koef == 2400 && inputIndicator)
		{
			num2 = p1 * p1 / 238 * 100 / (p5 / 10 * (p4 * 408 / 100) / 10);
		}
		efficiecy_label_value.Text = ((num2 < 100) ? num2.ToString() : "99");
	}

	private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		if (isMode)
		{
			op_label.Invoke((MethodInvoker)delegate
			{
				try
				{
					ST = serialPort.ReadLine();
					formUpdate();
					ST = "";
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			});
		}
		else if (!isMode && serialPort.IsOpen)
		{
			serialPort.Close();
		}
		if (isExit)
		{
			if (serialPort.IsOpen)
			{
				serialPort.Write("99");
				serialPort.Close();
			}
			Application.Exit();
		}
	}

	private void setup_button_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		if (p9 != 0)
		{
			return;
		}
		if (isMode)
		{
			if (mode == "USB")
			{
				serialPort.Write("99");
			}
			else
			{
				tcpClient.Send(Encoding.UTF8.GetBytes("99"), SocketFlags.None);
			}
		}
		isMode = false;
		if (mode == "LAN")
		{
			tcpClient.Close();
			udpClient.Close();
		}
		try
		{
			new Setup().ShowDialog();
			string[] array = File.ReadAllLines("save.txt");
			mode = array[4];
			if (mode == "USB")
			{
				OpenPort();
			}
			else
			{
				OpenLAN();
			}
			if (bool.Parse(array[8]))
			{
				base.TopMost = true;
			}
			else
			{
				base.TopMost = false;
			}
			if (array[10] == "F")
			{
				F = true;
				t_label.Text = "Temp °F";
				t_label_value.Text = (18 * p3 / 10 + 32).ToString();
			}
			else
			{
				F = false;
				t_label.Text = "Temp °C";
				t_label_value.Text = p3.ToString();
			}
			ant = array[16].Split(',');
			koef = Convert.ToInt32(array[13]);
			cat = Convert.ToInt32(array[15]) + 61;
			if (bool.Parse(array[17]))
			{
				sound = true;
			}
			else
			{
				sound = false;
			}
			if (bool.Parse(array[18]))
			{
				inputIndicator = true;
			}
			else
			{
				inputIndicator = false;
			}
			if (inputIndicator)
			{
				threeIndicators();
			}
			else
			{
				twoIndicators();
			}
			IndicatorUpdate(koef);
			if (mode == "USB")
			{
				serialPort.Write(cat.ToString());
			}
			else
			{
				tcpClient.Send(Encoding.UTF8.GetBytes(cat.ToString()), SocketFlags.None);
			}
			string s = "51";
			if (array[14] == "48")
			{
				s = "51";
			}
			else if (array[14] == "50")
			{
				s = "52";
			}
			else if (array[14] == "53.5")
			{
				s = "53";
			}
			else if (array[14] == "58.3")
			{
				s = "54";
			}
			if (mode == "USB")
			{
				serialPort.Write(s);
			}
			else
			{
				tcpClient.Send(Encoding.UTF8.GetBytes(s), SocketFlags.None);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private void exit_button_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		if (isMode)
		{
			if (mode == "USB")
			{
				if (serialPort.IsOpen)
				{
					serialPort.Write("99");
				}
			}
			else
			{
				try
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("99"), SocketFlags.None);
					tcpClient.Close();
				}
				catch
				{
					Application.Exit();
				}
			}
		}
		if (mode == "USB")
		{
			if (isMode)
			{
				isMode = false;
				isExit = true;
				Application.Exit();
			}
			else
			{
				Application.Exit();
			}
		}
		else
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	private void reset_button_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		try
		{
			if (p9 == 0 && isMode)
			{
				if (mode == "USB")
				{
					serialPort.Write("23");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("23"), SocketFlags.None);
				}
			}
		}
		catch
		{
			isMode = false;
			if (mode == "USB")
			{
				serialPort.Close();
			}
			else
			{
				OpenLAN();
			}
		}
	}

	private void ByPass_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		try
		{
			if (p9 == 0 && isMode)
			{
				if (!byPass)
				{
					byPass = !byPass;
					byPass_button.BackColor = Color.Blue;
					byPass_button.ForeColor = Color.White;
					if (mode == "USB")
					{
						serialPort.Write("21");
					}
					else
					{
						tcpClient.Send(Encoding.UTF8.GetBytes("21"), SocketFlags.None);
					}
				}
				else
				{
					byPass = !byPass;
					byPass_button.BackColor = Color.Black;
					byPass_button.ForeColor = Color.DodgerBlue;
					if (mode == "USB")
					{
						serialPort.Write("22");
					}
					else
					{
						tcpClient.Send(Encoding.UTF8.GetBytes("22"), SocketFlags.None);
					}
				}
			}
			try
			{
				string[] array = File.ReadAllLines("save.txt");
				using StreamWriter streamWriter = new StreamWriter("save.txt", append: false);
				if (array.Length != 0)
				{
					array[9] = byPass.ToString();
					for (int i = 0; i < array.Length; i++)
					{
						streamWriter.WriteLine(array[i]);
					}
				}
				streamWriter.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		catch
		{
			isMode = false;
			if (mode == "USB")
			{
				serialPort.Close();
			}
			else
			{
				OpenLAN();
			}
		}
	}

	private void op_timer_Tick(object sender, EventArgs e)
	{
		p1_1 = 0;
		op_timer.Enabled = false;
	}

	private void rp_timer_Tick(object sender, EventArgs e)
	{
		p2_1 = 0;
		rp_timer.Enabled = false;
	}

	private void Form1_MouseDown(object sender, MouseEventArgs e)
	{
		isMousePress = true;
		_clickPoint = Cursor.Position;
		_formStartPoint = base.Location;
	}

	private void IndicatorUpdate(int temp)
	{
		for (int i = 0; i < label_o.Length; i++)
		{
			switch (temp)
			{
			case 600:
				label_o[i].Text = o_600[i];
				break;
			case 1200:
				label_o[i].Text = o_1200[i];
				break;
			case 2400:
				label_o[i].Text = o_2400[i];
				break;
			}
		}
		for (int j = 0; j < label_r.Length; j++)
		{
			switch (temp)
			{
			case 600:
				label_r[j].Text = r_600[j];
				break;
			case 1200:
				label_r[j].Text = r_1200[j];
				break;
			case 2400:
				label_r[j].Text = r_2400[j];
				break;
			}
		}
		for (int k = 0; k < label_c.Length; k++)
		{
			switch (temp)
			{
			case 600:
				label_c[k].Text = c_600[k];
				break;
			case 1200:
				label_c[k].Text = c_1200[k];
				break;
			case 2400:
				label_c[k].Text = c_2400[k];
				break;
			}
		}
	}

	private void Form1_MouseMove(object sender, MouseEventArgs e)
	{
		if (isMousePress)
		{
			Point point = new Point(Cursor.Position.X - _clickPoint.X, Cursor.Position.Y - _clickPoint.Y);
			base.Location = new Point(_formStartPoint.X + point.X, _formStartPoint.Y + point.Y);
		}
	}

	private void Form1_MouseUp(object sender, MouseEventArgs e)
	{
		isMousePress = false;
		_clickPoint = Point.Empty;
		try
		{
			string[] array = File.ReadAllLines("save.txt");
			using StreamWriter streamWriter = new StreamWriter("save.txt", append: false);
			if (array.Length != 0)
			{
				array[2] = base.Location.X.ToString();
				array[3] = base.Location.Y.ToString();
				for (int i = 0; i < array.Length; i++)
				{
					streamWriter.WriteLine(array[i]);
				}
			}
			streamWriter.Close();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private void formUpdate()
	{
		volts_text.Text = (volts ? "Volts+" : "Volts");
		if (mode == "USB")
		{
			string[] array = ST.Split(',');
			if (array.Length == 4)
			{
				p1 = int.Parse(array[0]);
				p2 = int.Parse(array[1]);
				p4 = int.Parse(array[2]);
				p12 = int.Parse(array[3]);
			}
			else
			{
				p3 = int.Parse(array[0]);
				p5 = int.Parse(array[1]);
				p6 = int.Parse(array[2]);
				p7 = int.Parse(array[3]);
				p8 = int.Parse(array[4]);
				p9 = int.Parse(array[5]);
				p10 = int.Parse(array[6]);
				p11 = int.Parse(array[7]);
			}
		}
		if (p11 == 1)
		{
			byPass = true;
			byPass_button.BackColor = Color.Blue;
			byPass_button.ForeColor = Color.White;
		}
		else
		{
			byPass = false;
			byPass_button.BackColor = Color.Black;
			byPass_button.ForeColor = Color.DodgerBlue;
		}
		if (p10 == 1)
		{
			label29.Text = "Fan 100%";
			label29.ForeColor = Color.Magenta;
		}
		else
		{
			label29.Text = "Fan Auto";
			label29.ForeColor = Color.Green;
		}
		if (p9 == 0)
		{
			AIR_label.Text = "ON AIR";
			AIR_label.ForeColor = Color.Green;
			efficiecy_label_value.Text = "0";
			last_p9 = 0;
		}
		else
		{
			if (last_p9 == 0 && mode == "LAN")
			{
				UDPthread = new Thread(UDP_DataReceived);
				UDPthread.Start();
				last_p9 = 1;
			}
			AIR_label.Text = "ON AIR";
			AIR_label.ForeColor = Color.Red;
			int num = 0;
			if (p5 / 10 * p4_1 / 10 == 0)
			{
				num = 0;
			}
			else if (koef == 600 && !inputIndicator)
			{
				num = p1 * p1 / 952 * 100 / (p5 / 10 * (p4 * 66 / 100) / 10);
			}
			else if (koef == 600 && inputIndicator)
			{
				num = p1 * p1 / 952 * 100 / (p5 / 10 * (p4 * 102 / 100) / 10);
			}
			else if (koef == 1200 && !inputIndicator)
			{
				num = p1 * p1 / 476 * 100 / (p5 / 10 * (p4 * 132 / 100) / 10);
			}
			else if (koef == 1200 && inputIndicator)
			{
				num = p1 * p1 / 476 * 100 / (p5 / 10 * (p4 * 204 / 100) / 10);
			}
			else if (koef == 2400 && !inputIndicator)
			{
				num = p1 * p1 / 238 * 100 / (p5 / 10 * (p4 * 264 / 100) / 10);
			}
			else if (koef == 2400 && inputIndicator)
			{
				num = p1 * p1 / 238 * 100 / (p5 / 10 * (p4 * 408 / 100) / 10);
			}
			efficiecy_label_value.Text = ((num < 100) ? num.ToString() : "99");
		}
		if (p1 == 0 || p9 == 0)
		{
			op_indicator.Size = new Size(1, op_indicator.Size.Height);
			p1 = 0;
			p1_1 = 0;
		}
		else
		{
			Console.WriteLine(p1);
			op_indicator.Size = new Size(Map(p1, 0, 776, 0, 580), op_indicator.Size.Height);
		}
		if (p1_1 <= Map(p1, 0, 776, 0, 580))
		{
			p1_1 = Map(p1, 0, 776, 0, 580);
			op_timer.Stop();
			op_timer.Enabled = true;
			double value = 0.0;
			if (koef == 600)
			{
				value = p1 * p1 / 952;
			}
			else if (koef == 1200)
			{
				value = p1 * p1 / 476;
			}
			else if (koef == 2400)
			{
				value = p1 * p1 / 238;
			}
			op_label_value.Text = Convert.ToInt32(value).ToString();
		}
		op_left_line.Size = new Size(p1_1 - 5, op_left_line.Size.Height);
		op_right_line.Location = new Point(op_left_line.Location.X + p1_1 - 1, op_right_line.Location.Y);
		op_right_line.Size = new Size(580 - p1_1 + 1, op_right_line.Size.Height);
		if (p2 == 0 || p9 == 0)
		{
			rp_indicator.Size = new Size(1, rp_indicator.Size.Height);
			p2 = 0;
			p2_1 = 0;
		}
		else
		{
			rp_indicator.Size = new Size(Map(p2, 0, sizeind1, 0, sizeind2), rp_indicator.Size.Height);
		}
		if (p2_1 <= Map(p2, 0, sizeind1, 0, sizeind2))
		{
			p2_1 = Map(p2, 0, sizeind1, 0, sizeind2);
			rp_timer.Stop();
			rp_timer.Enabled = true;
			double value2 = 0.0;
			if (koef == 600)
			{
				value2 = p2 * p2 / 952;
			}
			else if (koef == 1200)
			{
				value2 = p2 * p2 / 476;
			}
			else if (koef == 2400)
			{
				value2 = p2 * p2 / 238;
			}
			rp_label_value.Text = Convert.ToInt32(value2).ToString();
		}
		rp_left_line.Size = new Size(p2_1 - 4, rp_left_line.Size.Height);
		rp_right_line.Location = new Point(rp_left_line.Location.X + p2_1 - 1, rp_right_line.Location.Y);
		rp_right_line.Size = new Size(sizeind3 - p2_1 + 1, rp_right_line.Size.Height);
		if (p4 == 0 || p9 == 0)
		{
			cr_indicator.Size = new Size(1, cr_indicator.Size.Height);
			p4 = 0;
			p4_1 = 0;
			current_label_value.Text = $"{Convert.ToDouble(p4) / 10.0: 0.00}";
		}
		else
		{
			cr_indicator.Size = new Size(Map(p4, 0, sizeind1, 0, sizeind2), cr_indicator.Size.Height);
		}
		if (p4_1 <= Map(p4, 0, sizeind1, 0, sizeind2))
		{
			p4_1 = Map(p4, 0, sizeind1, 0, sizeind2);
			cr_timer.Stop();
			cr_timer.Enabled = true;
			double num2 = 0.0;
			if (koef == 600 && !inputIndicator)
			{
				num2 = (double)p4 * 66.0 / 1000.0;
			}
			else if (koef == 600 && inputIndicator)
			{
				num2 = (double)p4 * 102.0 / 1000.0;
			}
			else if (koef == 1200 && !inputIndicator)
			{
				num2 = (double)p4 * 132.0 / 1000.0;
			}
			else if (koef == 1200 && inputIndicator)
			{
				num2 = (double)p4 * 204.0 / 1000.0;
			}
			else if (koef == 2400 && !inputIndicator)
			{
				num2 = (double)p4 * 264.0 / 1000.0;
			}
			else if (koef == 2400 && inputIndicator)
			{
				num2 = (double)p4 * 408.0 / 1000.0;
			}
			current_label_value.Text = $"{num2: 0.0}";
		}
		cr_left_line.Size = new Size(p4_1 - 4, cr_left_line.Size.Height);
		cr_right_line.Location = new Point(cr_left_line.Location.X + p4_1 - 1, cr_right_line.Location.Y);
		cr_right_line.Size = new Size(sizeind3 - p4_1 + 1, cr_right_line.Size.Height);
		if (inputIndicator)
		{
			if (p12 == 0 || p9 == 0)
			{
				in_indicator.Size = new Size(1, in_indicator.Size.Height);
				p12 = 0;
				p12_1 = 0;
			}
			else
			{
				in_indicator.Size = new Size(Map(p12, 0, 247, 0, 186), in_indicator.Size.Height);
			}
			if (p12_1 <= Map(p12, 0, 247, 0, 186))
			{
				p12_1 = Map(p12, 0, 247, 0, 186);
				in_timer.Stop();
				in_timer.Enabled = true;
				in_label_value.Text = (p12 * p12 / 476).ToString();
			}
			in_left_line.Size = new Size(p12_1 - 4, in_left_line.Size.Height);
			in_right_line.Location = new Point(in_left_line.Location.X + p12_1 - 1, in_right_line.Location.Y);
			in_right_line.Size = new Size(186 - p12_1 + 1, in_right_line.Size.Height);
		}
		swr_label_value.Text = ((p1_1 == 0 || p2_1 == 0 || p1_1 <= p2_1) ? "1.00" : (((double)(p1_1 + p2_1) / (double)(p1_1 - p2_1) < 10.0) ? Math.Round((double)(p1_1 + p2_1) / (double)(p1_1 - p2_1), 2).ToString() : "9.99"));
		_ = p5;
		voltage_label_value.Text = $"{Convert.ToDouble(p5) / 10.0: 0.0}";
		if (F)
		{
			t_label_value.Text = (18 * p3 / 10 + 32).ToString();
		}
		else
		{
			t_label_value.Text = p3.ToString();
		}
		switch (p6)
		{
		case 1:
			band_label_value.Text = "160";
			antenna = Convert.ToInt32(ant[0]);
			break;
		case 2:
			band_label_value.Text = "80";
			antenna = Convert.ToInt32(ant[1]);
			break;
		case 3:
			band_label_value.Text = "40";
			antenna = Convert.ToInt32(ant[2]);
			break;
		case 4:
			band_label_value.Text = "30";
			antenna = Convert.ToInt32(ant[3]);
			break;
		case 5:
			band_label_value.Text = "20";
			antenna = Convert.ToInt32(ant[4]);
			break;
		case 6:
			band_label_value.Text = "17-15";
			antenna = Convert.ToInt32(ant[5]);
			break;
		case 7:
			band_label_value.Text = "12-10";
			antenna = Convert.ToInt32(ant[6]);
			break;
		case 8:
			band_label_value.Text = "6";
			antenna = Convert.ToInt32(ant[7]);
			break;
		}
		band = p6;
		a_text.Text = antenna.ToString();
		try
		{
			if (band != lastBand)
			{
				lastBand = band;
				switch (antenna)
				{
				case 1:
					if (mode == "USB")
					{
						serialPort.Write("31");
					}
					else
					{
						tcpClient.Send(Encoding.UTF8.GetBytes("31"), SocketFlags.None);
					}
					break;
				case 2:
					if (mode == "USB")
					{
						serialPort.Write("32");
					}
					else
					{
						tcpClient.Send(Encoding.UTF8.GetBytes("32"), SocketFlags.None);
					}
					break;
				case 3:
					if (mode == "USB")
					{
						serialPort.Write("33");
					}
					else
					{
						tcpClient.Send(Encoding.UTF8.GetBytes("33"), SocketFlags.None);
					}
					break;
				case 4:
					if (mode == "USB")
					{
						serialPort.Write("34");
					}
					else
					{
						tcpClient.Send(Encoding.UTF8.GetBytes("34"), SocketFlags.None);
					}
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			return;
		}
		switch (p7)
		{
		case 1:
			antenna = 1;
			a_text.Text = antenna.ToString();
			break;
		case 2:
			antenna = 2;
			a_text.Text = antenna.ToString();
			break;
		case 3:
			antenna = 3;
			a_text.Text = antenna.ToString();
			break;
		}
		switch (p8)
		{
		case 0:
			error_label.Text = "Status OK";
			error_label.ForeColor = Color.Green;
			break;
		case 1:
			error_label.Text = "Error Input Power!";
			error_label.ForeColor = Color.Red;
			if (sound)
			{
				sp.Play();
			}
			break;
		case 2:
			error_label.Text = "Error Power";
			error_label.ForeColor = Color.Red;
			if (sound)
			{
				sp.Play();
			}
			break;
		case 3:
			error_label.Text = "Error REF!";
			error_label.ForeColor = Color.Red;
			if (sound)
			{
				sp.Play();
			}
			break;
		case 4:
			error_label.Text = "Error LPF!";
			error_label.ForeColor = Color.Red;
			if (sound)
			{
				sp.Play();
			}
			break;
		case 5:
			error_label.Text = "Error Current!";
			error_label.ForeColor = Color.Red;
			if (sound)
			{
				sp.Play();
			}
			break;
		case 6:
			error_label.Text = "Error Voltage!";
			error_label.ForeColor = Color.Red;
			if (sound)
			{
				sp.Play();
			}
			break;
		case 7:
			error_label.Text = "Error Temperaure!";
			error_label.ForeColor = Color.Red;
			if (sound)
			{
				sp.Play();
			}
			break;
		}
	}

	private void Form1_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (isMode)
		{
			try
			{
				if (mode == "USB")
				{
					serialPort.Write("99");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("99"), SocketFlags.None);
				}
			}
			catch
			{
				Application.Exit();
			}
		}
		if (mode == "USB")
		{
			if (isMode)
			{
				isMode = false;
				isExit = true;
				Process.GetCurrentProcess().Kill();
				Application.Exit();
			}
			else
			{
				Application.Exit();
			}
		}
		else
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	private void cr_timer_Tick(object sender, EventArgs e)
	{
		p4_1 = 0;
		cr_timer.Enabled = false;
	}

	private void antenna_button_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		try
		{
			if (!isMode || p9 != 0)
			{
				return;
			}
			antenna++;
			if (antenna > 3)
			{
				antenna = 1;
			}
			a_text.Text = antenna.ToString();
			if (antenna == 1)
			{
				if (mode == "USB")
				{
					serialPort.Write("31");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("31"), SocketFlags.None);
				}
			}
			else if (antenna == 2)
			{
				if (mode == "USB")
				{
					serialPort.Write("32");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("32"), SocketFlags.None);
				}
			}
			else if (antenna == 3)
			{
				if (mode == "USB")
				{
					serialPort.Write("33");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("33"), SocketFlags.None);
				}
			}
		}
		catch
		{
			isMode = false;
			if (mode == "USB")
			{
				serialPort.Close();
			}
			else
			{
				OpenLAN();
			}
		}
	}

	private void sleep_button_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		try
		{
			if (isMode && p9 == 0)
			{
				if (mode == "USB")
				{
					serialPort.Write("44");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("44"), SocketFlags.None);
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}

	private void volts_button_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		try
		{
			if (!isMode || p9 != 0)
			{
				return;
			}
			volts = !volts;
			volts_text.Text = (volts ? "Volts+" : "Volts");
			if (volts)
			{
				if (mode == "USB")
				{
					serialPort.Write("42");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("42"), SocketFlags.None);
				}
			}
			else if (mode == "USB")
			{
				serialPort.Write("41");
			}
			else
			{
				tcpClient.Send(Encoding.UTF8.GetBytes("41"), SocketFlags.None);
			}
			try
			{
				string[] array = File.ReadAllLines("save.txt");
				using StreamWriter streamWriter = new StreamWriter("save.txt", append: false);
				if (array.Length != 0)
				{
					array[12] = volts.ToString();
					for (int i = 0; i < array.Length; i++)
					{
						streamWriter.WriteLine(array[i]);
					}
				}
				streamWriter.Close();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		catch
		{
			isMode = false;
			if (mode == "USB")
			{
				serialPort.Close();
			}
			else
			{
				OpenLAN();
			}
		}
	}

	private void cooling_button_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		try
		{
			if (p9 != 0 || !isMode)
			{
				return;
			}
			if (!b_cooling)
			{
				b_cooling = !b_cooling;
				cooling_button.BackColor = Color.Blue;
				cooling_button.ForeColor = Color.White;
				if (mode == "USB")
				{
					serialPort.Write("45");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("45"), SocketFlags.None);
				}
			}
			else
			{
				b_cooling = !b_cooling;
				cooling_button.BackColor = Color.Black;
				cooling_button.ForeColor = Color.DodgerBlue;
				if (mode == "USB")
				{
					serialPort.Write("46");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("46"), SocketFlags.None);
				}
			}
		}
		catch
		{
			isMode = false;
			if (mode == "USB")
			{
				serialPort.Close();
			}
			else
			{
				OpenLAN();
			}
		}
	}

	private void in_timer_Tick(object sender, EventArgs e)
	{
		p12_1 = 0;
		in_timer.Enabled = false;
	}

	public static int Map(int x, int in_min, int in_max, int out_min, int out_max)
	{
		return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
	}

	private void band_button_Click(object sender, EventArgs e)
	{
		if (sound)
		{
			sp1.Play();
		}
		if (cat != 5)
		{
			return;
		}
		band++;
		if (band > 8)
		{
			band = 1;
		}
		switch (band)
		{
		case 1:
			band_label_value.Text = "160";
			antenna = Convert.ToInt32(ant[0]);
			break;
		case 2:
			band_label_value.Text = "80";
			antenna = Convert.ToInt32(ant[1]);
			break;
		case 3:
			band_label_value.Text = "40";
			antenna = Convert.ToInt32(ant[2]);
			break;
		case 4:
			band_label_value.Text = "30";
			antenna = Convert.ToInt32(ant[3]);
			break;
		case 5:
			band_label_value.Text = "20";
			antenna = Convert.ToInt32(ant[4]);
			break;
		case 6:
			band_label_value.Text = "17-15";
			antenna = Convert.ToInt32(ant[5]);
			break;
		case 7:
			band_label_value.Text = "12-10";
			antenna = Convert.ToInt32(ant[6]);
			break;
		case 8:
			band_label_value.Text = "6";
			antenna = Convert.ToInt32(ant[7]);
			break;
		}
		a_text.Text = antenna.ToString();
		try
		{
			switch (antenna)
			{
			case 1:
				if (mode == "USB")
				{
					serialPort.Write("31");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("31"), SocketFlags.None);
				}
				break;
			case 2:
				if (mode == "USB")
				{
					serialPort.Write("32");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("32"), SocketFlags.None);
				}
				break;
			case 3:
				if (mode == "USB")
				{
					serialPort.Write("33");
				}
				else
				{
					tcpClient.Send(Encoding.UTF8.GetBytes("33"), SocketFlags.None);
				}
				break;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeliosDX.Main));
		this.serialPort = new System.IO.Ports.SerialPort(this.components);
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.op_timer = new System.Windows.Forms.Timer(this.components);
		this.rp_timer = new System.Windows.Forms.Timer(this.components);
		this.cr_timer = new System.Windows.Forms.Timer(this.components);
		this.in_timer = new System.Windows.Forms.Timer(this.components);
		this.AIR_label = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.label_r_4 = new System.Windows.Forms.Label();
		this.label_r_3 = new System.Windows.Forms.Label();
		this.label_r_2 = new System.Windows.Forms.Label();
		this.error_label = new System.Windows.Forms.Label();
		this.rp_label_value = new System.Windows.Forms.Label();
		this.setup_button = new System.Windows.Forms.Button();
		this.rp_label = new System.Windows.Forms.Label();
		this.reset_button = new System.Windows.Forms.Button();
		this.label_r_1 = new System.Windows.Forms.Label();
		this.byPass_button = new System.Windows.Forms.Button();
		this.label_o_7 = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.label23 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.exit_button = new System.Windows.Forms.Button();
		this.op_label = new System.Windows.Forms.Label();
		this.op_label_value = new System.Windows.Forms.Label();
		this.label_o_6 = new System.Windows.Forms.Label();
		this.panel2 = new System.Windows.Forms.Panel();
		this.label_o_5 = new System.Windows.Forms.Label();
		this.label_o_4 = new System.Windows.Forms.Label();
		this.panel4 = new System.Windows.Forms.Panel();
		this.label_o_3 = new System.Windows.Forms.Label();
		this.label_o_1 = new System.Windows.Forms.Label();
		this.rp_right_line = new System.Windows.Forms.Label();
		this.op_left_line = new System.Windows.Forms.Label();
		this.op_right_line = new System.Windows.Forms.Label();
		this.label_c_3 = new System.Windows.Forms.Label();
		this.label_c_4 = new System.Windows.Forms.Label();
		this.label_c_2 = new System.Windows.Forms.Label();
		this.current_label_value = new System.Windows.Forms.Label();
		this.label45 = new System.Windows.Forms.Label();
		this.label_c_1 = new System.Windows.Forms.Label();
		this.cr_left_line = new System.Windows.Forms.Label();
		this.cr_right_line = new System.Windows.Forms.Label();
		this.label29 = new System.Windows.Forms.Label();
		this.label_o_2 = new System.Windows.Forms.Label();
		this.in_right_line = new System.Windows.Forms.Label();
		this.in_left_line = new System.Windows.Forms.Label();
		this.in_label_value = new System.Windows.Forms.Label();
		this.label19 = new System.Windows.Forms.Label();
		this.label25 = new System.Windows.Forms.Label();
		this.label21 = new System.Windows.Forms.Label();
		this.sleep_button = new System.Windows.Forms.Button();
		this.cooling_button = new System.Windows.Forms.Button();
		this.antenna_button = new System.Windows.Forms.Panel();
		this.a_text = new System.Windows.Forms.Label();
		this.label28 = new System.Windows.Forms.Label();
		this.band_button = new System.Windows.Forms.Panel();
		this.label34 = new System.Windows.Forms.Label();
		this.band_label_value = new System.Windows.Forms.Label();
		this.panel8 = new System.Windows.Forms.Panel();
		this.swr_label_value = new System.Windows.Forms.Label();
		this.label27 = new System.Windows.Forms.Label();
		this.panel9 = new System.Windows.Forms.Panel();
		this.efficiecy_label_value = new System.Windows.Forms.Label();
		this.label30 = new System.Windows.Forms.Label();
		this.rp_left_line = new System.Windows.Forms.Label();
		this.volts_button = new System.Windows.Forms.Panel();
		this.voltage_label_value = new System.Windows.Forms.Label();
		this.volts_text = new System.Windows.Forms.Label();
		this.panel11 = new System.Windows.Forms.Panel();
		this.t_label_value = new System.Windows.Forms.Label();
		this.t_label = new System.Windows.Forms.Label();
		this.panel5 = new System.Windows.Forms.Panel();
		this.label26 = new System.Windows.Forms.Label();
		this.in_indicator = new System.Windows.Forms.PictureBox();
		this.rp_indicator = new System.Windows.Forms.PictureBox();
		this.in_indicator_2 = new System.Windows.Forms.PictureBox();
		this.cr_indicator = new System.Windows.Forms.PictureBox();
		this.cr_indicator_2 = new System.Windows.Forms.PictureBox();
		this.op_indicator = new System.Windows.Forms.PictureBox();
		this.op_indicator_2 = new System.Windows.Forms.PictureBox();
		this.rp_indicator_2 = new System.Windows.Forms.PictureBox();
		this.antenna_button.SuspendLayout();
		this.band_button.SuspendLayout();
		this.panel8.SuspendLayout();
		this.panel9.SuspendLayout();
		this.volts_button.SuspendLayout();
		this.panel11.SuspendLayout();
		this.panel5.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.in_indicator).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.rp_indicator).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.in_indicator_2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cr_indicator).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.cr_indicator_2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.op_indicator).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.op_indicator_2).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.rp_indicator_2).BeginInit();
		base.SuspendLayout();
		this.serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPort1_DataReceived);
		this.label2.Location = new System.Drawing.Point(0, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(100, 23);
		this.label2.TabIndex = 0;
		this.label3.Location = new System.Drawing.Point(0, 0);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(100, 23);
		this.label3.TabIndex = 0;
		this.label4.Location = new System.Drawing.Point(0, 0);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(100, 23);
		this.label4.TabIndex = 0;
		this.label5.Location = new System.Drawing.Point(0, 0);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(100, 23);
		this.label5.TabIndex = 0;
		this.label6.Location = new System.Drawing.Point(0, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(100, 23);
		this.label6.TabIndex = 0;
		this.op_timer.Interval = 1500;
		this.op_timer.Tick += new System.EventHandler(op_timer_Tick);
		this.rp_timer.Interval = 1500;
		this.rp_timer.Tick += new System.EventHandler(rp_timer_Tick);
		this.cr_timer.Interval = 1500;
		this.cr_timer.Tick += new System.EventHandler(cr_timer_Tick);
		this.in_timer.Interval = 1500;
		this.in_timer.Tick += new System.EventHandler(in_timer_Tick);
		this.AIR_label.Font = new System.Drawing.Font("Hero", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.AIR_label.ForeColor = System.Drawing.Color.Green;
		this.AIR_label.Location = new System.Drawing.Point(248, 211);
		this.AIR_label.Name = "AIR_label";
		this.AIR_label.Size = new System.Drawing.Size(100, 27);
		this.AIR_label.TabIndex = 54;
		this.AIR_label.Text = "ON AIR";
		this.AIR_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label17.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label17.ForeColor = System.Drawing.Color.Red;
		this.label17.Location = new System.Drawing.Point(356, 140);
		this.label17.Margin = new System.Windows.Forms.Padding(0);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(30, 18);
		this.label17.TabIndex = 38;
		this.label17.Text = "100";
		this.label17.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_r_4.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_r_4.ForeColor = System.Drawing.Color.Red;
		this.label_r_4.Location = new System.Drawing.Point(158, 140);
		this.label_r_4.Margin = new System.Windows.Forms.Padding(0);
		this.label_r_4.Name = "label_r_4";
		this.label_r_4.Size = new System.Drawing.Size(30, 18);
		this.label_r_4.TabIndex = 37;
		this.label_r_4.Text = "100";
		this.label_r_4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_r_3.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_r_3.ForeColor = System.Drawing.Color.Yellow;
		this.label_r_3.Location = new System.Drawing.Point(112, 140);
		this.label_r_3.Margin = new System.Windows.Forms.Padding(0);
		this.label_r_3.Name = "label_r_3";
		this.label_r_3.Size = new System.Drawing.Size(30, 18);
		this.label_r_3.TabIndex = 36;
		this.label_r_3.Text = "50";
		this.label_r_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_r_2.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_r_2.ForeColor = System.Drawing.Color.White;
		this.label_r_2.Location = new System.Drawing.Point(71, 140);
		this.label_r_2.Margin = new System.Windows.Forms.Padding(0);
		this.label_r_2.Name = "label_r_2";
		this.label_r_2.Size = new System.Drawing.Size(30, 18);
		this.label_r_2.TabIndex = 35;
		this.label_r_2.Text = "20";
		this.label_r_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.error_label.Font = new System.Drawing.Font("Hero", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.error_label.ForeColor = System.Drawing.Color.Green;
		this.error_label.Location = new System.Drawing.Point(16, 211);
		this.error_label.Name = "error_label";
		this.error_label.Size = new System.Drawing.Size(224, 27);
		this.error_label.TabIndex = 67;
		this.error_label.Text = "Status OK";
		this.error_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.rp_label_value.AutoSize = true;
		this.rp_label_value.Font = new System.Drawing.Font("Hero", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rp_label_value.ForeColor = System.Drawing.Color.Lime;
		this.rp_label_value.Location = new System.Drawing.Point(105, 87);
		this.rp_label_value.Name = "rp_label_value";
		this.rp_label_value.Size = new System.Drawing.Size(24, 27);
		this.rp_label_value.TabIndex = 33;
		this.rp_label_value.Text = "0";
		this.setup_button.BackColor = System.Drawing.Color.Black;
		this.setup_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.setup_button.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.setup_button.ForeColor = System.Drawing.Color.DodgerBlue;
		this.setup_button.Location = new System.Drawing.Point(475, 242);
		this.setup_button.Name = "setup_button";
		this.setup_button.Size = new System.Drawing.Size(110, 35);
		this.setup_button.TabIndex = 70;
		this.setup_button.Text = "Setup";
		this.setup_button.UseVisualStyleBackColor = false;
		this.setup_button.Click += new System.EventHandler(setup_button_Click);
		this.rp_label.AutoSize = true;
		this.rp_label.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.rp_label.ForeColor = System.Drawing.Color.Gray;
		this.rp_label.Location = new System.Drawing.Point(10, 90);
		this.rp_label.Name = "rp_label";
		this.rp_label.Size = new System.Drawing.Size(98, 23);
		this.rp_label.TabIndex = 32;
		this.rp_label.Text = "Reflected";
		this.reset_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.reset_button.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.reset_button.ForeColor = System.Drawing.Color.DodgerBlue;
		this.reset_button.Location = new System.Drawing.Point(15, 242);
		this.reset_button.Name = "reset_button";
		this.reset_button.Size = new System.Drawing.Size(110, 35);
		this.reset_button.TabIndex = 71;
		this.reset_button.Text = "Reset";
		this.reset_button.UseVisualStyleBackColor = false;
		this.reset_button.Click += new System.EventHandler(reset_button_Click);
		this.label_r_1.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_r_1.ForeColor = System.Drawing.Color.White;
		this.label_r_1.Location = new System.Drawing.Point(38, 140);
		this.label_r_1.Margin = new System.Windows.Forms.Padding(0);
		this.label_r_1.Name = "label_r_1";
		this.label_r_1.Size = new System.Drawing.Size(30, 18);
		this.label_r_1.TabIndex = 31;
		this.label_r_1.Text = "5";
		this.label_r_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.byPass_button.BackColor = System.Drawing.Color.Black;
		this.byPass_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.byPass_button.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.byPass_button.ForeColor = System.Drawing.Color.DodgerBlue;
		this.byPass_button.Location = new System.Drawing.Point(245, 242);
		this.byPass_button.Name = "byPass_button";
		this.byPass_button.Size = new System.Drawing.Size(110, 35);
		this.byPass_button.TabIndex = 72;
		this.byPass_button.Text = "ByPass";
		this.byPass_button.UseVisualStyleBackColor = false;
		this.byPass_button.Click += new System.EventHandler(ByPass_Click);
		this.label_o_7.Font = new System.Drawing.Font("Hero", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_o_7.ForeColor = System.Drawing.Color.Red;
		this.label_o_7.Location = new System.Drawing.Point(552, 65);
		this.label_o_7.Name = "label_o_7";
		this.label_o_7.Size = new System.Drawing.Size(45, 19);
		this.label_o_7.TabIndex = 26;
		this.label_o_7.Text = "1200";
		this.label_o_7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.panel1.BackColor = System.Drawing.Color.Black;
		this.panel1.Location = new System.Drawing.Point(381, 0);
		this.panel1.Margin = new System.Windows.Forms.Padding(0);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(194, 30);
		this.panel1.TabIndex = 75;
		this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(Form1_MouseDown);
		this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(Form1_MouseMove);
		this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(Form1_MouseUp);
		this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 6f, System.Drawing.FontStyle.Bold);
		this.label23.ForeColor = System.Drawing.Color.Gray;
		this.label23.Location = new System.Drawing.Point(3, 18);
		this.label23.Name = "label23";
		this.label23.Size = new System.Drawing.Size(77, 10);
		this.label23.TabIndex = 88;
		this.label23.Text = "Connection...";
		this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label23.Visible = false;
		this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 7f);
		this.label9.ForeColor = System.Drawing.Color.Red;
		this.label9.Location = new System.Drawing.Point(3, 1);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(169, 18);
		this.label9.TabIndex = 87;
		this.label9.Text = "IP: 192.168.0.55";
		this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.exit_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.exit_button.Font = new System.Drawing.Font("Microsoft Sans Serif", 8f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.exit_button.ForeColor = System.Drawing.Color.DarkRed;
		this.exit_button.Location = new System.Drawing.Point(577, 1);
		this.exit_button.Name = "exit_button";
		this.exit_button.Size = new System.Drawing.Size(20, 20);
		this.exit_button.TabIndex = 15;
		this.exit_button.Text = "Х";
		this.exit_button.UseVisualStyleBackColor = true;
		this.exit_button.Click += new System.EventHandler(exit_button_Click);
		this.op_label.Font = new System.Drawing.Font("Hero Light", 20f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.op_label.ForeColor = System.Drawing.Color.Gray;
		this.op_label.Location = new System.Drawing.Point(179, 2);
		this.op_label.Margin = new System.Windows.Forms.Padding(0);
		this.op_label.Name = "op_label";
		this.op_label.Size = new System.Drawing.Size(115, 30);
		this.op_label.TabIndex = 11;
		this.op_label.Text = "Output";
		this.op_label_value.Font = new System.Drawing.Font("Hero", 24f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.op_label_value.ForeColor = System.Drawing.Color.Lime;
		this.op_label_value.Location = new System.Drawing.Point(285, -2);
		this.op_label_value.Margin = new System.Windows.Forms.Padding(0);
		this.op_label_value.Name = "op_label_value";
		this.op_label_value.Size = new System.Drawing.Size(96, 34);
		this.op_label_value.TabIndex = 12;
		this.op_label_value.Text = "0";
		this.label_o_6.Font = new System.Drawing.Font("Hero", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_o_6.ForeColor = System.Drawing.Color.Yellow;
		this.label_o_6.Location = new System.Drawing.Point(503, 65);
		this.label_o_6.Margin = new System.Windows.Forms.Padding(0);
		this.label_o_6.Name = "label_o_6";
		this.label_o_6.Size = new System.Drawing.Size(45, 19);
		this.label_o_6.TabIndex = 25;
		this.label_o_6.Text = "1000";
		this.label_o_6.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.panel2.BackColor = System.Drawing.Color.Black;
		this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
		this.panel2.Location = new System.Drawing.Point(0, 85);
		this.panel2.Margin = new System.Windows.Forms.Padding(0);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(601, 1);
		this.panel2.TabIndex = 76;
		this.label_o_5.Font = new System.Drawing.Font("Hero", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_o_5.ForeColor = System.Drawing.Color.White;
		this.label_o_5.Location = new System.Drawing.Point(449, 65);
		this.label_o_5.Name = "label_o_5";
		this.label_o_5.Size = new System.Drawing.Size(45, 19);
		this.label_o_5.TabIndex = 24;
		this.label_o_5.Text = "800";
		this.label_o_5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_o_4.Font = new System.Drawing.Font("Hero", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_o_4.ForeColor = System.Drawing.Color.White;
		this.label_o_4.Location = new System.Drawing.Point(387, 65);
		this.label_o_4.Name = "label_o_4";
		this.label_o_4.Size = new System.Drawing.Size(45, 19);
		this.label_o_4.TabIndex = 23;
		this.label_o_4.Text = "600";
		this.label_o_4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.panel4.BackColor = System.Drawing.Color.Black;
		this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel4.Location = new System.Drawing.Point(0, 240);
		this.panel4.Name = "panel4";
		this.panel4.Size = new System.Drawing.Size(601, 1);
		this.panel4.TabIndex = 78;
		this.label_o_3.Font = new System.Drawing.Font("Hero", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_o_3.ForeColor = System.Drawing.Color.White;
		this.label_o_3.Location = new System.Drawing.Point(313, 65);
		this.label_o_3.Name = "label_o_3";
		this.label_o_3.Size = new System.Drawing.Size(45, 19);
		this.label_o_3.TabIndex = 22;
		this.label_o_3.Text = "400";
		this.label_o_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_o_1.Font = new System.Drawing.Font("Hero", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_o_1.ForeColor = System.Drawing.Color.White;
		this.label_o_1.Location = new System.Drawing.Point(102, 65);
		this.label_o_1.Name = "label_o_1";
		this.label_o_1.Size = new System.Drawing.Size(45, 19);
		this.label_o_1.TabIndex = 21;
		this.label_o_1.Text = "50";
		this.label_o_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.rp_right_line.BackColor = System.Drawing.Color.FromArgb(25, 30, 30);
		this.rp_right_line.Location = new System.Drawing.Point(11, 115);
		this.rp_right_line.Name = "rp_right_line";
		this.rp_right_line.Size = new System.Drawing.Size(4, 25);
		this.rp_right_line.TabIndex = 84;
		this.op_left_line.BackColor = System.Drawing.Color.FromArgb(25, 30, 30);
		this.op_left_line.Location = new System.Drawing.Point(10, 35);
		this.op_left_line.Margin = new System.Windows.Forms.Padding(0);
		this.op_left_line.Name = "op_left_line";
		this.op_left_line.Size = new System.Drawing.Size(4, 30);
		this.op_left_line.TabIndex = 86;
		this.op_right_line.BackColor = System.Drawing.Color.FromArgb(25, 30, 30);
		this.op_right_line.Location = new System.Drawing.Point(9, 35);
		this.op_right_line.Margin = new System.Windows.Forms.Padding(0);
		this.op_right_line.Name = "op_right_line";
		this.op_right_line.Size = new System.Drawing.Size(580, 30);
		this.op_right_line.TabIndex = 85;
		this.label_c_3.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_c_3.ForeColor = System.Drawing.Color.Yellow;
		this.label_c_3.Location = new System.Drawing.Point(512, 140);
		this.label_c_3.Margin = new System.Windows.Forms.Padding(0);
		this.label_c_3.Name = "label_c_3";
		this.label_c_3.Size = new System.Drawing.Size(30, 18);
		this.label_c_3.TabIndex = 106;
		this.label_c_3.Text = "30";
		this.label_c_3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_c_4.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_c_4.ForeColor = System.Drawing.Color.Red;
		this.label_c_4.Location = new System.Drawing.Point(557, 140);
		this.label_c_4.Margin = new System.Windows.Forms.Padding(0);
		this.label_c_4.Name = "label_c_4";
		this.label_c_4.Size = new System.Drawing.Size(30, 18);
		this.label_c_4.TabIndex = 105;
		this.label_c_4.Text = "40";
		this.label_c_4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label_c_2.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_c_2.ForeColor = System.Drawing.Color.White;
		this.label_c_2.Location = new System.Drawing.Point(469, 140);
		this.label_c_2.Margin = new System.Windows.Forms.Padding(0);
		this.label_c_2.Name = "label_c_2";
		this.label_c_2.Size = new System.Drawing.Size(30, 18);
		this.label_c_2.TabIndex = 104;
		this.label_c_2.Text = "20";
		this.label_c_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.current_label_value.AutoSize = true;
		this.current_label_value.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.current_label_value.Font = new System.Drawing.Font("Hero", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.current_label_value.ForeColor = System.Drawing.Color.Lime;
		this.current_label_value.Location = new System.Drawing.Point(479, 87);
		this.current_label_value.Name = "current_label_value";
		this.current_label_value.Size = new System.Drawing.Size(39, 27);
		this.current_label_value.TabIndex = 103;
		this.current_label_value.Text = "0.0";
		this.current_label_value.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label45.AutoSize = true;
		this.label45.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label45.ForeColor = System.Drawing.Color.Gray;
		this.label45.Location = new System.Drawing.Point(406, 90);
		this.label45.Name = "label45";
		this.label45.Size = new System.Drawing.Size(78, 23);
		this.label45.TabIndex = 102;
		this.label45.Text = "Current";
		this.label_c_1.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_c_1.ForeColor = System.Drawing.Color.White;
		this.label_c_1.Location = new System.Drawing.Point(426, 140);
		this.label_c_1.Margin = new System.Windows.Forms.Padding(0);
		this.label_c_1.Name = "label_c_1";
		this.label_c_1.Size = new System.Drawing.Size(30, 18);
		this.label_c_1.TabIndex = 101;
		this.label_c_1.Text = "10";
		this.label_c_1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.cr_left_line.BackColor = System.Drawing.Color.FromArgb(25, 30, 30);
		this.cr_left_line.Location = new System.Drawing.Point(403, 115);
		this.cr_left_line.Name = "cr_left_line";
		this.cr_left_line.Size = new System.Drawing.Size(185, 25);
		this.cr_left_line.TabIndex = 108;
		this.cr_right_line.BackColor = System.Drawing.Color.FromArgb(25, 30, 30);
		this.cr_right_line.Location = new System.Drawing.Point(403, 115);
		this.cr_right_line.Name = "cr_right_line";
		this.cr_right_line.Size = new System.Drawing.Size(10, 25);
		this.cr_right_line.TabIndex = 109;
		this.label29.Font = new System.Drawing.Font("Hero", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label29.ForeColor = System.Drawing.Color.Green;
		this.label29.Location = new System.Drawing.Point(415, 211);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(120, 27);
		this.label29.TabIndex = 110;
		this.label29.Text = "Fan Auto";
		this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label_o_2.Font = new System.Drawing.Font("Hero", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label_o_2.ForeColor = System.Drawing.Color.White;
		this.label_o_2.Location = new System.Drawing.Point(218, 65);
		this.label_o_2.Name = "label_o_2";
		this.label_o_2.Size = new System.Drawing.Size(45, 19);
		this.label_o_2.TabIndex = 116;
		this.label_o_2.Text = "200";
		this.label_o_2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.in_right_line.BackColor = System.Drawing.Color.FromArgb(25, 30, 30);
		this.in_right_line.Location = new System.Drawing.Point(208, 115);
		this.in_right_line.Name = "in_right_line";
		this.in_right_line.Size = new System.Drawing.Size(4, 25);
		this.in_right_line.TabIndex = 120;
		this.in_left_line.BackColor = System.Drawing.Color.FromArgb(25, 30, 30);
		this.in_left_line.Location = new System.Drawing.Point(206, 115);
		this.in_left_line.Name = "in_left_line";
		this.in_left_line.Size = new System.Drawing.Size(186, 25);
		this.in_left_line.TabIndex = 121;
		this.in_label_value.AutoSize = true;
		this.in_label_value.Font = new System.Drawing.Font("Hero", 16f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.in_label_value.ForeColor = System.Drawing.Color.Lime;
		this.in_label_value.Location = new System.Drawing.Point(262, 87);
		this.in_label_value.Name = "in_label_value";
		this.in_label_value.Size = new System.Drawing.Size(24, 27);
		this.in_label_value.TabIndex = 123;
		this.in_label_value.Text = "0";
		this.label19.AutoSize = true;
		this.label19.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label19.ForeColor = System.Drawing.Color.Gray;
		this.label19.Location = new System.Drawing.Point(208, 90);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(57, 23);
		this.label19.TabIndex = 122;
		this.label19.Text = "Input";
		this.label25.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label25.ForeColor = System.Drawing.Color.Yellow;
		this.label25.Location = new System.Drawing.Point(307, 140);
		this.label25.Margin = new System.Windows.Forms.Padding(0);
		this.label25.Name = "label25";
		this.label25.Size = new System.Drawing.Size(30, 18);
		this.label25.TabIndex = 126;
		this.label25.Text = "50";
		this.label25.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.label21.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label21.ForeColor = System.Drawing.Color.White;
		this.label21.Location = new System.Drawing.Point(265, 140);
		this.label21.Margin = new System.Windows.Forms.Padding(0);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(30, 18);
		this.label21.TabIndex = 124;
		this.label21.Text = "20";
		this.label21.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.sleep_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.sleep_button.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.sleep_button.ForeColor = System.Drawing.Color.DodgerBlue;
		this.sleep_button.Location = new System.Drawing.Point(130, 242);
		this.sleep_button.Name = "sleep_button";
		this.sleep_button.Size = new System.Drawing.Size(110, 35);
		this.sleep_button.TabIndex = 129;
		this.sleep_button.Text = "Sleep";
		this.sleep_button.UseVisualStyleBackColor = false;
		this.sleep_button.Click += new System.EventHandler(sleep_button_Click);
		this.cooling_button.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
		this.cooling_button.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.cooling_button.ForeColor = System.Drawing.Color.DodgerBlue;
		this.cooling_button.Location = new System.Drawing.Point(360, 242);
		this.cooling_button.Name = "cooling_button";
		this.cooling_button.Size = new System.Drawing.Size(110, 35);
		this.cooling_button.TabIndex = 130;
		this.cooling_button.Text = "Cooling";
		this.cooling_button.UseVisualStyleBackColor = false;
		this.cooling_button.Click += new System.EventHandler(cooling_button_Click);
		this.antenna_button.BackColor = System.Drawing.Color.Black;
		this.antenna_button.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.antenna_button.Controls.Add(this.a_text);
		this.antenna_button.Controls.Add(this.label28);
		this.antenna_button.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.antenna_button.Location = new System.Drawing.Point(-1, 160);
		this.antenna_button.Name = "antenna_button";
		this.antenna_button.Size = new System.Drawing.Size(101, 50);
		this.antenna_button.TabIndex = 131;
		this.antenna_button.Click += new System.EventHandler(antenna_button_Click);
		this.a_text.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.a_text.ForeColor = System.Drawing.Color.DodgerBlue;
		this.a_text.Location = new System.Drawing.Point(5, 0);
		this.a_text.Name = "a_text";
		this.a_text.Size = new System.Drawing.Size(90, 25);
		this.a_text.TabIndex = 129;
		this.a_text.Text = "1";
		this.a_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.a_text.Click += new System.EventHandler(antenna_button_Click);
		this.label28.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label28.ForeColor = System.Drawing.Color.Gray;
		this.label28.Location = new System.Drawing.Point(5, 25);
		this.label28.MinimumSize = new System.Drawing.Size(0, 19);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(90, 19);
		this.label28.TabIndex = 128;
		this.label28.Text = "Antenna";
		this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label28.Click += new System.EventHandler(antenna_button_Click);
		this.band_button.BackColor = System.Drawing.Color.Black;
		this.band_button.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.band_button.Controls.Add(this.label34);
		this.band_button.Controls.Add(this.band_label_value);
		this.band_button.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.band_button.Location = new System.Drawing.Point(98, 160);
		this.band_button.Name = "band_button";
		this.band_button.Size = new System.Drawing.Size(101, 50);
		this.band_button.TabIndex = 132;
		this.band_button.Click += new System.EventHandler(band_button_Click);
		this.label34.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label34.ForeColor = System.Drawing.Color.Gray;
		this.label34.Location = new System.Drawing.Point(5, 25);
		this.label34.Margin = new System.Windows.Forms.Padding(0);
		this.label34.MaximumSize = new System.Drawing.Size(90, 19);
		this.label34.MinimumSize = new System.Drawing.Size(90, 19);
		this.label34.Name = "label34";
		this.label34.Size = new System.Drawing.Size(90, 19);
		this.label34.TabIndex = 63;
		this.label34.Text = "Band";
		this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label34.Click += new System.EventHandler(band_button_Click);
		this.band_label_value.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.band_label_value.ForeColor = System.Drawing.Color.DarkGoldenrod;
		this.band_label_value.Location = new System.Drawing.Point(5, 0);
		this.band_label_value.Margin = new System.Windows.Forms.Padding(0);
		this.band_label_value.Name = "band_label_value";
		this.band_label_value.Size = new System.Drawing.Size(90, 25);
		this.band_label_value.TabIndex = 64;
		this.band_label_value.Text = "160";
		this.band_label_value.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.band_label_value.Click += new System.EventHandler(band_button_Click);
		this.panel8.BackColor = System.Drawing.Color.Black;
		this.panel8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel8.Controls.Add(this.swr_label_value);
		this.panel8.Controls.Add(this.label27);
		this.panel8.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.panel8.Location = new System.Drawing.Point(198, 160);
		this.panel8.Name = "panel8";
		this.panel8.Size = new System.Drawing.Size(101, 50);
		this.panel8.TabIndex = 133;
		this.swr_label_value.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.swr_label_value.ForeColor = System.Drawing.Color.LimeGreen;
		this.swr_label_value.Location = new System.Drawing.Point(3, 0);
		this.swr_label_value.Margin = new System.Windows.Forms.Padding(0);
		this.swr_label_value.Name = "swr_label_value";
		this.swr_label_value.Size = new System.Drawing.Size(90, 25);
		this.swr_label_value.TabIndex = 56;
		this.swr_label_value.Text = "1.00";
		this.swr_label_value.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label27.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label27.ForeColor = System.Drawing.Color.Gray;
		this.label27.Location = new System.Drawing.Point(5, 25);
		this.label27.Margin = new System.Windows.Forms.Padding(0);
		this.label27.MaximumSize = new System.Drawing.Size(90, 19);
		this.label27.MinimumSize = new System.Drawing.Size(90, 19);
		this.label27.Name = "label27";
		this.label27.Size = new System.Drawing.Size(90, 19);
		this.label27.TabIndex = 55;
		this.label27.Text = "SWR";
		this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.panel9.BackColor = System.Drawing.Color.Black;
		this.panel9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel9.Controls.Add(this.efficiecy_label_value);
		this.panel9.Controls.Add(this.label30);
		this.panel9.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.panel9.Location = new System.Drawing.Point(298, 160);
		this.panel9.Name = "panel9";
		this.panel9.Size = new System.Drawing.Size(101, 50);
		this.panel9.TabIndex = 134;
		this.efficiecy_label_value.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.efficiecy_label_value.ForeColor = System.Drawing.Color.LimeGreen;
		this.efficiecy_label_value.Location = new System.Drawing.Point(3, 0);
		this.efficiecy_label_value.Margin = new System.Windows.Forms.Padding(0);
		this.efficiecy_label_value.Name = "efficiecy_label_value";
		this.efficiecy_label_value.Size = new System.Drawing.Size(90, 25);
		this.efficiecy_label_value.TabIndex = 62;
		this.efficiecy_label_value.Text = "0";
		this.efficiecy_label_value.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label30.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label30.ForeColor = System.Drawing.Color.Gray;
		this.label30.Location = new System.Drawing.Point(5, 25);
		this.label30.Margin = new System.Windows.Forms.Padding(0);
		this.label30.MaximumSize = new System.Drawing.Size(90, 19);
		this.label30.MinimumSize = new System.Drawing.Size(90, 19);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(90, 19);
		this.label30.TabIndex = 61;
		this.label30.Text = "Eff %";
		this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.rp_left_line.BackColor = System.Drawing.Color.FromArgb(25, 30, 30);
		this.rp_left_line.Location = new System.Drawing.Point(9, 115);
		this.rp_left_line.Name = "rp_left_line";
		this.rp_left_line.Size = new System.Drawing.Size(186, 25);
		this.rp_left_line.TabIndex = 83;
		this.volts_button.AutoSize = true;
		this.volts_button.BackColor = System.Drawing.Color.Black;
		this.volts_button.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.volts_button.Controls.Add(this.voltage_label_value);
		this.volts_button.Controls.Add(this.volts_text);
		this.volts_button.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.volts_button.Location = new System.Drawing.Point(398, 160);
		this.volts_button.Name = "volts_button";
		this.volts_button.Size = new System.Drawing.Size(101, 50);
		this.volts_button.TabIndex = 135;
		this.volts_button.Click += new System.EventHandler(volts_button_Click);
		this.voltage_label_value.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.voltage_label_value.ForeColor = System.Drawing.Color.DarkGoldenrod;
		this.voltage_label_value.Location = new System.Drawing.Point(3, 0);
		this.voltage_label_value.Margin = new System.Windows.Forms.Padding(0);
		this.voltage_label_value.Name = "voltage_label_value";
		this.voltage_label_value.Size = new System.Drawing.Size(90, 25);
		this.voltage_label_value.TabIndex = 91;
		this.voltage_label_value.Text = "0.0";
		this.voltage_label_value.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.voltage_label_value.Click += new System.EventHandler(volts_button_Click);
		this.volts_text.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.volts_text.ForeColor = System.Drawing.Color.Gray;
		this.volts_text.Location = new System.Drawing.Point(5, 25);
		this.volts_text.Margin = new System.Windows.Forms.Padding(0);
		this.volts_text.MaximumSize = new System.Drawing.Size(90, 19);
		this.volts_text.MinimumSize = new System.Drawing.Size(90, 19);
		this.volts_text.Name = "volts_text";
		this.volts_text.Size = new System.Drawing.Size(90, 19);
		this.volts_text.TabIndex = 90;
		this.volts_text.Text = "Volts";
		this.volts_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.volts_text.Click += new System.EventHandler(volts_button_Click);
		this.panel11.BackColor = System.Drawing.Color.Black;
		this.panel11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel11.Controls.Add(this.t_label_value);
		this.panel11.Controls.Add(this.t_label);
		this.panel11.ForeColor = System.Drawing.SystemColors.ActiveCaption;
		this.panel11.Location = new System.Drawing.Point(498, 160);
		this.panel11.Name = "panel11";
		this.panel11.Size = new System.Drawing.Size(101, 50);
		this.panel11.TabIndex = 135;
		this.t_label_value.Font = new System.Drawing.Font("Hero", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.t_label_value.ForeColor = System.Drawing.Color.DodgerBlue;
		this.t_label_value.Location = new System.Drawing.Point(3, 0);
		this.t_label_value.Margin = new System.Windows.Forms.Padding(0);
		this.t_label_value.Name = "t_label_value";
		this.t_label_value.Size = new System.Drawing.Size(90, 25);
		this.t_label_value.TabIndex = 30;
		this.t_label_value.Text = "0";
		this.t_label_value.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.t_label.AutoSize = true;
		this.t_label.Font = new System.Drawing.Font("Hero", 14f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.t_label.ForeColor = System.Drawing.Color.Gray;
		this.t_label.Location = new System.Drawing.Point(5, 25);
		this.t_label.Margin = new System.Windows.Forms.Padding(0);
		this.t_label.MaximumSize = new System.Drawing.Size(90, 19);
		this.t_label.MinimumSize = new System.Drawing.Size(90, 19);
		this.t_label.Name = "t_label";
		this.t_label.Size = new System.Drawing.Size(90, 19);
		this.t_label.TabIndex = 44;
		this.t_label.Text = "Temp °C";
		this.t_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.panel5.AutoSize = true;
		this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panel5.Controls.Add(this.exit_button);
		this.panel5.Controls.Add(this.label23);
		this.panel5.Controls.Add(this.op_label_value);
		this.panel5.Controls.Add(this.op_label);
		this.panel5.Controls.Add(this.in_indicator);
		this.panel5.Controls.Add(this.label9);
		this.panel5.Controls.Add(this.rp_indicator);
		this.panel5.Controls.Add(this.panel11);
		this.panel5.Controls.Add(this.volts_button);
		this.panel5.Controls.Add(this.rp_left_line);
		this.panel5.Controls.Add(this.panel9);
		this.panel5.Controls.Add(this.panel8);
		this.panel5.Controls.Add(this.band_button);
		this.panel5.Controls.Add(this.antenna_button);
		this.panel5.Controls.Add(this.cooling_button);
		this.panel5.Controls.Add(this.sleep_button);
		this.panel5.Controls.Add(this.label26);
		this.panel5.Controls.Add(this.label21);
		this.panel5.Controls.Add(this.label25);
		this.panel5.Controls.Add(this.label19);
		this.panel5.Controls.Add(this.in_label_value);
		this.panel5.Controls.Add(this.in_left_line);
		this.panel5.Controls.Add(this.in_right_line);
		this.panel5.Controls.Add(this.in_indicator_2);
		this.panel5.Controls.Add(this.label_o_2);
		this.panel5.Controls.Add(this.label29);
		this.panel5.Controls.Add(this.cr_indicator);
		this.panel5.Controls.Add(this.cr_right_line);
		this.panel5.Controls.Add(this.cr_left_line);
		this.panel5.Controls.Add(this.label_c_1);
		this.panel5.Controls.Add(this.label45);
		this.panel5.Controls.Add(this.current_label_value);
		this.panel5.Controls.Add(this.label_c_2);
		this.panel5.Controls.Add(this.label_c_4);
		this.panel5.Controls.Add(this.label_c_3);
		this.panel5.Controls.Add(this.cr_indicator_2);
		this.panel5.Controls.Add(this.op_indicator);
		this.panel5.Controls.Add(this.op_right_line);
		this.panel5.Controls.Add(this.op_left_line);
		this.panel5.Controls.Add(this.op_indicator_2);
		this.panel5.Controls.Add(this.rp_right_line);
		this.panel5.Controls.Add(this.label_o_1);
		this.panel5.Controls.Add(this.label_o_3);
		this.panel5.Controls.Add(this.panel4);
		this.panel5.Controls.Add(this.label_o_4);
		this.panel5.Controls.Add(this.label_o_5);
		this.panel5.Controls.Add(this.panel2);
		this.panel5.Controls.Add(this.label_o_6);
		this.panel5.Controls.Add(this.panel1);
		this.panel5.Controls.Add(this.label_o_7);
		this.panel5.Controls.Add(this.byPass_button);
		this.panel5.Controls.Add(this.label_r_1);
		this.panel5.Controls.Add(this.reset_button);
		this.panel5.Controls.Add(this.rp_label);
		this.panel5.Controls.Add(this.setup_button);
		this.panel5.Controls.Add(this.rp_label_value);
		this.panel5.Controls.Add(this.error_label);
		this.panel5.Controls.Add(this.label_r_2);
		this.panel5.Controls.Add(this.label_r_3);
		this.panel5.Controls.Add(this.label_r_4);
		this.panel5.Controls.Add(this.label17);
		this.panel5.Controls.Add(this.AIR_label);
		this.panel5.Controls.Add(this.rp_indicator_2);
		this.panel5.Location = new System.Drawing.Point(0, 0);
		this.panel5.MaximumSize = new System.Drawing.Size(600, 280);
		this.panel5.Name = "panel5";
		this.panel5.Size = new System.Drawing.Size(600, 280);
		this.panel5.TabIndex = 87;
		this.label26.Font = new System.Drawing.Font("Hero", 11f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 204);
		this.label26.ForeColor = System.Drawing.Color.White;
		this.label26.Location = new System.Drawing.Point(229, 140);
		this.label26.Margin = new System.Windows.Forms.Padding(0);
		this.label26.Name = "label26";
		this.label26.Size = new System.Drawing.Size(30, 18);
		this.label26.TabIndex = 127;
		this.label26.Text = "5";
		this.label26.TextAlign = System.Drawing.ContentAlignment.TopCenter;
		this.in_indicator.BackColor = System.Drawing.Color.Transparent;
		this.in_indicator.Image = HeliosDX.Properties.Resources.Input;
		this.in_indicator.Location = new System.Drawing.Point(206, 115);
		this.in_indicator.Name = "in_indicator";
		this.in_indicator.Size = new System.Drawing.Size(1, 25);
		this.in_indicator.TabIndex = 119;
		this.in_indicator.TabStop = false;
		this.rp_indicator.BackColor = System.Drawing.Color.Transparent;
		this.rp_indicator.Image = (System.Drawing.Image)resources.GetObject("rp_indicator.Image");
		this.rp_indicator.Location = new System.Drawing.Point(9, 115);
		this.rp_indicator.Name = "rp_indicator";
		this.rp_indicator.Size = new System.Drawing.Size(1, 25);
		this.rp_indicator.TabIndex = 27;
		this.rp_indicator.TabStop = false;
		this.in_indicator_2.Image = HeliosDX.Properties.Resources.Input;
		this.in_indicator_2.Location = new System.Drawing.Point(206, 115);
		this.in_indicator_2.Name = "in_indicator_2";
		this.in_indicator_2.Size = new System.Drawing.Size(186, 25);
		this.in_indicator_2.TabIndex = 118;
		this.in_indicator_2.TabStop = false;
		this.cr_indicator.BackColor = System.Drawing.Color.Transparent;
		this.cr_indicator.Image = (System.Drawing.Image)resources.GetObject("cr_indicator.Image");
		this.cr_indicator.Location = new System.Drawing.Point(403, 115);
		this.cr_indicator.Name = "cr_indicator";
		this.cr_indicator.Size = new System.Drawing.Size(1, 25);
		this.cr_indicator.TabIndex = 99;
		this.cr_indicator.TabStop = false;
		this.cr_indicator_2.Image = (System.Drawing.Image)resources.GetObject("cr_indicator_2.Image");
		this.cr_indicator_2.Location = new System.Drawing.Point(403, 115);
		this.cr_indicator_2.Name = "cr_indicator_2";
		this.cr_indicator_2.Size = new System.Drawing.Size(185, 25);
		this.cr_indicator_2.TabIndex = 100;
		this.cr_indicator_2.TabStop = false;
		this.op_indicator.BackColor = System.Drawing.Color.Transparent;
		this.op_indicator.Image = HeliosDX.Properties.Resources.Power;
		this.op_indicator.Location = new System.Drawing.Point(9, 35);
		this.op_indicator.Name = "op_indicator";
		this.op_indicator.Size = new System.Drawing.Size(1, 30);
		this.op_indicator.TabIndex = 9;
		this.op_indicator.TabStop = false;
		this.op_indicator_2.Image = HeliosDX.Properties.Resources.Power;
		this.op_indicator_2.Location = new System.Drawing.Point(9, 35);
		this.op_indicator_2.Margin = new System.Windows.Forms.Padding(0);
		this.op_indicator_2.Name = "op_indicator_2";
		this.op_indicator_2.Size = new System.Drawing.Size(580, 30);
		this.op_indicator_2.TabIndex = 18;
		this.op_indicator_2.TabStop = false;
		this.rp_indicator_2.Image = (System.Drawing.Image)resources.GetObject("rp_indicator_2.Image");
		this.rp_indicator_2.Location = new System.Drawing.Point(9, 115);
		this.rp_indicator_2.Name = "rp_indicator_2";
		this.rp_indicator_2.Size = new System.Drawing.Size(186, 25);
		this.rp_indicator_2.TabIndex = 30;
		this.rp_indicator_2.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Black;
		base.ClientSize = new System.Drawing.Size(600, 280);
		base.Controls.Add(this.panel5);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.Name = "Main";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		base.TopMost = true;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form1_FormClosing);
		base.Load += new System.EventHandler(Main_Load);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(Form1_MouseDown);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(Form1_MouseMove);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(Form1_MouseUp);
		this.antenna_button.ResumeLayout(false);
		this.band_button.ResumeLayout(false);
		this.panel8.ResumeLayout(false);
		this.panel9.ResumeLayout(false);
		this.volts_button.ResumeLayout(false);
		this.panel11.ResumeLayout(false);
		this.panel11.PerformLayout();
		this.panel5.ResumeLayout(false);
		this.panel5.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.in_indicator).EndInit();
		((System.ComponentModel.ISupportInitialize)this.rp_indicator).EndInit();
		((System.ComponentModel.ISupportInitialize)this.in_indicator_2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cr_indicator).EndInit();
		((System.ComponentModel.ISupportInitialize)this.cr_indicator_2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.op_indicator).EndInit();
		((System.ComponentModel.ISupportInitialize)this.op_indicator_2).EndInit();
		((System.ComponentModel.ISupportInitialize)this.rp_indicator_2).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
