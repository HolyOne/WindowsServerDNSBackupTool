using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Xml;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WindowsApplication9
{

   
    
    public partial class Form1 : Form
    {

     

        public string  stat
        {
            get { return toolStripStatusLabel1.Text; }
            set { toolStripStatusLabel1.Text = value; }
        }
	
        HolyOne.DNSAdminLib dns;
        public Form1()
        {
            dns = new HolyOne.DNSAdminLib();
            InitializeComponent();
        }

        private void loaddnstotree()
        {
            treeView1.Nodes.Clear();

            TreeNode root = treeView1.Nodes.Add("All Records");

           string[] dnss= dns.GetZones();

           foreach (string z in dnss)
           {
            //   treeView1.Nodes.Add(z);

               TreeNode zone = new TreeNode(z);
               zone.ImageIndex = 1;
               root.Nodes.Add(zone);
              
           }
           root.ImageIndex = 0;
           root.ExpandAll();
        
        }


        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void loadlistitems(string z)
        { listView1.Items.Clear();
        if (z == "All Records") z = "";


        HolyOne.DNSAdminLib.dnsrecord[] dnsa = dns.GetRecordsByType(z);

        foreach (HolyOne.DNSAdminLib.dnsrecord r in dnsa)
        {
            ListViewItem v = (new ListViewItem(r.OwnerName));
            v.SubItems.Add(r.ContainerName);
            v.SubItems.Add(r.DnsServerName);
            v.SubItems.Add(r.DomainName);
            v.SubItems.Add(r.RecordClass.ToString());
            v.SubItems.Add(r.RecordData.ToString());
            v.SubItems.Add(r.dnstype.ToString());
            v.ImageIndex = 2;
            
     
           listView1.Items.Add(v); ;
        }






        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            loadlistitems(e.Node.Text);
          
        }



        private void RefreshTree()
        {

            listView1.Columns.Clear();
           ColumnHeader hh=    listView1.Columns.Add("OwnerName");

         listView1.Columns.Add("ContainerName");

        
            listView1.Columns.Add("DnsServerName");
            listView1.Columns.Add("DomainName");
            listView1.Columns.Add("RecordClass");
            listView1.Columns.Add("RecordData");
            listView1.Columns.Add("Type");
            foreach (ColumnHeader h in listView1.Columns)
            {
                h.Width = 100;
            }

           hh.Width = 150;
            loaddnstotree();

            if (treeView1.Nodes.Count > 0)
                loadlistitems(treeView1.Nodes[0].Text);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
 RefreshTree();
            }
            catch (Exception x)
            {

                MessageBox.Show("Could not load DNS records\r\n\r\nMake sure you are running a server edition of Windows OS with DNS server service Installed\r\n\r\nDetail:\r\n"+ x.Message, "Could not load DNS records", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {


            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "Scribble Files (*.HolyOneDNS)|*.HolyOneDNS|All Files (*.*)|*.*";
            saveDlg.DefaultExt = ".HolyOneDNS";
            saveDlg.FileName = "Backup.HolyOneDNS";
            DialogResult res = saveDlg.ShowDialog();
            if (res == DialogResult.OK)
            {

                HolyOne.DNSAdminLib.dnsrecord[] dd = dns.GetRecordsByType("");
                DataTable t = new DataTable("DNS");
                DataTable t2 = new DataTable("ZONE");

                t.Columns.Add("OwnerName");

                t.Columns.Add("ContainerName");
                t.Columns.Add("DnsServerName");
                t.Columns.Add("DomainName");
                t.Columns.Add("RecordClass");
                t.Columns.Add("RecordData");
                t.Columns.Add("Type");
                int max = dd.Length;

                t2.Columns.Add("ContainerName");
                t2.Columns.Add("DnsServerName");
                t2.Columns.Add("Name");

                HolyOne.DNSAdminLib.dnszone[] zz = dns.GetZonesAsClass();
                int i = 0;
                foreach (HolyOne.DNSAdminLib.dnszone z in zz)
                {
                    t2.Rows.Add(z.ContainerName, z.DnsServerName, z.Name);
                    i++;
                }


                stat = "Generating DNS Table";
                toolStripProgressBar1.Maximum = max;
                toolStripProgressBar1.Minimum = 0;

                i = 0;


                foreach (HolyOne.DNSAdminLib.dnsrecord d in dd)
                {

                    toolStripProgressBar1.Value = i;
                    Application.DoEvents();
                    if (!d.ContainerName.StartsWith(".."))
                        t.Rows.Add(d.OwnerName, d.ContainerName, d.DnsServerName, d.DomainName, d.RecordClass.ToString(), d.RecordData, d.dnstype.ToString());
                    i++;

                }




                DataSet ds = new DataSet();
                ds.Tables.Add(t);
                ds.Tables.Add(t2);

                // HolyOne.XMLOut xxx = new HolyOne.XMLOut();
                //  xxx.datatable = t;
                ds.WriteXml(saveDlg.FileName);
                // System.IO.File.WriteAllText(saveDlg.FileName,    );

            }


          







        }



        protected DataSet LoadDatasetFromXml(string fileName)
        {
            DataSet ds = new DataSet();
            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(fs);
                ds.ReadXml(reader);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return ds;
        }




        private void loadxmltoDNS(string s) {


            DataSet ds =LoadDatasetFromXml(s);

            DataTable t = ds.Tables["DNS"];
            DataTable t2 = ds.Tables["ZONE"];

          //  System.Collections.Generic.List<string> zones = new List<string>();

            int i = 0;
            stat = "Reading XML Data";
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = t.Rows.Count;

 HolyOne.DNSAdminLib.dnszone[] zz=new HolyOne.DNSAdminLib.dnszone[t2.Rows.Count];
          
            foreach (DataRow  r in t2.Rows)
            {

                zz[i].ContainerName = r["ContainerName"].ToString();
                zz[i].DnsServerName = r["DnsServerName"].ToString();
                zz[i].Name = r["Name"].ToString();
                toolStripProgressBar1.Value = i;
                Application.DoEvents();
                i++;
            }

           
 
            toolStripProgressBar1.Maximum = zz.Length;
           
            i = 0;
            stat = "Creating DNS Zones";
            foreach (HolyOne.DNSAdminLib.dnszone z in zz)
            {

                dns.CreateZone(z.Name,"");
                toolStripProgressBar1.Value = i;
               
              
                Application.DoEvents();
                i++;
            }

            i = 0;
            toolStripProgressBar1.Maximum = t.Rows.Count;
            stat = "Creating DNS Records";
            foreach (DataRow dr in t.Rows)
            {

                toolStripProgressBar1.Value = i;
                Application.DoEvents();
                i++;


                HolyOne.DNSAdminLib.dnsrecord r;
                r.ContainerName = dr["ContainerName"].ToString();
                r.DnsServerName = dr["DnsServerName"].ToString();
               r.dnstype =    (HolyOne.DNSAdminLib.DnsType)Enum.Parse(typeof(HolyOne.DNSAdminLib.DnsType), dr["Type"].ToString());
               r.DomainName = dr["DomainName"].ToString();
               r.OwnerName = dr["OwnerName"].ToString();
               r.RecordClass = int.Parse(dr["RecordClass"].ToString());
               r.RecordData = dr["RecordData"].ToString();


               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.A)
                   dns.CreateAType(r.ContainerName, r.OwnerName, r.RecordData);

               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.TXT)
                   dns.CreateTXTType(r.ContainerName, r.OwnerName, getStrWithoutQuotes(r.RecordData.Trim()));

               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.MX)
               {
                   string data = r.RecordData.Trim() ;
                   string pref = "";
                   string name = "";
                   int intpref = 10;

                   pref = data.Substring(0, data.IndexOf(" "));
                   name = data.Substring( data.IndexOf(" ")+1);

                   if (!int.TryParse(pref, out intpref)) intpref = 10;

                   dns.CreateMXType(r.ContainerName, r.OwnerName, intpref, name);

               }



               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.CNAME)
                   dns.CreateCNAMEType(r.ContainerName, r.OwnerName, r.RecordData,r.RecordClass);

               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.NS)
               {
                  
                   dns.CreateNSType(r.ContainerName, r.OwnerName, r.RecordData);
               }
               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.PTR)
                   dns.CreatePTRType(r.ContainerName, r.OwnerName, r.RecordData);

               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.RP)
                   dns.CreateRPType(r.ContainerName, r.OwnerName, r.RecordData,r.RecordClass);

               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.MB)
                   dns.CreateMBType(r.ContainerName, r.OwnerName, r.RecordData, r.RecordClass);


                /*

 
               
            

               if (r.dnstype == HolyOne.DNSAdminLib.DnsType.RP)
                   dns.CreateNSType(r.ContainerName, r.OwnerName, r.RecordData);
                 * */
            }

            stat = "Finished Restoring DNS Records";
            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

         //   dataGridView1.DataSource = ds.Tables[0];
     
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Scribble Files (*.HolyOneDNS)|*.HolyOneDNS|All Files (*.*)|*.*";
            openDlg.FileName = "";
            openDlg.DefaultExt = ".HolyOneDNS";
            openDlg.CheckFileExists = true;
            openDlg.CheckPathExists = true;
            DialogResult res = openDlg.ShowDialog();
            if (res == DialogResult.OK)
            {
                if (!(openDlg.FileName).EndsWith(".HolyOneDNS") && !(openDlg.FileName).EndsWith(".HolyOneDNS"))
                    MessageBox.Show("Unexpected file format", "HolyOneDNS", MessageBoxButtons.OK);
                else
                {

                    loadxmltoDNS(openDlg.FileName);
                    RefreshTree();


                }
            }

        }


        private void button3_Click(object sender, EventArgs e)
        {
            FlushDNS(true);
        }

        private void FlushDNS(bool mbox)
        {
            DialogResult r=DialogResult.None;
            if(mbox)r = MessageBox.Show("Flush ALL DNS DATA?", "ARE YOU SURE?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (r == DialogResult.OK || !mbox)
            {
                int i = 0;
                stat = "Clearing DNS Records...";
                string[] dd = dns.GetZones();
                toolStripProgressBar1.Maximum = dd.Length;
                foreach (string s in dd)
                {
                    toolStripProgressBar1.Value = i;
                    dns.DelZone(s);
                    i++;

                    Application.DoEvents();
                }
                stat = "Dns Records Cleared";
                RefreshTree();

            }
        }

      /*  private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox2.Enabled = ((sender as CheckBox).Checked);
            if (!checkBox2.Enabled) checkBox2.Checked = false;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = ((sender as CheckBox).Checked);
            button2.Enabled = ((sender as CheckBox).Checked);
            button3.Enabled = ((sender as CheckBox).Checked);  
        }*/

        private void button4_Click(object sender, EventArgs e)
        {
            dns.GetZonesAsClass();
        }

        private static string getStrWithoutQuotes(string a)
        {
            if (a.StartsWith(@"""")) a = a.Substring(1);
            if (a.EndsWith(@"""")) a = a.Substring(0, a.Length - 1);
            return a;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.tahribat.com");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



    }




    public class filerec {

        public string[] zones;
        public HolyOne.DNSAdminLib.dnsrecord[] dnss;

    }
}