using System.Collections.Immutable;
using System.Data;

namespace test1
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataTable table = new DataTable("table");
            table.Columns.Add("col1");
            DataRow row = table.NewRow();
            row["col1"] = "Kg";
            table.Rows.Add(row);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string meyveler, sebzeler;
            int kg;
            meyveler = comboBox2.Text;
            sebzeler = comboBox1.Text;
            kg = Convert.ToInt16(textBox1.Text);

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}