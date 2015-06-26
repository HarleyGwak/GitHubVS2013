using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Graphic_Monitoring_TXT
{
    public partial class Form1 : Form
    {
        bool isFirst = true;
        long startTime;
        int moteid, speed, winlen, x_pos, y_pos;
        string type = null;

        Thread fThread = null;
        List<string> arrSrc = new List<string>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cb_type.SelectedIndex = 2;
            type = this.cb_type.SelectedItem.ToString();
        }

       

        private void btn_start_Click(object sender, EventArgs e)
        {
            
            if(isFirst)
            {
                btn_start.Text = "PAUSE";
                isFirst = false;
                fThread = new Thread(new ThreadStart(go2Work));
                fThread.Start();
            }
            else
            {
                if (btn_start.Text == "START")
                {
                    btn_start.Text = "PAUSE";
                    fThread.Resume();
                }
                else
                {
                    btn_start.Text = "START";
                    fThread.Suspend();
                }
            }
        }

        private void go2Work()
        {
            DataTable rawData = new DataTable();
            rawData.Columns.Add("x_value", Type.GetType("System.String"));
            rawData.Columns.Add("y_value", Type.GetType("System.Double"));

            
            if (!isReady())
            {
                MessageBox.Show("입력해 주세요!");
                return;
            }

            startTime = Convert.ToInt64(tb_startTime.Text);
            moteid = Convert.ToInt32(tb_moteid.Text);
            speed = Convert.ToInt32(tb_speed.Text);
            winlen = Convert.ToInt32(tb_winlen.Text);
            x_pos = Convert.ToInt32(tb_x_pos.Text);
            y_pos = Convert.ToInt32(tb_y_pos.Text);

            for (int i = 0; i < arrSrc.Count; i++)
            {
                string line = "";
                System.IO.StreamReader file = new System.IO.StreamReader(arrSrc[i]);
                while (line != null)
                {
                    string[] result = new string[10];
                    line = file.ReadLine();
                    if (line != null && !line.Equals(""))
                    {
                        result = line.Split('\t');
                        double x = Convert.ToDouble(result[5]);
                        double y = Convert.ToDouble(result[6]);
                        double z = Convert.ToDouble(result[7]);
                        double et = Convert.ToDouble(result[9]);

                        if (Convert.ToInt32(result[1]) != moteid || Convert.ToInt64(result[2]) < startTime)
                            continue;
                        else
                        {
                            DataRow dr = rawData.NewRow();
                            dr["x_value"] = result[2] + "." + result[3];
                            switch(type)
                            {
                                case "x": dr["y_value"] = x; break;
                                case "y": dr["y_value"] = y; break;
                                case "z": dr["y_value"] = z; break;
                                case "xy": dr["y_value"] = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)); break;
                                case "xz": dr["y_value"] = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(z, 2)); break;
                                case "yz": dr["y_value"] = Math.Sqrt(Math.Pow(y, 2) + Math.Pow(z, 2)); break;
                                case "xyz": dr["y_value"] = et; break;
                                default: MessageBox.Show("y축 값 유형을 선택해 주세요!"); break;
                            }
                            rawData.Rows.Add(dr);
                        }
                    }
                    if (rawData.Rows.Count >= winlen)
                    {
                        this.Invoke((EventHandler)(delegate //清除画板
                        {
                            chart1.Series[0].Points.Clear(); 
                            PicPaint(ref rawData, ref chart1);
                            
                        }));
                        Thread currentThread = Thread.CurrentThread;//得到当前线程
                        Thread.Sleep(speed); //让线程休息5秒钟 
                        rawData.Rows.RemoveAt(0);
                    }
                }
            }
        }

        /// <summary>
        /// 画图程序
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="chart"></param>
        private void PicPaint(ref DataTable rawData, ref  System.Windows.Forms.DataVisualization.Charting.Chart chart)
        { 
            //将数据传给换图组件Chart
            for (int row = 0; row < rawData.Rows.Count; row++)
            {
                chart.Series[0].Points.AddXY(rawData.Rows[row][0], Convert.ToDouble(rawData.Rows[row][1])); 
            }
            chart.DataBind();
        }
         


        private bool isReady()
        {
            if (tb_startTime.Text == "" || tb_src.Text == "" || tb_moteid.Text == "" || tb_speed.Text == "" || tb_winlen.Text == "" || tb_x_pos.Text == "" || tb_y_pos.Text == "")
                return false;
            return true;
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            fThread.Abort();
            isFirst = true;
        }


        private void btn_getdir_Click(object sender, EventArgs e)
        {
            Stream mystream;
            OpenFileDialog of = new OpenFileDialog();
            of.InitialDirectory = "I:\\Farm Project\\第二次daelim农场实验\\测试数据\\测试数据集合";
            this.tb_src.Clear();
            of.Multiselect = true; 
            of.Filter="txt files(*.txt)|*.txt|All files(*.*)|*.*";
            of.FilterIndex = 2;
            of.RestoreDirectory = true;
            if (of.ShowDialog() == DialogResult.OK)
            {
                if ((mystream = of.OpenFile()) != null)
                {
                    this.tb_src.Text = "";
                    for (int fi = 0; fi < of.FileNames.Length; fi++)
                    {
                        this.tb_src.Text += of.FileNames[fi].ToString();
                        this.arrSrc.Add(of.FileNames[fi].ToString());
                    }
                    mystream.Close();
                }
            }
        }

        private void cb_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            type = this.cb_type.SelectedItem.ToString();
        }

        private void tb_speed_TextChanged(object sender, EventArgs e)
        {
            speed = Convert.ToInt32(this.tb_speed.Text);
        }
    }
}
