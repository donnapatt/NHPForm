using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NHP_Container_OCR_Form
{
    public partial class Form1 : Form
    {
        private PictureBox title = new PictureBox(); // create a PictureBox
        private Label minimise = new Label(); // this doesn't even have to be a label!
        private Label maximise = new Label(); // this will simulate our this.maximise box
        private Label close = new Label(); // simulates the this.close box
        private bool drag = false; // determine if we should be moving the form
        private Point startPoint = new Point(0, 0); // also for the moving

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.DarkSalmon;
            setData();
            setData();


        }

        private void setData()
        {
            int n;
            n = dataGridView1.Rows.Add();
            dataGridView1.Rows[n].DefaultCellStyle.BackColor = Color.LightSalmon;
            dataGridView1.Rows[n].DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.Rows[n].Cells[0].Value = "EGHU3562025";
            dataGridView1.Rows[n].Cells[1].Value = "55-9876";
            dataGridView1.Rows[n].Cells[2].Value = "19/7/18";
            dataGridView1.Rows[n].Cells[3].Value = "10.10";

            n = dataGridView1.Rows.Add();
            dataGridView1.Rows[n].DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.Rows[n].Cells[0].Value = "UAEU1242493";
            dataGridView1.Rows[n].Cells[1].Value = "45-3454";
            dataGridView1.Rows[n].Cells[2].Value = "19/7/18";
            dataGridView1.Rows[n].Cells[3].Value = "14.09";

            n = dataGridView1.Rows.Add();
            dataGridView1.Rows[n].DefaultCellStyle.BackColor = Color.LightSalmon;
            dataGridView1.Rows[n].DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.Rows[n].Cells[0].Value = "OOLU6506251";
            dataGridView1.Rows[n].Cells[1].Value = "12-3333";
            dataGridView1.Rows[n].Cells[2].Value = "19/7/18";
            dataGridView1.Rows[n].Cells[3].Value = "08.25";
        }
    }
}
