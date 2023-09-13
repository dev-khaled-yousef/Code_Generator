﻿using CodeGenerator_Business;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Code_Generator
{
    public partial class Form1 : Form
    {
        // Table Info
        private string _TableName = string.Empty;
        private string _TableSingleName = string.Empty;
        private bool _IsLogin = false;

        public Form1()
        {
            InitializeComponent();
        }

        private bool _DoesTableHaveUsernameColumn()
        {
            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;

                    if (ColumnName.ToLower() == "username")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool _DoesTableHavePasswordColumn()
        {
            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;

                    if (ColumnName.ToLower() == "password")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool _DoesTableHaveUsernameAndPassword()
        {
            return (_DoesTableHaveUsernameColumn() && _DoesTableHavePasswordColumn());
        }

        private void _FillListViewWithTablesName()
        {
            listviewTablesName.Items.Clear();

            DataTable dt = clsCodeGenerator.GetAllTablesNameInASpecificDatabase(comboDatabaseName.Text.Trim());

            // Loop through the rows in the DataTable and add them to the ListView
            foreach (DataRow row in dt.Rows)
            {
                ListViewItem item = new ListViewItem(row["TableName"].ToString()); // Column1 represents the first column in your DataTable   

                listviewTablesName.Items.Add(item);
            }

            // To show the number of records
            lblNumberOfTablesRecords.Text = listviewTablesName.Items.Count.ToString();
        }

        private void _FillComboBoxWithDatabaseName()
        {
            DataTable dtDatabase = clsCodeGenerator.GetAllDatabaseName();

            foreach (DataRow row in dtDatabase.Rows)
            {
                comboDatabaseName.Items.Add(row["DatabaseName"]);
            }

        }

        private void _FillListViewWithColumnsData()
        {
            listviewColumnsInfo.Items.Clear();

            DataTable dt = clsCodeGenerator.GetColumnsNameWithInfo(_TableName, comboDatabaseName.Text);

            // Loop through the rows in the DataTable and add them to the ListView
            foreach (DataRow row in dt.Rows)
            {
                ListViewItem item = new ListViewItem(row["Column Name"].ToString()); // Column1 represents the first column in your DataTable
                item.SubItems.Add(row["Data Type"].ToString()); // Column2 represents the second column in your DataTable
                item.SubItems.Add(row["Is Nullable"].ToString()); // Column3 represents the third column in your DataTable
                                                                  // Add more sub-items as needed for additional columns

                listviewColumnsInfo.Items.Add(item);
            }

            // To get the single column name
            if (listviewColumnsInfo.Items.Count > 0)
            {
                string FirstValue = listviewColumnsInfo.Items[0].Text;
                _TableSingleName = FirstValue.Remove(FirstValue.Length - 2);
            }

            // To show the number of records
            lblNumberOfColumnsRecords.Text = listviewColumnsInfo.Items.Count.ToString();
        }

        private string _GetDataTypeCSharp(string DataType)
        {

            switch (DataType)
            {
                case "int": return "int";

                case "bigint": return "long";

                case "float": return "float";

                case "decimal": return "decimal";

                case "money": return "decimal";

                case "smallmoney": return "decimal";

                case "smallint": return "short";

                case "tinyint": return "byte";

                case "nvarchar": return "string";

                case "varchar": return "string";

                case "char": return "char";

                case "datetime": return "DateTime";

                case "date": return "DateTime";

                case "bit": return "bool";

                default: return "string";

            }
        }

        private string _GetConnectionString()
        {
            return "SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);";
        }

        private string _MakeParametersForFindMethod()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;
                    string DataType = firstItem.SubItems[1].Text;
                    //string IsNullable = firstItem.SubItems[2].Text;

                    if (i == 0)
                    {
                        Parameters += _GetDataTypeCSharp(DataType) + " " + ColumnName + ", ";
                    }
                    else
                    {
                        Parameters += "ref " + _GetDataTypeCSharp(DataType) + " " + ColumnName + ", ";
                    }

                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ")";

            return Parameters.Trim();
        }

        private string _FillTheVariableWithDataThatComingFromDatabase()
        {
            string Text = string.Empty;

            for (int i = 1; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondItem = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondItem.SubItems.Count > 0)
                {
                    string ColumnName = SecondItem.SubItems[0].Text;
                    string DataType = SecondItem.SubItems[1].Text;
                    string IsNullable = SecondItem.SubItems[2].Text;

                    if (IsNullable == "YES")
                    {
                        switch (DataType)
                        {
                            case "int":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0;";
                                break;

                            case "bigint":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0;";
                                break;

                            case "float":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0f;";
                                break;

                            case "decimal":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                break;

                            case "money":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                break;

                            case "smallmoney":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                break;

                            case "smallint":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : (short)0;";
                                break;

                            case "tinyint":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : (byte)0;";
                                break;

                            case "nvarchar":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : string.Empty;";
                                break;

                            case "varchar":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : string.Empty;";
                                break;

                            case "char":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : ' ';";
                                break;

                            case "datetime":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : DateTime.Now;";
                                break;

                            case "date":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : DateTime.Now;";
                                break;

                            case "bit":
                                Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : false;";
                                break;
                        }

                        Text += Environment.NewLine;
                    }
                    else
                    {
                        Text += ColumnName + " = (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"];" + Environment.NewLine;
                    }
                }

            }

            return Text.Trim();
        }

        private void _CreateFindMethod()
        {
            txtDataAccessLayer.Text = $"public static bool Get{_TableSingleName}InfoByID{_MakeParametersForFindMethod()}";

            txtDataAccessLayer.Text += Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "bool IsFound = false;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"string query = @\"select * from {_TableName} where {_TableSingleName}ID = @{_TableSingleName}ID\";" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@{_TableSingleName}ID\", {_TableSingleName}ID);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlDataReader reader = command.ExecuteReader();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "if (reader.Read())" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "// The record was found" + Environment.NewLine + "IsFound = true;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _FillTheVariableWithDataThatComingFromDatabase() + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "else" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "// The record was not found" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "reader.Close();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return IsFound;" + Environment.NewLine + "}" + Environment.NewLine;
        }

        private string _MakeParametersForFindMethodForUsername()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;
                    string DataType = firstItem.SubItems[1].Text;

                    if (ColumnName.ToLower() == "username")
                    {
                        Parameters += _GetDataTypeCSharp(DataType) + " " + ColumnName + ", ";
                    }
                    else
                    {
                        Parameters += "ref " + _GetDataTypeCSharp(DataType) + " " + ColumnName + ", ";
                    }

                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ")";

            return Parameters.Trim();
        }

        private string _FillTheVariableWithDataThatComingFromDatabaseForUsername()
        {
            string Text = string.Empty;

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem FirstValue = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (FirstValue.SubItems.Count > 0)
                {
                    string ColumnName = FirstValue.SubItems[0].Text;
                    string DataType = FirstValue.SubItems[1].Text;
                    string IsNullable = FirstValue.SubItems[2].Text;

                    if (ColumnName.ToLower() != "username")
                    {
                        if (IsNullable == "YES")
                        {
                            switch (DataType)
                            {
                                case "int":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0;";
                                    break;

                                case "bigint":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0;";
                                    break;

                                case "float":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0f;";
                                    break;

                                case "decimal":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                    break;

                                case "money":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                    break;

                                case "smallmoney":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                    break;

                                case "smallint":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : (short)0;";
                                    break;

                                case "tinyint":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : (byte)0;";
                                    break;

                                case "nvarchar":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : string.Empty;";
                                    break;

                                case "varchar":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : string.Empty;";
                                    break;

                                case "char":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : ' ';";
                                    break;

                                case "datetime":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : DateTime.Now;";
                                    break;

                                case "date":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : DateTime.Now;";
                                    break;

                                case "bit":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : false;";
                                    break;
                            }

                            Text += Environment.NewLine;
                        }
                        else
                        {
                            Text += ColumnName + " = (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"];" + Environment.NewLine;
                        }
                    }


                }

            }

            return Text.Trim();
        }

        private void _CreateFindMethodForUsername()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static bool Get{_TableSingleName}InfoByUsername{_MakeParametersForFindMethodForUsername()}";

            txtDataAccessLayer.Text += Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "bool IsFound = false;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"string query = @\"select * from {_TableName} where Username = @Username COLLATE SQL_Latin1_General_CP1_CS_AS\";" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@Username\", Username);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlDataReader reader = command.ExecuteReader();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "if (reader.Read())" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "// The record was found" + Environment.NewLine + "IsFound = true;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _FillTheVariableWithDataThatComingFromDatabaseForUsername() + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "else" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "// The record was not found" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "reader.Close();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return IsFound;" + Environment.NewLine + "}" + Environment.NewLine;
        }

        private string _MakeParametersForFindMethodForUsernameAndPassword()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;
                    string DataType = firstItem.SubItems[1].Text;

                    if (ColumnName.ToLower() == "username" || ColumnName.ToLower() == "password")
                    {
                        Parameters += _GetDataTypeCSharp(DataType) + " " + ColumnName + ", ";
                    }
                    else
                    {
                        Parameters += "ref " + _GetDataTypeCSharp(DataType) + " " + ColumnName + ", ";
                    }

                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ")";

            return Parameters.Trim();
        }

        private string _FillTheVariableWithDataThatComingFromDatabaseForUsernameAndPassword()
        {
            string Text = string.Empty;

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem FirstValue = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (FirstValue.SubItems.Count > 0)
                {
                    string ColumnName = FirstValue.SubItems[0].Text;
                    string DataType = FirstValue.SubItems[1].Text;
                    string IsNullable = FirstValue.SubItems[2].Text;

                    if (ColumnName.ToLower() != "username" && ColumnName.ToLower() != "password")
                    {
                        if (IsNullable == "YES")
                        {
                            switch (DataType)
                            {
                                case "int":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0;";
                                    break;

                                case "bigint":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0;";
                                    break;

                                case "float":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0f;";
                                    break;

                                case "decimal":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                    break;

                                case "money":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                    break;

                                case "smallmoney":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : 0M;";
                                    break;

                                case "smallint":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : (short)0;";
                                    break;

                                case "tinyint":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : (byte)0;";
                                    break;

                                case "nvarchar":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : string.Empty;";
                                    break;

                                case "varchar":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : string.Empty;";
                                    break;

                                case "char":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : ' ';";
                                    break;

                                case "datetime":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : DateTime.Now;";
                                    break;

                                case "date":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : DateTime.Now;";
                                    break;

                                case "bit":
                                    Text += ColumnName + " = (reader[\"" + ColumnName + "\"] != DBNull.Value) ? (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"] : false;";
                                    break;
                            }

                            Text += Environment.NewLine;
                        }
                        else
                        {
                            Text += ColumnName + " = (" + _GetDataTypeCSharp(DataType) + ")reader[\"" + ColumnName + "\"];" + Environment.NewLine;
                        }
                    }
                }

            }

            return Text.Trim();
        }

        private void _CreateFindMethodForUsernameAndPassword()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static bool Get{_TableSingleName}InfoByUsernameAndPassword{_MakeParametersForFindMethodForUsernameAndPassword()}";

            txtDataAccessLayer.Text += Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "bool IsFound = false;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"string query = @\"select * from {_TableName} where Username = @Username COLLATE SQL_Latin1_General_CP1_CS_AS AND Password = @Password COLLATE SQL_Latin1_General_CP1_CS_AS\";" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@Username\", Username);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@Password\", Password);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlDataReader reader = command.ExecuteReader();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "if (reader.Read())" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "// The record was found" + Environment.NewLine + "IsFound = true;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _FillTheVariableWithDataThatComingFromDatabaseForUsernameAndPassword() + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "else" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "// The record was not found" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "reader.Close();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return IsFound;" + Environment.NewLine + "}" + Environment.NewLine;
        }

        private string _MakeParametersForAddNewMethod()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 1; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondRow = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondRow.SubItems.Count > 0)
                {
                    string ColumnName = SecondRow.SubItems[0].Text;
                    string DataType = SecondRow.SubItems[1].Text;

                    Parameters += _GetDataTypeCSharp(DataType) + " " + ColumnName + ", ";
                }

            }

            // To remove the ", " from the end of the text
            if (Parameters.Length >= 2)
            {
                Parameters = Parameters.Remove(Parameters.Length - 2);
            }

            Parameters += ")";

            return Parameters.Trim();
        }

        private string _GetQueryForAddNewMethod()
        {
            string query = string.Empty;

            if (_IsLogin)
            {
                query += "string query = " + $"@\"if not exists (select found = 1 from {_TableName} where Username = @Username)"
                    + Environment.NewLine + "begin" + Environment.NewLine + $"insert into {_TableName} (";
            }
            else
            {
                query += "string query = " + $"@\"insert into {_TableName} (";
            }


            // Print the header of the columns
            for (int i = 1; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondItem = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondItem.SubItems.Count > 0)
                {
                    string ColumnName = SecondItem.SubItems[0].Text;

                    query += ColumnName + ", ";
                }

            }

            // To remove the ", " from the end of the query
            query = query.Remove(query.Length - 2);

            query += ")" + Environment.NewLine;

            query += "values (";

            // Print the values
            for (int i = 1; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondItem = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondItem.SubItems.Count > 0)
                {
                    string ColumnName = SecondItem.SubItems[0].Text;

                    query += "@" + ColumnName + ", ";
                }

            }

            // To remove the ", " from the end of the query
            query = query.Remove(query.Length - 2);

            query += ")" + Environment.NewLine;

            if (_IsLogin)
            {
                query += "select scope_identity()" + Environment.NewLine + "end\";";
            }
            else
            {
                query += "select scope_identity()\";";
            }


            return query;
        }

        private string _FillParametersInTheCommand()
        {
            string Text = string.Empty;

            for (int i = 1; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondItem = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondItem.SubItems.Count > 0)
                {
                    string ColumnName = SecondItem.SubItems[0].Text;
                    string DataType = SecondItem.SubItems[1].Text;
                    string IsNullable = SecondItem.SubItems[2].Text;

                    if (IsNullable == "YES")
                    {
                        switch (DataType)
                        {

                            case "int":
                                Text += $"if ({ColumnName} <= 0)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "bigint":
                                Text += $"if ({ColumnName} <= 0)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "float":
                                Text += $"if ({ColumnName} <= 0)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "decimal":
                                Text += $"if ({ColumnName} <= 0)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "money":
                                Text += $"if ({ColumnName} <= 0)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "smallmoney":
                                Text += $"if ({ColumnName} <= 0)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "tinyint":
                                Text += $"if ({ColumnName} <= 0)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "smallint":
                                Text += $"if ({ColumnName} <= 0)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "nvarchar":
                                Text += $"if (string.IsNullOrWhiteSpace({ColumnName}))" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "varchar":
                                Text += $"if (string.IsNullOrWhiteSpace({ColumnName}))" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "char":
                                Text += $"if ({ColumnName} == ' ')" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "datetime":
                                Text += $"if ({ColumnName} == DateTime.Now)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "date":
                                Text += $"if ({ColumnName} == DateTime.Now)" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                            case "bit":
                                Text += $"if (!{ColumnName})" + Environment.NewLine + "{" + Environment.NewLine;
                                break;

                        }

                        Text += $"command.Parameters.AddWithValue(\"@{ColumnName}\", DBNull.Value);" + Environment.NewLine + "}" + Environment.NewLine;
                        Text += "else" + Environment.NewLine + "{" + Environment.NewLine + $"command.Parameters.AddWithValue(\"@{ColumnName}\", {ColumnName});" + Environment.NewLine + "}" + Environment.NewLine;
                    }
                    else
                    {
                        Text += $"command.Parameters.AddWithValue(\"@{ColumnName}\", {ColumnName});" + Environment.NewLine;
                    }
                }

            }

            return Text.Trim();
        }

        private void _CreateAddNewMethod()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static int AddNew{_TableSingleName}{_MakeParametersForAddNewMethod()}" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "// This function will return the new person id if succeeded and -1 if not" + Environment.NewLine;

            txtDataAccessLayer.Text += $"int {_TableSingleName}ID = -1;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetQueryForAddNewMethod() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _FillParametersInTheCommand() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine + "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "object result = command.ExecuteScalar();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "if (result != null && int.TryParse(result.ToString(), out int InsertID))" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += $"{_TableSingleName}ID = InsertID;" + Environment.NewLine + "}" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"return {_TableSingleName}ID;" + Environment.NewLine + "}" + Environment.NewLine;

        }

        private string _MakeParametersForUpdateMethod()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondRow = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondRow.SubItems.Count > 0)
                {
                    string ColumnName = SecondRow.SubItems[0].Text;
                    string DataType = SecondRow.SubItems[1].Text;

                    Parameters += _GetDataTypeCSharp(DataType) + " " + ColumnName + ", ";
                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ")";

            return Parameters.Trim();
        }

        private string _GetQueryForUpdateMethod()
        {
            string query = string.Empty;

            query += "string query = " + $"@\"Update {_TableName}" + Environment.NewLine + "set ";

            // Print the header of the columns
            for (int i = 1; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondItem = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondItem.SubItems.Count > 0)
                {
                    string ColumnName = SecondItem.SubItems[0].Text;

                    query += ColumnName + " = @" + ColumnName + "," + Environment.NewLine;
                }

            }

            // To remove the ", " from the end of the query
            query = query.Remove(query.Length - 3);

            query += Environment.NewLine + $"where {_TableSingleName}ID = @{_TableSingleName}ID\";";

            return query;
        }

        private void _CreateUpdateMethod()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static bool Update{_TableSingleName}{_MakeParametersForUpdateMethod()}" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "int RowAffected = 0;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetQueryForUpdateMethod() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@{_TableSingleName}ID\", {_TableSingleName}ID);" + Environment.NewLine;

            txtDataAccessLayer.Text += _FillParametersInTheCommand() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine + "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "RowAffected = command.ExecuteNonQuery();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return (RowAffected > 0);" + Environment.NewLine + "}" + Environment.NewLine;
        }

        private string _MakeParametersForDeleteMethod()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            ListViewItem SecondRow = listviewColumnsInfo.Items[0]; // Access the second row (index 0)

            if (SecondRow.SubItems.Count > 0)
            {
                string ColumnName = SecondRow.SubItems[0].Text;
                string DataType = SecondRow.SubItems[1].Text;

                Parameters += _GetDataTypeCSharp(DataType) + " " + ColumnName + ")";
            }

            return Parameters.Trim();
        }

        private void _CreateDeleteMethod()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static bool Delete{_TableSingleName}{_MakeParametersForDeleteMethod()}" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "int RowAffected = 0;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"string query = @\"delete {_TableName} where {_TableSingleName}ID = @{_TableSingleName}ID\";" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@{_TableSingleName}ID\", {_TableSingleName}ID);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine + "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "RowAffected = command.ExecuteNonQuery();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return (RowAffected > 0);" + Environment.NewLine + "}" + Environment.NewLine;

        }

        private void _CreateIsExistsMethod()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static bool Is{_TableSingleName}Exists{_MakeParametersForDeleteMethod()}" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "bool IsFound = false;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"string query = @\"select found = 1 from {_TableName} where {_TableSingleName}ID = @{_TableSingleName}ID\";" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@{_TableSingleName}ID\", {_TableSingleName}ID);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine + "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlDataReader reader = command.ExecuteReader();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = reader.HasRows;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "reader.Close();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return IsFound;" + Environment.NewLine + "}" + Environment.NewLine;
        }

        private void _CreateIsExistsMethodForUsername()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static bool Is{_TableSingleName}Exists(string Username)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "bool IsFound = false;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"string query = @\"select found = 1 from {_TableName} where Username = @Username COLLATE SQL_Latin1_General_CP1_CS_AS\";" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@Username\", Username);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine + "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlDataReader reader = command.ExecuteReader();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = reader.HasRows;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "reader.Close();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return IsFound;" + Environment.NewLine + "}" + Environment.NewLine;
        }

        private void _CreateIsExistsMethodForUsernameAndPassword()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static bool Is{_TableSingleName}Exists(string Username, string Password)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "bool IsFound = false;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"string query = @\"select found = 1 from {_TableName} where Username = @Username COLLATE SQL_Latin1_General_CP1_CS_AS and Password = @Password COLLATE SQL_Latin1_General_CP1_CS_AS\";" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@Username\", Username);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"command.Parameters.AddWithValue(\"@Password\", Password);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine + "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlDataReader reader = command.ExecuteReader();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = reader.HasRows;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "reader.Close();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "IsFound = false;" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return IsFound;" + Environment.NewLine + "}" + Environment.NewLine;
        }

        private void _CreateGetAllMethod()
        {
            txtDataAccessLayer.Text += Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"public static DataTable GetAll{_TableName}()" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "DataTable dt = new DataTable();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetConnectionString() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += $"string query = @\"select * from {_TableName}\";" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlCommand command = new SqlCommand(query, connection);" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "try" + Environment.NewLine + "{" + Environment.NewLine + "connection.Open();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "SqlDataReader reader = command.ExecuteReader();" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "if (reader.HasRows)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "dt.Load(reader);" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "reader.Close();" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "catch (Exception ex)" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "" + Environment.NewLine + "}" + Environment.NewLine;

            txtDataAccessLayer.Text += "finally" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "connection.Close();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "return dt;" + Environment.NewLine + "}" + Environment.NewLine;
        }

        private string _MakeParametersForBusinessLayer()
        {
            string Parameters = string.Empty;

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;
                    string DataType = firstItem.SubItems[1].Text;

                    Parameters += "public " + _GetDataTypeCSharp(DataType) + " " + ColumnName + " { get; set; }" + Environment.NewLine;
                }

            }

            return Parameters.Trim();
        }

        private string _GetPublicConstructor()
        {
            string Constructor = string.Empty;

            Constructor += $"public cls{_TableSingleName}()" + Environment.NewLine + "{" + Environment.NewLine;

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;
                    string DataType = firstItem.SubItems[1].Text;

                    switch (DataType)
                    {

                        case "int":
                            Constructor += $"this.{ColumnName} = -1;" + Environment.NewLine;
                            break;

                        case "bigint":
                            Constructor += $"this.{ColumnName} = -1;" + Environment.NewLine;
                            break;

                        case "float":
                            Constructor += $"this.{ColumnName} = -1F;" + Environment.NewLine;
                            break;

                        case "decimal":
                            Constructor += $"this.{ColumnName} = -1M;" + Environment.NewLine;
                            break;

                        case "money":
                            Constructor += $"this.{ColumnName} = -1M;" + Environment.NewLine;
                            break;

                        case "smallmoney":
                            Constructor += $"this.{ColumnName} = -1M;" + Environment.NewLine;
                            break;

                        case "tinyint":
                            Constructor += $"this.{ColumnName} = 0;" + Environment.NewLine;
                            break;

                        case "smallint":
                            Constructor += $"this.{ColumnName} = -1;" + Environment.NewLine;
                            break;

                        case "nvarchar":
                            Constructor += $"this.{ColumnName} = string.Empty;" + Environment.NewLine;
                            break;

                        case "varchar":
                            Constructor += $"this.{ColumnName} = string.Empty;" + Environment.NewLine;
                            break;

                        case "char":
                            Constructor += $"this.{ColumnName} = ' ';" + Environment.NewLine;
                            break;

                        case "datetime":
                            Constructor += $"this.{ColumnName} = DateTime.Now;" + Environment.NewLine;
                            break;

                        case "date":
                            Constructor += $"this.{ColumnName} = DateTime.Now;" + Environment.NewLine;
                            break;

                        case "bit":
                            Constructor += $"this.{ColumnName} = false;" + Environment.NewLine;
                            break;

                    }

                }

            }

            Constructor += Environment.NewLine + "Mode = enMode.AddNew;" + Environment.NewLine + "}" + Environment.NewLine;

            return Constructor.Trim();
        }

        private string _GetPrivateConstructor()
        {
            string Constructor = string.Empty;

            Constructor += $"private cls{_TableSingleName}{_MakeParametersForUpdateMethod()}" + Environment.NewLine + "{" + Environment.NewLine;

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;
                    string DataType = firstItem.SubItems[1].Text;

                    switch (DataType)
                    {

                        case "int":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "bigint":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "float":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "decimal":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "money":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "smallmoney":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "tinyint":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "smallint":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "nvarchar":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "varchar":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "char":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "datetime":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "date":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                        case "bit":
                            Constructor += $"this.{ColumnName} = {ColumnName};" + Environment.NewLine;
                            break;

                    }

                }

            }

            Constructor += Environment.NewLine + "Mode = enMode.Update;" + Environment.NewLine + "}" + Environment.NewLine;

            return Constructor.Trim();
        }

        private string _MakeParametersForAddNewMethodInBusinessLayer()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 1; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondRow = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondRow.SubItems.Count > 0)
                {
                    string ColumnName = SecondRow.SubItems[0].Text;

                    Parameters += "this." + ColumnName + ", ";
                }

            }

            // To remove the ", " from the end of the text
            if (Parameters.Length >= 2)
            {
                Parameters = Parameters?.Remove(Parameters.Length - 2);
            }

            Parameters += ");";

            return Parameters.Trim();
        }

        private string _GetAddNewInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"private bool _AddNew{_TableSingleName}()" + Environment.NewLine + "{" + Environment.NewLine;

            Text += $"this.{_TableSingleName}ID = cls{_TableSingleName}Data.AddNew{_TableSingleName}{_MakeParametersForAddNewMethodInBusinessLayer()}" + Environment.NewLine + Environment.NewLine;

            Text += $"return (this.{_TableSingleName}ID != -1);" + Environment.NewLine + "}";

            return Text.Trim();
        }

        private string _MakeParametersForUpdateMethodInBusinessLayer()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem FirstRow = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (FirstRow.SubItems.Count > 0)
                {
                    string ColumnName = FirstRow.SubItems[0].Text;

                    Parameters += "this." + ColumnName + ", ";
                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ");";

            return Parameters.Trim();
        }

        private string _GetUpdateInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"private bool _Update{_TableSingleName}()" + Environment.NewLine + "{" + Environment.NewLine;

            Text += $"return cls{_TableSingleName}Data.Update{_TableSingleName}{_MakeParametersForUpdateMethodInBusinessLayer()}" + Environment.NewLine + "}";

            return Text.Trim();
        }

        private string _GetSaveMethod()
        {
            string Text = string.Empty;

            Text += "public bool Save()" + Environment.NewLine + "{" + Environment.NewLine;

            Text += "switch (Mode)" + Environment.NewLine + "{" + Environment.NewLine;

            Text += "case enMode.AddNew:" + Environment.NewLine;

            Text += $"if (_AddNew{_TableSingleName}())" + Environment.NewLine + "{" + Environment.NewLine;

            Text += "Mode = enMode.Update;" + Environment.NewLine + "return true;" + Environment.NewLine + "}" + Environment.NewLine;

            Text += "else" + Environment.NewLine + "{" + Environment.NewLine + "return false;" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            Text += "case enMode.Update:" + Environment.NewLine + $"return _Update{_TableSingleName}();" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            Text += "return false;" + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine;

            return Text.Trim();
        }

        private string _MakeInitialParametersForFindMethodInBusinessLayer()
        {
            string Variable = string.Empty;

            for (int i = 1; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem SecondRow = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (SecondRow.SubItems.Count > 0)
                {
                    string ColumnName = SecondRow.SubItems[0].Text;
                    string DataType = SecondRow.SubItems[1].Text;

                    switch (DataType)
                    {

                        case "int":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                            break;

                        case "bigint":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                            break;

                        case "float":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1F;" + Environment.NewLine;
                            break;

                        case "decimal":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                            break;

                        case "money":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                            break;

                        case "smallmoney":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                            break;

                        case "tinyint":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = 0;" + Environment.NewLine;
                            break;

                        case "smallint":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                            break;

                        case "nvarchar":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = string.Empty;" + Environment.NewLine;
                            break;

                        case "varchar":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = string.Empty;" + Environment.NewLine;
                            break;

                        case "char":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = ' ';" + Environment.NewLine;
                            break;

                        case "datetime":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = DateTime.Now;" + Environment.NewLine;
                            break;

                        case "date":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = DateTime.Now;" + Environment.NewLine;
                            break;

                        case "bit":
                            Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = false;" + Environment.NewLine;
                            break;

                    }
                }

            }

            return Variable.Trim();
        }

        private string _MakeParametersForFindMethodInBusinessLayer()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;

                    if (i == 0)
                    {
                        Parameters += ColumnName + ", ";
                    }
                    else
                    {
                        Parameters += "ref " + ColumnName + ", ";
                    }

                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ");" + Environment.NewLine;

            return Parameters.Trim();
        }

        private string _MakeReturnParametersForFindMethodInBusinessLayer()
        {
            string Parameters = string.Empty;

            Parameters = $"return new cls{_TableSingleName}(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;

                    Parameters += ColumnName + ", ";
                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ");" + Environment.NewLine;

            return Parameters.Trim();
        }

        private string _GetFindMethodInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"public static cls{_TableSingleName} Find{_MakeParametersForDeleteMethod()}" + Environment.NewLine + "{" + Environment.NewLine;

            Text += _MakeInitialParametersForFindMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            Text += $"bool IsFound = cls{_TableSingleName}Data.Get{_TableSingleName}InfoByID{_MakeParametersForFindMethodInBusinessLayer()}" + Environment.NewLine + Environment.NewLine;

            Text += "if (IsFound)" + Environment.NewLine + "{" + Environment.NewLine;

            Text += _MakeReturnParametersForFindMethodInBusinessLayer() + "}" + Environment.NewLine;

            Text += "else" + Environment.NewLine + "{" + Environment.NewLine;

            Text += "return null;" + Environment.NewLine + "}" + Environment.NewLine + "}" + Environment.NewLine;

            return Text.Trim();
        }

        private string _GetDeleteMethodInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"public static bool Delete{_TableSingleName}{_MakeParametersForDeleteMethod()}" + Environment.NewLine + "{" + Environment.NewLine;

            Text += $"return cls{_TableSingleName}Data.Delete{_TableSingleName}({_TableSingleName}ID);" + Environment.NewLine + "}" + Environment.NewLine;

            return Text.Trim();
        }

        private string _GetIsExistsMethodInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"public static bool Is{_TableSingleName}Exists{_MakeParametersForDeleteMethod()}" + Environment.NewLine + "{" + Environment.NewLine;

            Text += $"return cls{_TableSingleName}Data.Is{_TableSingleName}Exists({_TableSingleName}ID);" + Environment.NewLine + "}" + Environment.NewLine;

            return Text.Trim();
        }

        private string _GetAllMethodInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"public static DataTable GetAll{_TableName}()" + Environment.NewLine + "{" + Environment.NewLine;

            Text += $"return cls{_TableSingleName}Data.GetAll{_TableName}();" + Environment.NewLine + "}" + Environment.NewLine;

            return Text.Trim();
        }

        private string _MakeInitialParametersForFindUsernameMethodInBusinessLayer()
        {
            string Variable = string.Empty;

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem FirstRow = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (FirstRow.SubItems.Count > 0)
                {
                    string ColumnName = FirstRow.SubItems[0].Text;
                    string DataType = FirstRow.SubItems[1].Text;

                    if (ColumnName.ToLower() != "username")
                    {
                        switch (DataType)
                        {

                            case "int":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                                break;

                            case "bigint":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                                break;

                            case "float":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1F;" + Environment.NewLine;
                                break;

                            case "decimal":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                                break;

                            case "money":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                                break;

                            case "smallmoney":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                                break;

                            case "tinyint":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = 0;" + Environment.NewLine;
                                break;

                            case "smallint":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                                break;

                            case "nvarchar":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = string.Empty;" + Environment.NewLine;
                                break;

                            case "varchar":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = string.Empty;" + Environment.NewLine;
                                break;

                            case "char":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = ' ';" + Environment.NewLine;
                                break;

                            case "datetime":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = DateTime.Now;" + Environment.NewLine;
                                break;

                            case "date":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = DateTime.Now;" + Environment.NewLine;
                                break;

                            case "bit":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = false;" + Environment.NewLine;
                                break;

                        }
                    }
                }

            }

            return Variable.Trim();
        }

        private string _MakeParametersForFindUsernameMethodInBusinessLayer()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;

                    if (ColumnName.ToLower() == "username")
                    {
                        Parameters += ColumnName + ", ";
                    }
                    else
                    {
                        Parameters += "ref " + ColumnName + ", ";
                    }

                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ");" + Environment.NewLine;

            return Parameters.Trim();
        }

        private string _GetFindUsernameMethodInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"public static cls{_TableSingleName} Find(string Username)" + Environment.NewLine + "{" + Environment.NewLine;

            Text += _MakeInitialParametersForFindUsernameMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            Text += $"bool IsFound = cls{_TableSingleName}Data.Get{_TableSingleName}InfoByUsername{_MakeParametersForFindUsernameMethodInBusinessLayer()}" + Environment.NewLine + Environment.NewLine;

            Text += "if (IsFound)" + Environment.NewLine + "{" + Environment.NewLine;

            Text += _MakeReturnParametersForFindMethodInBusinessLayer() + "}" + Environment.NewLine;

            Text += "else" + Environment.NewLine + "{" + Environment.NewLine;

            Text += "return null;" + Environment.NewLine + "}" + Environment.NewLine + "}" + Environment.NewLine;

            return Text.Trim();
        }

        private string _MakeInitialParametersForFindUsernameAndPasswordMethodInBusinessLayer()
        {
            string Variable = string.Empty;

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem FirstRow = listviewColumnsInfo.Items[i]; // Access the second row (index 0)

                if (FirstRow.SubItems.Count > 0)
                {
                    string ColumnName = FirstRow.SubItems[0].Text;
                    string DataType = FirstRow.SubItems[1].Text;

                    if (ColumnName.ToLower() != "username" && ColumnName.ToLower() != "password")
                    {
                        switch (DataType)
                        {

                            case "int":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                                break;

                            case "bigint":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                                break;

                            case "float":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1F;" + Environment.NewLine;
                                break;

                            case "decimal":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                                break;

                            case "money":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                                break;

                            case "smallmoney":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1M;" + Environment.NewLine;
                                break;

                            case "tinyint":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = 0;" + Environment.NewLine;
                                break;

                            case "smallint":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = -1;" + Environment.NewLine;
                                break;

                            case "nvarchar":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = string.Empty;" + Environment.NewLine;
                                break;

                            case "varchar":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = string.Empty;" + Environment.NewLine;
                                break;

                            case "char":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = ' ';" + Environment.NewLine;
                                break;

                            case "datetime":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = DateTime.Now;" + Environment.NewLine;
                                break;

                            case "date":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = DateTime.Now;" + Environment.NewLine;
                                break;

                            case "bit":
                                Variable += _GetDataTypeCSharp(DataType) + " " + ColumnName + " = false;" + Environment.NewLine;
                                break;

                        }
                    }
                }

            }

            return Variable.Trim();
        }

        private string _MakeParametersForFindUsernameAndPasswordMethodInBusinessLayer()
        {
            string Parameters = string.Empty;

            Parameters = "(";

            for (int i = 0; i < listviewColumnsInfo.Items.Count; i++)
            {

                ListViewItem firstItem = listviewColumnsInfo.Items[i]; // Access the first row (index 0)

                if (firstItem.SubItems.Count > 0)
                {
                    string ColumnName = firstItem.SubItems[0].Text;

                    if (ColumnName.ToLower() == "username" || ColumnName.ToLower() == "password")
                    {
                        Parameters += ColumnName + ", ";
                    }
                    else
                    {
                        Parameters += "ref " + ColumnName + ", ";
                    }

                }

            }

            // To remove the ", " from the end of the text
            Parameters = Parameters.Remove(Parameters.Length - 2);

            Parameters += ");" + Environment.NewLine;

            return Parameters.Trim();
        }

        private string _GetFindUsernameAndPasswordMethodInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"public static cls{_TableSingleName} Find(string Username, string Password)" + Environment.NewLine + "{" + Environment.NewLine;

            Text += _MakeInitialParametersForFindUsernameAndPasswordMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            Text += $"bool IsFound = cls{_TableSingleName}Data.Get{_TableSingleName}InfoByUsernameAndPassword{_MakeParametersForFindUsernameAndPasswordMethodInBusinessLayer()}" + Environment.NewLine + Environment.NewLine;

            Text += "if (IsFound)" + Environment.NewLine + "{" + Environment.NewLine;

            Text += _MakeReturnParametersForFindMethodInBusinessLayer() + "}" + Environment.NewLine;

            Text += "else" + Environment.NewLine + "{" + Environment.NewLine;

            Text += "return null;" + Environment.NewLine + "}" + Environment.NewLine + "}" + Environment.NewLine;

            return Text.Trim();
        }

        private string _GetIsUsernameExistsMethodInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"public static bool Is{_TableSingleName}Exists(string Username)" + Environment.NewLine + "{" + Environment.NewLine;

            Text += $"return cls{_TableSingleName}Data.Is{_TableSingleName}Exists(Username);" + Environment.NewLine + "}" + Environment.NewLine;

            return Text.Trim();
        }

        private string _GetIsUsernameAndPasswordExistsMethodInBusinessLayer()
        {
            string Text = string.Empty;

            Text += $"public static bool Is{_TableSingleName}Exists(string Username, string Password)" + Environment.NewLine + "{" + Environment.NewLine;

            Text += $"return cls{_TableSingleName}Data.Is{_TableSingleName}Exists(Username, Password);" + Environment.NewLine + "}" + Environment.NewLine;

            return Text.Trim();
        }

        private void _CreateBusinessLayer()
        {
            txtDataAccessLayer.Text += $"public class cls{_TableSingleName}" + Environment.NewLine + "{" + Environment.NewLine;

            txtDataAccessLayer.Text += "public enum enMode { AddNew = 0, Update = 1 };" + Environment.NewLine;

            txtDataAccessLayer.Text += "public enMode Mode = enMode.AddNew;" + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _MakeParametersForBusinessLayer() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetPublicConstructor() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetPrivateConstructor() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetAddNewInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetUpdateInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetSaveMethod() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetFindMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            if (_IsLogin)
            {
                txtDataAccessLayer.Text += _GetFindUsernameMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

                txtDataAccessLayer.Text += _GetFindUsernameAndPasswordMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;
            }

            txtDataAccessLayer.Text += _GetDeleteMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += _GetIsExistsMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            if (_IsLogin)
            {
                txtDataAccessLayer.Text += _GetIsUsernameExistsMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

                txtDataAccessLayer.Text += _GetIsUsernameAndPasswordExistsMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;
            }

            txtDataAccessLayer.Text += _GetAllMethodInBusinessLayer() + Environment.NewLine + Environment.NewLine;

            txtDataAccessLayer.Text += "}" + Environment.NewLine + Environment.NewLine;
        }

        private void _DataAccessAsLoginInfo()
        {
            _CreateFindMethod();
            _CreateFindMethodForUsername();
            _CreateFindMethodForUsernameAndPassword();
            _CreateAddNewMethod();
            _CreateUpdateMethod();
            _CreateDeleteMethod();
            _CreateIsExistsMethod();
            _CreateIsExistsMethodForUsername();
            _CreateIsExistsMethodForUsernameAndPassword();
            _CreateGetAllMethod();
        }

        private void _DataAccessAsNormal()
        {
            _CreateFindMethod();
            _CreateAddNewMethod();
            _CreateUpdateMethod();
            _CreateDeleteMethod();
            _CreateIsExistsMethod();
            _CreateGetAllMethod();
        }

        private void btnShowDateAccessLayer_Click(object sender, EventArgs e)
        {
            if (int.Parse(lblNumberOfColumnsRecords.Text) <= 0)
            {
                MessageBox.Show("You have to add the row at least!", "Miss Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                txtDataAccessLayer.Clear();

                if (_IsLogin)
                {
                    _DataAccessAsLoginInfo();
                }
                else
                {
                    _DataAccessAsNormal();
                }
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtDataAccessLayer.Text))
            {
                // Copy the text to the clipboard
                Clipboard.SetText(txtDataAccessLayer.Text);
            }
        }

        private void btnShowBusinessLayer_Click(object sender, EventArgs e)
        {
            if (int.Parse(lblNumberOfColumnsRecords.Text) <= 0)
            {
                MessageBox.Show("You have to add the row at least!", "Miss Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                txtDataAccessLayer.Clear();

                _CreateBusinessLayer();
            }
        }

        private void listviewColumnsInfo_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label != null) // Check if the label was edited
            {
                // Update the item's text with the edited label
                listviewColumnsInfo.Items[e.Item].Text = e.Label;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            listviewColumnsInfo.Items.Clear();

            listviewTablesName.Items.Clear();

            txtDataAccessLayer.Clear();

            lblNumberOfColumnsRecords.Text = "0";

            lblNumberOfTablesRecords.Text = "0";

            comboDatabaseName.Text = "";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _FillComboBoxWithDatabaseName();
        }

        private void comboDatabaseName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _FillListViewWithTablesName();

            listviewColumnsInfo.Items.Clear();

            txtDataAccessLayer.Clear();

            lblNumberOfColumnsRecords.Text = "0";
        }

        private void listviewTablesName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listviewTablesName.SelectedItems.Count > 0)
            {
                txtDataAccessLayer.Clear();

                // Access the first value (first column) of the selected item
                _TableName = listviewTablesName.SelectedItems[0].SubItems[0].Text;

                _FillListViewWithColumnsData();

                _IsLogin = _DoesTableHaveUsernameAndPassword();
            }
        }

        private void comboDatabaseName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // To prevent typing in the combobox
            e.Handled = true;
        }

        private void comboDatabaseName_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                comboDatabaseName.DroppedDown = true;
            }
        }
    }
}
