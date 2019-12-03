﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Configuration;
using System.Net.Mail;
using Castest.Models;

namespace Castest.Controllers
{

    [Authorize]

    public class WebController : Controller
    {

        SqlConnection con = new SqlConnection();
        SqlCommand faculty = new SqlCommand();
        SqlCommand orders = new SqlCommand();
        SqlCommand generateordernum = new SqlCommand();
        SqlCommand findemail = new SqlCommand();

        //  String connectionstring = "Data Source = 10.10.9.100; User ID = sa; Password = Passw0rd12; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False; Initial Catalog=Capping";

        string connectionstring = System.IO.File.ReadAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory.ToString(),@"ConnectionString.txt"));

        public void Test() {

            Debug.WriteLine(connectionstring);


        }


        public ActionResult GetData(bool fridayCheckBox, bool saturdayCheckBox, String Name, String PhoneNumber, String Department, int Feet, int Inches, int Weight, String CapSize, String Degree, String GrantingInstitution, String InstitutionState, String InstitutionCity)
        {

            Debug.WriteLine(User.Identity.Name);
            Debug.WriteLine(Ceremony(fridayCheckBox, saturdayCheckBox));
            Debug.WriteLine(Name);
            Debug.WriteLine(PhoneNumber);
            Debug.WriteLine(Department);
            Debug.WriteLine(Feet);
            Debug.WriteLine(Inches);
            Debug.WriteLine(Weight);
            Debug.WriteLine(CapSize);
            Debug.WriteLine(Degree);
            Debug.WriteLine(GrantingInstitution);
            Debug.WriteLine(InstitutionState);
            Debug.WriteLine(InstitutionCity);


            String facultystring;
            String ordersstring;

            if (EmailAlreadyExists(User.Identity.Name) == true)
            {

                con.ConnectionString = connectionstring;
                facultystring = "UPDATE Faculty SET Name = @name, PhoneNumber = @phonenumber, Department = @department, CapSize = @capsize, University = @school, Degree = @degree, Inches = @inches, Feet = @feet, Weight = @weight WHERE Email = @email";

                if (OrderAlreadyExists(User.Identity.Name) == true)
                {

                    ordersstring = "UPDATE Orders SET DateandTime = @dateandtime, CeremonyType = @ceremonytype, CapSize = @capsize, Weight = @weight, HeightFeet = @heightfeet, HeightInches = @heightinches, Degree = @degree, College = @school, CollegeCity = @city, CollegeState = @state WHERE Email = @email";

                    WriteDB(facultystring, ordersstring, User.Identity.Name, Ceremony(fridayCheckBox, saturdayCheckBox), Name, PhoneNumber, Department, Feet, Inches, Weight, CapSize, Degree, GrantingInstitution, InstitutionState, InstitutionCity);

                }

                else if (OrderAlreadyExists(User.Identity.Name) == false)
                {

                    ordersstring = "INSERT INTO Orders" + "(OrderID, Email, DateandTime, CeremonyType, CapSize, Weight, HeightFeet, HeightInches, Degree, College, CollegeCity, CollegeState) VALUES (@ordernumber, @email, @dateandtime, @ceremonytype, @capsize, @weight, @heightfeet, @heightinches, @degree, @school, @city, @state )";

                    WriteDB(facultystring, ordersstring, User.Identity.Name, Ceremony(fridayCheckBox, saturdayCheckBox), Name, PhoneNumber, Department, Feet, Inches, Weight, CapSize, Degree, GrantingInstitution, InstitutionState, InstitutionCity);

                }

            }

            //If the email does not exist use the INSERT command to add the new data to the DB
            else if (EmailAlreadyExists(User.Identity.Name) == false)
            {

                OrderAlreadyExists(User.Identity.Name);

                facultystring = "INSERT INTO Faculty" + "(Email, Name, PhoneNumber, Department, CapSize, University, Degree, Inches, Feet, Weight, isAdmin) VALUES (@email, @name, @phonenumber, @department, @capsize, @school, @degree, @inches, @feet, @weight, @isadmin)";
                ordersstring = "INSERT INTO Orders" + "(OrderID, Email, DateandTime, CeremonyType, CapSize, Weight, HeightFeet, HeightInches, Degree, College, CollegeCity, CollegeState) VALUES (@ordernumber, @email, @dateandtime, @ceremonytype, @capsize, @weight, @heightfeet, @heightinches, @degree, @school, @city, @state )";

                WriteDB(facultystring, ordersstring, User.Identity.Name, Ceremony(fridayCheckBox, saturdayCheckBox), Name, PhoneNumber, Department, Feet, Inches, Weight, CapSize, Degree, GrantingInstitution, InstitutionState, InstitutionCity);

            }


            SendEmail(User.Identity.Name, Name, PhoneNumber, Department, Feet, Inches, Weight, CapSize, Degree, GrantingInstitution, InstitutionState, InstitutionCity);


            return View("CompleteRedirect");

        }


        public ActionResult WriteHistoric()
        {

            if (isAdmin())
            {

                con.ConnectionString = connectionstring;

                con.Open();


                SqlCommand writehistoric = new SqlCommand("INSERT INTO HistoricOrders SELECT * FROM Orders", con);
                SqlCommand clearorders = new SqlCommand("TRUNCATE TABLE Orders", con);



                writehistoric.ExecuteScalar();
                clearorders.ExecuteScalar();

                con.Close();

                return View("AdminRedirect");
            }

            else {

                return View("Index");

            }

        }


        public void WriteDB(String facultystring, String ordersstring, String Email, String Ceremony, String Name, String PhoneNumber, String Department, int Feet, int Inches, int Weight, String CapSize, String Degree, String GrantingInstitution, String InstitiutionState, String InstitutionCity)
        {

            //Connection strings for various databases, the commended ones are old and outdated

            //con.ConnectionString = "Data Source=DESKTOP-AUSRECR;User ID=sa;Password=alpaca;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False; Initial Catalog=Capping";
            con.ConnectionString = connectionstring;
            //con.ConnectionString = "Data Source = 72.76.93.24; User ID = sa; Password = Passw0rd12; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False; Initial Catalog=Capping";

            con.Open();

            faculty.CommandText = facultystring;
            faculty.Connection = con;

            faculty.Parameters.AddWithValue("@email", Email);
            faculty.Parameters.AddWithValue("@name", Name);
            faculty.Parameters.AddWithValue("@phonenumber", PhoneNumber);
            faculty.Parameters.AddWithValue("@department", Department);
            faculty.Parameters.AddWithValue("@capsize", CapSize);
            faculty.Parameters.AddWithValue("@school", GrantingInstitution);
            faculty.Parameters.AddWithValue("@degree", Degree);
            faculty.Parameters.AddWithValue("@inches", Inches);
            faculty.Parameters.AddWithValue("@feet", Feet);
            faculty.Parameters.AddWithValue("@weight", Weight);
            faculty.Parameters.AddWithValue("@state", InstitiutionState);
            faculty.Parameters.AddWithValue("@isadmin", false);

            faculty.ExecuteNonQuery();


            orders.CommandText = ordersstring;
            orders.Connection = con;

            orders.Parameters.AddWithValue("@ordernumber", Ordernumber());
            orders.Parameters.AddWithValue("@email", Email);
            orders.Parameters.AddWithValue("@capsize", CapSize);
            orders.Parameters.AddWithValue("@weight", Weight);
            orders.Parameters.AddWithValue("@heightfeet", Feet);
            orders.Parameters.AddWithValue("@heightinches", Inches);
            orders.Parameters.AddWithValue("@city", InstitutionCity);
            orders.Parameters.AddWithValue("@school", GrantingInstitution);
            orders.Parameters.AddWithValue("@degree", Degree);
            orders.Parameters.AddWithValue("@state", InstitiutionState);
            orders.Parameters.AddWithValue("@ceremonytype", Ceremony);
            orders.Parameters.AddWithValue("@dateandtime", DateTime.Now);

            orders.ExecuteNonQuery();


            con.Close();

        }


        public ActionResult DeleteOrder(String id)
        {

            if (isAdmin())
            {

                con.ConnectionString = connectionstring;

                con.Open();

                SqlCommand deleteorder = new SqlCommand("DELETE FROM Orders WHERE OrderID = @id", con);

                deleteorder.Parameters.AddWithValue("@id", id);


                deleteorder.ExecuteScalar();

                con.Close();

                return (View("AdminRedirect"));
            }

            else {

                return View("Index");

            }

        }

        public ActionResult DeleteFaculty(String email)
        {

            if (isAdmin())
            {

                if (email != User.Identity.Name)
                {

                    con.ConnectionString = connectionstring;

                    con.Open();

                    SqlCommand deletefaculty = new SqlCommand("DELETE FROM Faculty WHERE Email = @email", con);


                    deletefaculty.Parameters.AddWithValue("@email", email);

                    deletefaculty.ExecuteScalar();

                    con.Close();

                    return (View("AdminRedirect"));

                }

                else
                {
                    return (View("AdminRedirect"));


                }

            }

            else {

                return View("Index");

            }

        }

        public ActionResult NewAdmin(String id)
        {

            if (isAdmin())
            {

                con.ConnectionString = connectionstring;

                if (EmailAlreadyExists(id + "@marist.edu"))
                {

                    con.Open();

                    SqlCommand makeadmin = new SqlCommand("UPDATE Faculty SET isAdmin = 1 WHERE Email = @email", con);

                    makeadmin.Parameters.AddWithValue("@email", id + "@marist.edu");

                    makeadmin.ExecuteScalar();
                }

                else
                {

                    con.Open();

                    SqlCommand makenewadmin = new SqlCommand("INSERT INTO Faculty (Email, isAdmin) VALUES (@email, 1)", con);

                    makenewadmin.Parameters.AddWithValue("@email", id + "@marist.edu");

                    makenewadmin.ExecuteScalar();


                }

                con.Close();

                return View("AdminRedirect");

            }

            else {

                return View("Index");

            }

        }

        public ActionResult Admin()
        {

            if (isAdmin() == true)
            {

                DatabaseTable faculty = new DatabaseTable();

                ViewBag.faculty = faculty.Faculties.ToList();
                ViewBag.orders = faculty.Orders.ToList();

                return View();

            }

            else {

                return View("Index");

            }

        }


        public bool isAdmin()
        {

            con.ConnectionString = connectionstring;

            con.Open();

            SqlCommand getadminstatus = new SqlCommand("SELECT isAdmin FROM Faculty WHERE Email = @email", con);

            getadminstatus.Parameters.AddWithValue("@email", User.Identity.Name);

            var status = Convert.ToInt32(getadminstatus.ExecuteScalar());


            if (status == 1)
            {

                con.Close();
                return (true);
 
            }

            else {

                con.Close();

                return (false);
                
            }

        }


        public ActionResult Index()
        {

            Debug.WriteLine("DOes this shit work");

            // con.ConnectionString = connectionstring;

            //  con.Open();

            ViewBag.ID = User.Identity.Name.Substring(0, User.Identity.Name.IndexOf("@"));


            if (EmailAlreadyExists(User.Identity.Name) == true)
            {

                ViewBag.Name = GetUserData("Name");
                ViewBag.PhoneNumber = GetUserData("PhoneNumber");
                ViewBag.Department = GetUserData("Department");
                ViewBag.Feet = GetUserData("Feet");
                ViewBag.Inches = GetUserData("Inches");
                ViewBag.Weight = GetUserData("Weight");
                ViewBag.CapSize = GetUserData("CapSize");
                ViewBag.Degree = GetUserData("Degree");
                ViewBag.University = GetUserData("University");
                
            }

            else {
                ViewBag.Name = null;
                ViewBag.PhoneNumber = null;
                ViewBag.Department = "Department";
                ViewBag.Feet = "Feet";
                ViewBag.Inches = "Inches";
                ViewBag.Weight = null;
                ViewBag.CapSize = null;
                ViewBag.Degree = "Degree";
                ViewBag.University = null;
                
            }

            if (OrderAlreadyExists(User.Identity.Name) || HistoricOrderAlreadyExists(User.Identity.Name))
            {
                ViewBag.State = GetOrderData("CollegeState");
                ViewBag.City = GetOrderData("CollegeCity");


            }

            else
            {
                ViewBag.State = "State";
                ViewBag.City = null;


            }

            return View();

        }

        public String GetUserData(String Input)
        {

            con.ConnectionString = connectionstring;

            con.Open();

            SqlCommand findinfo = new SqlCommand("SELECT " + Input + " FROM Faculty WHERE Email = @email", con);

            findinfo.Parameters.AddWithValue("@email", User.Identity.Name);

            String output = findinfo.ExecuteScalar().ToString();

            con.Close();

            return (output);

        }

        public String GetOrderData(String Input)
        {

            if (OrderAlreadyExists(User.Identity.Name) == true)
            {
                con.ConnectionString = connectionstring;

                con.Open();

                SqlCommand findinfo = new SqlCommand("SELECT " + Input + " FROM Orders WHERE Email = @email", con);

                findinfo.Parameters.AddWithValue("@email", User.Identity.Name);

                String output = findinfo.ExecuteScalar().ToString();

                con.Close();

                return (output);


            }

            else if (HistoricOrderAlreadyExists(User.Identity.Name) == true)
            {
                con.ConnectionString = connectionstring;

                con.Open();

                SqlCommand findinfo = new SqlCommand("SELECT TOP 1 " + Input + " FROM HistoricOrders WHERE Email = @email ORDER BY DateandTime DESC ", con);

                findinfo.Parameters.AddWithValue("@email", User.Identity.Name);

                String output = findinfo.ExecuteScalar().ToString();

                con.Close();

                return (output);




            }
            else {
                return (null);

            }




        }


        //Construct all the SQL commands we will be using

        
        

        //This funciton takes in SQL commands for each table and assignes variables based on the information present in the website
        

        //This function is used to generate ordernumbers, it reads the DB for the largest number then adds one 
        public int Ordernumber()
        {
            int ordernumber;

        //    con.Open();

            SqlCommand generateordernum = new SqlCommand("SELECT MAX(OrderID) FROM Orders", con);
            SqlCommand generatehistoricordernum = new SqlCommand("SELECT MAX(OrderID) FROM HistoricOrders", con);

            if ((generateordernum.ExecuteScalar() is DBNull) && (generatehistoricordernum.ExecuteScalar() is DBNull))
            {

            ordernumber = 1;

            }

            else
            {

                if (generatehistoricordernum.ExecuteScalar() is DBNull)
                {
                    ordernumber = (Convert.ToInt32(generateordernum.ExecuteScalar())) + 1;

                }

                else if (generateordernum.ExecuteScalar() is DBNull)
                {

                    ordernumber = (Convert.ToInt32(generatehistoricordernum.ExecuteScalar())) + 1;

                }

                else if ((Convert.ToInt32(generateordernum.ExecuteScalar())) < (Convert.ToInt32(generatehistoricordernum.ExecuteScalar())))
                {

                    ordernumber = (Convert.ToInt32(generatehistoricordernum.ExecuteScalar())) + 1;


                }

                else if ((Convert.ToInt32(generatehistoricordernum.ExecuteScalar())) < (Convert.ToInt32(generateordernum.ExecuteScalar())))
                {

                    ordernumber = (Convert.ToInt32(generateordernum.ExecuteScalar())) + 1;


                }




                else {
                    ordernumber = 1000;

                }
                

            }


           

       //     con.Close();

            return ordernumber;

        }

        //This function is used to query the DB to find if an email has already been entered and returns a boolean
        public Boolean EmailAlreadyExists(String Email)
        {

            con.ConnectionString = connectionstring;

            con.Open();


            if (con.State.ToString() == "Open")
            {
                Debug.WriteLine("Open");
            }
            else
            {
                Debug.WriteLine("Closed");
            }



            SqlCommand findemail = new SqlCommand("SELECT 1 FROM Faculty WHERE Email = @email", con);

            findemail.Parameters.AddWithValue("@email", Email);

            if (Convert.ToInt32(findemail.ExecuteScalar()) == 1)
            {

                Debug.WriteLine("True");

                con.Close();
                return true;

            }

            else
            {

                Debug.WriteLine("False");

                con.Close();
                return false;

            }

        }

        public Boolean OrderAlreadyExists(String Email)
        {

            con.ConnectionString = connectionstring;

            con.Open();

            SqlCommand findemail = new SqlCommand("SELECT 1 FROM Orders WHERE Email = @email", con);

            findemail.Parameters.AddWithValue("@email", Email);

            if (Convert.ToInt32(findemail.ExecuteScalar()) == 1)
            {

                Debug.WriteLine("Found order True");

                con.Close();
                return true;

            }

            else
            {

                Debug.WriteLine("Didnt find order False");

                con.Close();
                return false;

            }

        }

        public Boolean HistoricOrderAlreadyExists(String Email)
        {

            con.ConnectionString = connectionstring;

            con.Open();

            SqlCommand findemail = new SqlCommand("SELECT 1 FROM HistoricOrders WHERE Email = @email", con);

            findemail.Parameters.AddWithValue("@email", Email);

            if (Convert.ToInt32(findemail.ExecuteScalar()) == 1)
            {

                Debug.WriteLine("Found order True");

                con.Close();
                return true;

            }

            else
            {

                Debug.WriteLine("Didnt find order False");

                con.Close();
                return false;

            }

        }


        //This function is used to check the values of the checkboxes at the top of the webpage to find the different combinations of ceremony attendance
        public String Ceremony(bool fridayCheckBox, bool saturdayCheckBox)
        {

            String ceremony;

            if (fridayCheckBox == true && saturdayCheckBox != true)
            {

                ceremony = "Friday";

            }

            else if (saturdayCheckBox == true && fridayCheckBox != true)
            {

                ceremony = "Saturday";

            }

            else if (fridayCheckBox == true && saturdayCheckBox == true)
            {

                ceremony = "Friday and Saturday";

            }

            else
            {

                ceremony = "Not Attending";

            }

            return ceremony;

        }



        public static void SendEmail(String email, String name, String PhoneNumber, String Department, int Feet, int Inches, int Weight, String CapSize, String Degree, String GrantingInstitution, String InstitutionState, String InstitutionCity)
        {
            MailMessage mailMessage = new MailMessage("maristregalia@gmail.com", email);
            mailMessage.Subject = "Your order on MaristRegalia.site"; 
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = "<p>Dear " + name + ",</p> <p> Thank you for submitting your regalia order!  We have the following information listed for you:<br/> Phone Number: " + PhoneNumber + " <br/> Department: " + Department + " <br/> Height: " + Feet + "\' " + Inches + "\"<br/>Weight: " + Weight + "<br />Cap Size: " + CapSize + "<br />Degree: " + Degree + "<br />Graduation Institution: " + GrantingInstitution + "<br />Graduation City: " + InstitutionCity + ", " + InstitutionState + "</p><p>If any of the previous information listed is incorrect, please revisit <a href='http://regalia.it.marist.edu/Web/Index'>https://regalia.it.marist.edu</a> to correct any errors.</p><p>Regards,</p><p>Marist Regalia Orders Team</p><p>&nbsp;</p>";

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "maristregalia@gmail.com",
                Password = "vkojywyjcjgwsziy"
            };
            smtpClient.EnableSsl = true;
            smtpClient.Send(mailMessage);

        }


        public void ExportOrders()
        {

            con.ConnectionString = connectionstring;

            con.Open();

            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Orders", con))
            {

                using (SqlDataAdapter sda = new SqlDataAdapter())

                {

                    cmd.Connection = con;
                    sda.SelectCommand = cmd;

                    using (DataTable dt = new DataTable())
                    {

                        sda.Fill(dt);

                        //Build the CSV file data as a Comma separated string.
                        string csv = string.Empty;

                        foreach (DataColumn column in dt.Columns)
                        {
                            
                            //Add the Header row for CSV file.
                            csv += column.ColumnName + ',';
                        }

                        //Add new line.
                        csv += "\r\n";

                        foreach (DataRow row in dt.Rows)
                        {

                            foreach (DataColumn column in dt.Columns)
                            {
            
                            //Add the Data rows.
                            csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';

                            }

                            //Add new line.
                            csv += "\r\n";

                        }

                        String Date = DateTime.Today.ToString("MM/dd/yyyy");

                        //Download the CSV file.
                        Response.Clear();
                        Response.Buffer = true;
                        Response.AddHeader("content-disposition", "attachment;filename=Orders_Table_"+ Date + ".csv");
                        Response.Charset = "";
                        Response.ContentType = "application/text";
                        Response.Output.Write(csv);
                        Response.Flush();
                        Response.End();

                    }

                }
         
            }

        }

        public void ExportFaculty()
        {

            con.ConnectionString = connectionstring;

            con.Open();

            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Faculty", con))
            {

                using (SqlDataAdapter sda = new SqlDataAdapter())

                {

                    cmd.Connection = con;
                    sda.SelectCommand = cmd;

                    using (DataTable dt = new DataTable())
                    {

                        sda.Fill(dt);

                        //Build the CSV file data as a Comma separated string.
                        string csv = string.Empty;

                        foreach (DataColumn column in dt.Columns)
                        {

                            //Add the Header row for CSV file.
                            csv += column.ColumnName + ',';
                        }

                        //Add new line.
                        csv += "\r\n";

                        foreach (DataRow row in dt.Rows)
                        {

                            foreach (DataColumn column in dt.Columns)
                            {

                                //Add the Data rows.
                                csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';

                            }

                            //Add new line.
                            csv += "\r\n";

                        }

                        String Date = DateTime.Today.ToString("MM/dd/yyyy");

                        //Download the CSV file.
                        Response.Clear();
                        Response.Buffer = true;
                        Response.AddHeader("content-disposition", "attachment;filename=Faculty_Table_" + Date + ".csv");
                        Response.Charset = "";
                        Response.ContentType = "application/text";
                        Response.Output.Write(csv);
                        Response.Flush();
                        Response.End();

                    }

                }

            }

        }

        public void ExportHistoricOrders()
        {

            con.ConnectionString = connectionstring;

            con.Open();

            using (SqlCommand cmd = new SqlCommand("SELECT * FROM HistoricOrders", con))
            {

                using (SqlDataAdapter sda = new SqlDataAdapter())

                {

                    cmd.Connection = con;
                    sda.SelectCommand = cmd;

                    using (DataTable dt = new DataTable())
                    {

                        sda.Fill(dt);

                        //Build the CSV file data as a Comma separated string.
                        string csv = string.Empty;

                        foreach (DataColumn column in dt.Columns)
                        {

                            //Add the Header row for CSV file.
                            csv += column.ColumnName + ',';
                        }

                        //Add new line.
                        csv += "\r\n";

                        foreach (DataRow row in dt.Rows)
                        {

                            foreach (DataColumn column in dt.Columns)
                            {

                                //Add the Data rows.
                                csv += row[column.ColumnName].ToString().Replace(",", ";") + ',';

                            }

                            //Add new line.
                            csv += "\r\n";

                        }

                        String Date = DateTime.Today.ToString("MM/dd/yyyy");

                        //Download the CSV file.
                        Response.Clear();
                        Response.Buffer = true;
                        Response.AddHeader("content-disposition", "attachment;filename=Historic_Orders_Table_" + Date + ".csv");
                        Response.Charset = "";
                        Response.ContentType = "application/text";
                        Response.Output.Write(csv);
                        Response.Flush();
                        Response.End();

                    }

                }

            }

        }

        public ActionResult FormComplete() {

            return View();



        }


    }
}