using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.Globalization ;

namespace WBrowser
{
    public partial class Open : Form
    {
        WebBrowser wb;
        public Open(WebBrowser wb)
        {
            this.wb = wb;
             InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            wb.Navigate(textBox1.Text);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Text Files(*.txt)|*.txt|Html file(*.html)|*.html|AllFiles|*.*";
            if (o.ShowDialog() == DialogResult.OK)
                textBox1.Text = o.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                wb.Navigate(textBox1.Text);
                this.Close();
            }
        }
        ResourceManager res_man;    
        CultureInfo cul;
       private void switch_language()
        {
            //if(j==1)
            cul = CultureInfo.CreateSpecificCulture("hi");
            //else 
                //if(j==0)
                    cul = CultureInfo.CreateSpecificCulture("en");
            this.Text = res_man.GetString("Open_text", cul);
            button1.Text = res_man.GetString("Button1_text", cul);
            button2.Text = res_man.GetString("Button2_text", cul);
            button3.Text = res_man.GetString("Button3_text", cul);
            label1.Text = res_man.GetString("Label1_text", cul);
            textBox1 .Text = res_man.GetString("Textbox1_text", cul);
        }

        private void Open_Load(object sender, EventArgs e)
        {
            res_man = new ResourceManager("WBrowser.Resource.Res", typeof(WBrowser).Assembly);
            switch_language();
        }
    }
}
