/*
DNS management lib of some chinese guy
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Management;

namespace HolyOne
{
    public class DNSAdminLib
    {

        public enum DnsType
        {
            AAAA, AFSDB, ATMA, A, CNAME, HINFO, ISDN, KEY, MB, MD, MF, MG, MINFO, MR, MX, NS, NXT, PTR, RP, RT, SIG, SOA, SRV, TXT, WINSR, WINS, WKS, X25
        }


        public struct dnsrecord
        {
            public string DnsServerName;
            public string ContainerName;

            public string DomainName;
            public string OwnerName;
            public int RecordClass;
            public string RecordData;

            public DnsType dnstype;
        }

        public struct dnszone
        {
            public string ContainerName;
            public string DnsServerName;
            public string Name;
        }





        private string sServerPath;
        private string username = null;
        private string password = null;
        private string DNSName = null;
        private ManagementScope DNS;
        private System.Management.ManagementObjectCollection Q;
        private ManagementClass DnsClass;
        private ManagementBaseObject MI;
        public string ServerName
        {
            set
            {
                this.sServerPath = string.Format(@"\\{0}\root\MicrosoftDNS", value);
                this.DNSName = value;
            }
        }

        public string userName
        {
            set
            {
                this.username = value;
            }
        }

        public string passWord
        {
            set
            {
                this.password = value;
            }
        }

        public DNSAdminLib()
        {
            sServerPath = @"\\localhost\root\MicrosoftDNS";
            DNSName = "localhost";

        }



        private void Create(string DnsType)
        {
            DNS = new ManagementScope(sServerPath);
            if (!DNS.IsConnected)
            {
                DNS.Connect();
            }
            ManagementPath Path = new ManagementPath(DnsType);
            this.DnsClass = new ManagementClass(DNS, Path, null);
        }

        public ManagementObjectCollection QueryDNS(string query, string DnsType)
        {
            this.Create(DnsType);
            System.Management.ManagementObjectSearcher QS = new ManagementObjectSearcher(DNS, new ObjectQuery(query));
            QS.Scope = DNS;
            return QS.Get();
        }

        public ManagementObjectCollection QueryDNS(string query)
        {
            DNS = new ManagementScope(sServerPath);
            if (!DNS.IsConnected)
            {
                DNS.Connect();
            }
            System.Management.ManagementObjectSearcher QS = new ManagementObjectSearcher(DNS, new ObjectQuery(query));
            QS.Scope = DNS;
            return QS.Get();


        }


        public bool IsExistsZone(string domain)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_ZONE where ContainerName='" + domain + "'");
                foreach (ManagementObject oManObject in Q)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        public dnszone[] GetZonesAsClass()
        {
            dnszone[] zz = null;
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_ZONE");
                zz = new dnszone[Q.Count];
                int i = 0;
                foreach (ManagementObject oManObject in Q)
                {
                    zz[i].ContainerName = oManObject["ContainerName"].ToString();
                    zz[i].DnsServerName = oManObject["DnsServerName"].ToString();
                    zz[i].Name = oManObject["Name"].ToString();
                    i++;

                    Console.WriteLine(oManObject.ToString());
                }
            }
            catch
            {
                return null;
            }
            return zz;
        }

        public string[] GetZones()
        {
            string[] ss = null;
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_ZONE");

                ss = new string[Q.Count];
                int i = 0;
                foreach (ManagementObject oManObject in Q)
                {
                    ss[i] = oManObject["name"].ToString();
                    i++;
                }

            }
            catch
            {

            }

            return ss;
        }




        public dnsrecord[] GetARecords(string domain)
        {
            dnsrecord[] dd = null;

            string querystr = "Select * From MicrosoftDNS_AType";
            if (!String.IsNullOrEmpty(domain))
            {

                querystr += " where ContainerName='" + domain + "'";
                if (!IsExistsZone(domain)) return new dnsrecord[0];
            }

            Q = QueryDNS(querystr); querystr = null;
            dd = new dnsrecord[Q.Count];

            int i = 0;
            foreach (ManagementObject oManObject in Q)
            {
                dd[i].DnsServerName = oManObject["DnsServerName"].ToString();
                dd[i].ContainerName = oManObject["ContainerName"].ToString();
                dd[i].DomainName = oManObject["DomainName"].ToString();
                dd[i].OwnerName = oManObject["OwnerName"].ToString();

                dd[i].RecordClass = int.Parse(oManObject["RecordClass"].ToString());
                dd[i].dnstype = DnsType.A;
                dd[i].RecordData = (oManObject["RecordData"].ToString());
                i++;
            }


            return dd;
        }







        public dnsrecord[] GetRecordsByType(string domain, DnsType t)
        {
            dnsrecord[] dd = null;

            string querystr = "Select * From MicrosoftDNS_" + t.ToString() + "Type";
            if (!String.IsNullOrEmpty(domain))
            {

                querystr += " where ContainerName='" + domain + "'";
                if (!IsExistsZone(domain)) return new dnsrecord[0];
            }

            Q = QueryDNS(querystr); querystr = null;
            dd = new dnsrecord[Q.Count];

            int i = 0;
            foreach (ManagementObject oManObject in Q)
            {
                dd[i].DnsServerName = oManObject["DnsServerName"].ToString();
                dd[i].ContainerName = oManObject["ContainerName"].ToString();
                dd[i].DomainName = oManObject["DomainName"].ToString();
                dd[i].OwnerName = oManObject["OwnerName"].ToString();

                dd[i].RecordClass = int.Parse(oManObject["RecordClass"].ToString());
                dd[i].dnstype = t;
                dd[i].RecordData = (oManObject["RecordData"].ToString());
                i++;
            }


            return dd;
        }








        public dnsrecord[] GetRecordsByType(string dnsType)
        {

            System.Collections.Generic.List<dnsrecord> dd = new List<dnsrecord>();


            foreach (string t in Enum.GetNames(typeof(DnsType)))
            {
                string querystr = "Select * From MicrosoftDNS_" + t.ToString() + "Type";
                if (!String.IsNullOrEmpty(dnsType))
                {

                    querystr += " where ContainerName='" + dnsType + "'";
                    if (!IsExistsZone(dnsType)) return new dnsrecord[0];
                }

                Q = QueryDNS(querystr); querystr = null;





                int i = 0;
                foreach (ManagementObject oManObject in Q)
                {
                    dnsrecord d = new dnsrecord();
                    d.DnsServerName = oManObject["DnsServerName"].ToString();
                    d.ContainerName = oManObject["ContainerName"].ToString();
                    d.DomainName = oManObject["DomainName"].ToString();
                    d.OwnerName = oManObject["OwnerName"].ToString();

                    d.RecordClass = int.Parse(oManObject["RecordClass"].ToString());

                    DnsType c = (DnsType)Enum.Parse(typeof(DnsType), t);

                    d.dnstype = c;
                    d.RecordData = (oManObject["RecordData"].ToString());
                    i++;
                    dd.Add(d);

                }


            }


            return dd.ToArray();
        }





        public dnsrecord[] GetCNAMERecords(string domain)
        {
            dnsrecord[] dd = null;
            string querystr = "Select * From MicrosoftDNS_CNAMEType";
            if (!String.IsNullOrEmpty(domain))
            {
                querystr += " where ContainerName='" + domain + "'";
                if (!IsExistsZone(domain)) return new dnsrecord[0];
            }
            Q = QueryDNS(querystr);
            querystr = null;
            dd = new dnsrecord[Q.Count];

            int i = 0;
            foreach (ManagementObject oManObject in Q)
            {
                dd[i].ContainerName = oManObject["ContainerName"].ToString();
                dd[i].DnsServerName = oManObject["DnsServerName"].ToString();
                dd[i].DomainName = oManObject["DomainName"].ToString();
                dd[i].OwnerName = oManObject["OwnerName"].ToString();
                dd[i].RecordClass = int.Parse(oManObject["RecordClass"].ToString());
                dd[i].RecordData = (oManObject["RecordData"].ToString());
                dd[i].dnstype = DnsType.CNAME;
                i++;
            }
            return dd;
        }






        public dnsrecord[] GetMXRecords(string domain)
        {
            dnsrecord[] dd = null;
            string querystr = "Select * From MicrosoftDNS_MXType";
            if (!String.IsNullOrEmpty(domain))
            {
                querystr += " where ContainerName='" + domain + "'";
                if (!IsExistsZone(domain)) return new dnsrecord[0];
            }
            Q = QueryDNS(querystr);
            querystr = null;
            dd = new dnsrecord[Q.Count];

            int i = 0;
            foreach (ManagementObject oManObject in Q)
            {
                dd[i].ContainerName = oManObject["ContainerName"].ToString();
                dd[i].DnsServerName = oManObject["DnsServerName"].ToString();
                dd[i].DomainName = oManObject["DomainName"].ToString();
                dd[i].OwnerName = oManObject["OwnerName"].ToString();
                dd[i].RecordClass = int.Parse(oManObject["RecordClass"].ToString());
                dd[i].RecordData = (oManObject["RecordData"].ToString());
                dd[i].dnstype = DnsType.MX;
                i++;
            }

            return dd;
        }

        public bool CreateZone(string domain, string AdminEmailName)
        {
            try
            {
                this.Create("MicrosoftDNS_Zone");
                if (IsExistsZone(domain))
                {
                    return false;
                }
                this.MI = DnsClass.GetMethodParameters("CreateZone");
                this.MI["ZoneName"] = domain;
                this.MI["ZoneType"] = 0;
                this.MI["AdminEmailName"] = AdminEmailName;


                ManagementBaseObject OutParams = this.DnsClass.InvokeMethod("CreateZone", MI, null);
                return true;
            }
            catch
            {
                return false;
            }

        }


        public bool CreateZone(string domain, uint ZoneType, string DataFileName, string[] IpAddr, string AdminEmailName)
        {
            try
            {
                this.Create("MicrosoftDNS_Zone");
                if (IsExistsZone(domain))
                {
                    return false;
                }
                MI = DnsClass.GetMethodParameters("CreateZone");
                MI["ZoneName"] = domain;
                MI["ZoneType"] = ZoneType;
                MI["DataFileName"] = DataFileName;
                MI["IpAddr"] = IpAddr;
                MI["AdminEmailName"] = AdminEmailName;
                ManagementBaseObject OutParams = this.DnsClass.InvokeMethod("CreateZone", MI, null);
                return true;
            }
            catch
            {
                return false;
            }

        }


        public bool ChangeZoneType(string domain, uint ZoneType, string DataFileName, string[] IpAddr, string AdminEmailName)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_ZONE where ContainerName='" + domain + "'", "MicrosoftDNS_Zone");

                foreach (ManagementObject oManObject in Q)
                {
                    MI = oManObject.GetMethodParameters("ChangeZoneType");
                    MI["ZoneType"] = ZoneType;
                    MI["DataFileName"] = DataFileName;
                    MI["IpAddr"] = IpAddr;
                    MI["AdminEmailName"] = AdminEmailName;
                    oManObject.InvokeMethod("ChangeZoneType", MI, null);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool DelZone(string domain)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_ZONE where ContainerName='" + domain + "'", "MicrosoftDNS_Zone");
                foreach (ManagementObject oManObject in Q)
                {
                    oManObject.Delete();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        public bool IsExistsAType(string domain, string OwnerName)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_AType where OwnerName='" + OwnerName + "' and ContainerName='" + domain + "'");
                foreach (ManagementObject oManObject in Q)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }




        public bool IsExistsAnyType(string domain, string OwnerName, DnsType dnstype)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_" + dnstype.ToString() + "Type where OwnerName='" + OwnerName + "' and ContainerName='" + domain + "'");
                foreach (ManagementObject oManObject in Q)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }



        public bool IsExistsMXType(string domain, string OwnerName)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_MXType where OwnerName='" + OwnerName + "' and ContainerName='" + domain + "'");
                foreach (ManagementObject oManObject in Q)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsExistsCNAMEType(string domain, string OwnerName)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_CNAMEType where OwnerName='" + OwnerName + "' and ContainerName='" + domain + "'");
                foreach (ManagementObject oManObject in Q)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsExistsTXTType(string domain, string OwnerName)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_TXTType where OwnerName='" + OwnerName + "' and ContainerName='" + domain + "'");
                foreach (ManagementObject oManObject in Q)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        public bool CreateAType(string ContainerName, string OwnerName, string IPAddress)
        {
            try
            {

                this.Create("MicrosoftDNS_AType");
                if (!IsExistsZone(ContainerName))
                {
                    Console.WriteLine("Container does not exist:{0}", ContainerName);
                    return false;
                }
                if (IsExistsAType(ContainerName, OwnerName))
                {
                    Console.WriteLine("ContainerName {0} don't exist in Owner {1}", ContainerName, OwnerName);
                    return false;
                }
                MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
                MI["DnsServerName"] = "localhost";
                MI["ContainerName"] = ContainerName;
                MI["OwnerName"] = OwnerName;
                MI["IPAddress"] = IPAddress;
                MI["TTL"] = 3600;
                DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);


            }
            catch (Exception)
            {



            }
            return true;
        }






        public bool CreateMXType(string ContainerName, string OwnerName, int preference, string data)
        {
            try
            {

                this.Create("MicrosoftDNS_MXType");
                if (!IsExistsZone(ContainerName))
                {
                    Console.WriteLine("区域:{0}不存在,创建失败", ContainerName);
                    return false;
                }
                if (IsExistsMXType(ContainerName, OwnerName))
                {
                    Console.WriteLine("{0}中已存在{1},创建失败", ContainerName, OwnerName);
                    return false;
                }
                MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
                MI["DnsServerName"] = "localhost";
                MI["ContainerName"] = ContainerName;
                MI["OwnerName"] = OwnerName;

                MI["Preference"] = preference;
                MI["MailExchange"] = data;
                MI["TTL"] = 3600;
                MI["RecordClass"] = 1;
                DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);


            }
            catch (Exception)
            {



            }
            return true;
        }



        public bool CreateNSType(string ContainerName, string OwnerName, string NSHost)
        {
            try
            {

                this.Create("MicrosoftDNS_NSType");
                if (!IsExistsZone(ContainerName))
                {
                    Console.WriteLine("区域:{0}不存在,创建失败", ContainerName);
                    return false;
                }
                if (IsExistsAnyType(ContainerName, OwnerName, DnsType.NS))
                {
                    Console.WriteLine("{0}中已存在{1},创建失败", ContainerName, OwnerName);
                    return false;
                }
                MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
                MI["DnsServerName"] = "localhost";
                MI["ContainerName"] = ContainerName;
                MI["OwnerName"] = OwnerName;

                MI["NSHost"] = NSHost;
                MI["TTL"] = 3600;
                MI["RecordClass"] = 1;
                DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);


            }
            catch (Exception)
            {



            }
            return true;
        }




        public bool CreateRPType(string ContainerName, string OwnerName, string RPMailbox)
        {
            return CreateRPType(ContainerName, OwnerName, RPMailbox, 1);


        }


        public bool CreateRPType(string ContainerName, string OwnerName, string RPMailbox, int RecordClass)
        {
            try
            {

                this.Create("MicrosoftDNS_RPType");
                if (!IsExistsZone(ContainerName))
                {
                    Console.WriteLine("区域:{0}不存在,创建失败", ContainerName);
                    return false;
                }
                if (IsExistsAnyType(ContainerName, OwnerName, DnsType.RP))
                {
                    Console.WriteLine("{0}中已存在{1},创建失败", ContainerName, OwnerName);
                    return false;
                }
                MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
                MI["DnsServerName"] = "localhost";
                MI["ContainerName"] = ContainerName;
                MI["OwnerName"] = OwnerName;

                MI["RPMailbox"] = RPMailbox;
                MI["TTL"] = 3600;
                MI["RecordClass"] = RecordClass;
                DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);


            }
            catch (Exception)
            {



            }
            return true;
        }





        public bool CreateMBType(string ContainerName, string OwnerName, string MBHost)
        {
            return CreateMBType(ContainerName, OwnerName, MBHost, 1);


        }
        public bool CreateMBType(string ContainerName, string OwnerName, string MBHost, int RecordClass)
        {
            try
            {

                this.Create("MicrosoftDNS_MBType");
                if (!IsExistsZone(ContainerName))
                {
                    Console.WriteLine("区域:{0}不存在,创建失败", ContainerName);
                    return false;
                }
                if (IsExistsAnyType(ContainerName, OwnerName, DnsType.MB))
                {
                    Console.WriteLine("{0}中已存在{1},创建失败", ContainerName, OwnerName);
                    return false;
                }
                MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
                MI["DnsServerName"] = "localhost";
                MI["ContainerName"] = ContainerName;
                MI["OwnerName"] = OwnerName;

                MI["MBHost"] = MBHost;
                MI["TTL"] = 3600;
                MI["RecordClass"] = RecordClass;
                DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);


            }
            catch (Exception)
            {



            }
            return true;
        }





        public bool CreatePTRType(string ContainerName, string OwnerName, string PTRDomainName)
        {
            try
            {
                this.Create("MicrosoftDNS_PTRType");
                if (!IsExistsZone(ContainerName))
                {
                    Console.WriteLine("区域:{0}不存在,创建失败", ContainerName);
                    return false;
                }
                /*
                if (!IsExistsAnyType(ContainerName, OwnerName, DnsType.PTR))
                {
                Console.WriteLine("{0}中已存在{1},创建失败", ContainerName, OwnerName);
                return false;
                }
                */
                MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
                MI["DnsServerName"] = "localhost";
                MI["ContainerName"] = ContainerName;
                MI["OwnerName"] = OwnerName;

                MI["PTRDomainName"] = PTRDomainName;
                MI["TTL"] = 3600;
                MI["RecordClass"] = 1;
                DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);
            }
            catch (Exception)
            {
            }
            return true;
        }




        public bool CreateCNAMEType(string ContainerName, string OwnerName, string PrimaryName)
        {
            return CreateCNAMEType(ContainerName, OwnerName, PrimaryName, 1);
        }

        public bool CreateCNAMEType(string ContainerName, string OwnerName, string PrimaryName, int recordclass)
        {
            try
            {

                this.Create("MicrosoftDNS_CNAMEType");
                if (!IsExistsZone(ContainerName))
                {
                    Console.WriteLine("区域:{0}不存在,创建失败", ContainerName);
                    return false;
                }
                if (IsExistsCNAMEType(ContainerName, OwnerName))
                {
                    Console.WriteLine("{0}中已存在{1},创建失败", ContainerName, OwnerName);
                    return false;
                }
                MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
                MI["DnsServerName"] = "localhost";
                MI["ContainerName"] = ContainerName;
                MI["OwnerName"] = OwnerName;


                MI["PrimaryName"] = PrimaryName;
                MI["TTL"] = 3600;
                MI["RecordClass"] = recordclass;
                DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);


            }
            catch (Exception)
            {



            }
            return true;
        }







        public bool CreateTXTType(string ContainerName, string OwnerName, string descriptivetext)
        {

            this.Create("MicrosoftDNS_TXTType");
            if (!IsExistsZone(ContainerName))
            {
                return false;
            }


            if (IsExistsTXTType(ContainerName, OwnerName))
            {
                return false;
            }
            /*
            string DnsServerName,
            string ContainerName,
            string OwnerName,
            uint32 RecordClass = 1,
            uint32 TTL,
            string DescriptiveText,
            MicrosoftDNS_TXTType& RR

            */
            object o = new object();
            MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
            MI["DnsServerName"] = "localhost";
            MI["ContainerName"] = ContainerName;
            MI["OwnerName"] = OwnerName;
            MI["RecordClass"] = 1;

            MI["TTL"] = 3600;
            MI["DescriptiveText"] = descriptivetext;
            DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);


            return true;
        }


        public bool CreateAType(string DnsServerName, string ContainerName, string OwnerName, uint RecordClass, uint TTL, string IPAddress)
        {
            try
            {
                this.Create("MicrosoftDNS_AType");
                if (!IsExistsZone(ContainerName))
                {
                    Console.WriteLine("区域:{0}不存在,创建失败", ContainerName);
                    return false;
                }
                if (IsExistsAType(ContainerName, OwnerName))
                {
                    Console.WriteLine("{0}中已存在{1},创建失败", ContainerName, OwnerName);
                    return false;
                }
                MI = DnsClass.GetMethodParameters("CreateInstanceFromPropertyData");
                MI["DnsServerName"] = DnsServerName;
                MI["ContainerName"] = ContainerName;
                MI["OwnerName"] = OwnerName;
                MI["RecordClass"] = RecordClass;
                MI["TTL"] = TTL;
                MI["IPAddress"] = IPAddress;
                DnsClass.InvokeMethod("CreateInstanceFromPropertyData", MI, null);
                return true;
            }
            catch
            {
                return false;
            }

        }


        public bool ModifyAType(string ContainerName, string OwnerName, string IPAddress)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_AType where ContainerName='" + ContainerName + "' and OwnerName='" + OwnerName + "'", "MicrosoftDNS_AType");

                foreach (ManagementObject oManObject in Q)
                {
                    MI = oManObject.GetMethodParameters("Modify");
                    MI["IPAddress"] = IPAddress;
                    oManObject.InvokeMethod("Modify", MI, null);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool ModifyAType(string ContainerName, string OwnerName, uint TTL, string IPAddress)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_AType where ContainerName='" + ContainerName + "' and OwnerName='" + OwnerName + "'", "MicrosoftDNS_AType");

                foreach (ManagementObject oManObject in Q)
                {
                    MI = oManObject.GetMethodParameters("Modify");
                    MI["TTL"] = TTL;
                    MI["IPAddress"] = IPAddress;
                    oManObject.InvokeMethod("Modify", MI, null);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }



        public bool DelAType(string ContainerName, string OwnerName)
        {
            try
            {
                Q = QueryDNS("Select * From MicrosoftDNS_AType where ContainerName='" + ContainerName + "' and OwnerName='" + OwnerName + "'", "MicrosoftDNS_AType");

                foreach (ManagementObject oManObject in Q)
                {
                    oManObject.Delete();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }




    }

}
