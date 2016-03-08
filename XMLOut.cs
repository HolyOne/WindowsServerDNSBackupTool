using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Xml; 
 

/// <summary>
/// Summary description for Class1
/// </summary>
/// 

namespace HolyOne{
    public class XMLOut
    {



        private System.Data.DataTable _t;

        public System.Data.DataTable datatable
        {
            get { return _t; }
            set { _t = value; }
        }




        public XmlDocument GetXmlFromTable()
        {

            XmlDocument xmldoc;
            XmlNode xmlnode;
            XmlElement xmlelem;
            XmlElement xmlelem2;

            XmlAttribute xmlelem3;



            if (datatable == null) throw new Exception("set datatable first");





            xmldoc = new XmlDocument();
            //let's add the XML declaration section
            xmlnode = xmldoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");

            xmldoc.AppendChild(xmlnode);
            xmlelem = xmldoc.CreateElement("", datatable.TableName, "");


            xmldoc.AppendChild(xmlelem);

            //------------- view source not posible with that
            xmldoc.PreserveWhitespace = true;

            //---------
            for (int i = 0; i < datatable.Rows.Count; i++)
            {


                DataRow r = datatable.Rows[i];
                xmlelem2 = xmldoc.CreateElement(datatable.TableName + "Row");

                xmlelem.AppendChild(xmlelem2);



                for (int j = 0; j < r.ItemArray.Length; j++)
                {

                    object o = r.ItemArray[j];
                    string ostr = "";
                    //custom outputs here

                    if (o.GetType() == typeof(DateTime)) ostr = ((DateTime)o).ToUniversalTime().ToString();

                    else ostr = o.ToString();


                    xmlelem3 = xmldoc.CreateAttribute(r.Table.Columns[j].ColumnName);
                    xmlelem3.Value = ostr;

                    //   xmlelem3 = xmldoc.CreateElement(datatable.TableName + "Row");

                    xmlelem2.Attributes.Append(xmlelem3);
                    //  xmlelem3.AppendChild(xmlelem2);



                }




            }


            return xmldoc;



        }






    }


}


/*
  
     Response.ContentType = "text/xml";
       DataSet1TableAdapters.Table1TableAdapter a = new DataSet1TableAdapters.Table1TableAdapter();
      DataTable t= a.GetData();
       XMLOut o = new XMLOut();
       o.datatable = t;
     Response.Write(  o.GetXmlFromTable());
  
 */