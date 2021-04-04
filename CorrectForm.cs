using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Sockets1_ClientSide_AustinK
{
    
    public partial class CorrectForm : Form
    {
        public delVoidVoid formReset;
        private Random rand = new Random();
        public CorrectForm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
            this.ForeColor = Color.FromArgb(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
        }

        private void returnButton_Click(object sender, EventArgs e)
        {
            if (formReset != null)
            {
                formReset.Invoke();
            }
            this.Close();
        }
    }
}
