// 6-7
// OleDbCommandBuilder = SqlCommandBuilder

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ChapterSix6_1
{
    public partial class frmPhoneDB : Form
    {
        SqlConnection phoneConnection;
        SqlCommand phoneCommand;
        SqlDataAdapter phoneAdapter;
        DataTable phoneTable;
        CurrencyManager phoneManager;

        // 6-2 
        string myState;
        int myBookmark; 

        public frmPhoneDB()
        {
            InitializeComponent();
        }

        private void frmPhoneDB_Load(object sender, EventArgs e)
        {

            SetState("View"); 

            phoneConnection = new SqlConnection(@"Data Source=.\SQLEXPRESS; 
                                                AttachDbFilename=c:\Users\mholmes022726\source\repos\ChapterSix6-1\ChapterSix6-1\bin\Debug\netcoreapp3.1\SQLPhoneDB.mdf; 
                                                Integrated Security=True; Connect Timeout=30; User Instance=True");
            phoneConnection.Open();

            // establish command object   
            phoneCommand = new SqlCommand("Select * from phoneTable ORDER BY ContactName", phoneConnection);

            // estbalish data adapter/data table 
            phoneAdapter = new SqlDataAdapter();
            phoneAdapter.SelectCommand = phoneCommand;
            phoneTable = new DataTable();
            phoneAdapter.Fill(phoneTable);

            // bind controls to the data table
            txtID.DataBindings.Add("Text", phoneTable, "ContactID");
            txtName.DataBindings.Add("Text", phoneTable, "ContactName");
            txtNumber.DataBindings.Add("Text", phoneTable, "ContactName");

            // establish conncurency manager
            phoneManager = (CurrencyManager)this.BindingContext[phoneTable];

            // 6-4
            foreach(DataRow phoneRow in phoneTable.Rows) 
            {
                phoneRow["ContactNumber"] = "(206)" + phoneRow["ContactNumber"].ToString(); 
            }
        }

        private void frmPhoneDB_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myState.Equals("Edit") || myState.Equals("Add"))
            {
                MessageBox.Show("You must finish the current edit before stopping the application",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else 
            {
                try 
                {
                    // save the updated phone table
                    SqlCommandBuilder phoneAdapterCommands = new SqlCommandBuilder(phoneAdapter);
                    phoneAdapter.Update(phoneTable);

                }
                catch(Exception ex) 
                {
                    MessageBox.Show("Error saving database to file:\r\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            phoneConnection.Close();

            phoneConnection.Dispose();
            phoneCommand.Dispose();
            phoneAdapter.Dispose();
            phoneTable.Dispose(); 
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            phoneManager.Position = 0; 
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            phoneManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            phoneManager.Position++;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            phoneManager.Position = phoneManager.Count - 1; 
        }
        private void SetState(string appState) 
        {
            myState = appState;
            switch (appState) 
            {
                case "View":
                    btnFirst.Enabled = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnLast.Enabled = true;
                    btnEdit.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAdd.Enabled = true; // 6-2
                    txtID.BackColor = Color.White;
                    txtID.ForeColor = Color.Black;
                    txtName.ReadOnly = true;
                    txtNumber.ReadOnly = true;
                    break;
                default:
                    btnFirst.Enabled = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    btnEdit.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAdd.Enabled = false; 
                    txtID.BackColor = Color.Red;
                    txtID.ForeColor = Color.White;
                    txtName.ReadOnly = false;
                    txtNumber.ReadOnly = false;
                    break;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string savedName = txtName.Text;
            int savedRow; 
            phoneManager.EndCurrentEdit();
            phoneTable.DefaultView.Sort = "ContactName";
            savedRow = phoneTable.DefaultView.Find(savedName);
            phoneManager.Position = savedRow;
            SetState("View");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            phoneManager.EndCurrentEdit();
            if (myState.Equals("Add")) // 6-2
            {
                phoneManager.Position = myBookmark;
            }
            SetState("View");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            myBookmark = phoneManager.Position;
            SetState("Add");
            phoneManager.AddNew();
        }
        
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to delete this record?", "Delete Record", MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) 
            {
                phoneManager.RemoveAt(phoneManager.Position);
            }
            SetState("View"); 
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
