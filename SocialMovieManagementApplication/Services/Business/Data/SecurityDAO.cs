﻿using SocialMovieManagementApplication.Models;
using System.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.IO;

/* Social Movie Management Applicaiton
 * Version 0.1
 * Cameron Deao
 * 6/25/2021
 * The SecurityDAO performs operations of creation, and retrieval of data
 * within the persistent database.
 */

namespace SocialMovieManagementApplication.Services.Business.Data
{
    public class SecurityDAO
    {
        //Connection string to the persistent database.
        readonly string connectionStr = "Data Source=(localdb)\\MSSQLLocalDB;initial catalog=SocialMovieManagement ;Integrated Security=True;";
        const string key = "asdf78954wer7q5re72er54wr5654dsf";
        //FindByUser method returns true if passed credentials are found within the database.
        public bool FindByUser(UserLoginModel user)
        {
            //SQL query
            string query = "SELECT * FROM dbo.Users WHERE username = @Username";

            //Establishing connection and creating a command.
            SqlConnection conn = new SqlConnection(connectionStr);
            SqlCommand command = new SqlCommand(query, conn);

            //Return bool for outcome of operation.
            bool authenticatedUser = false;

            //Exception handling.
            try
            {
                //Adding a parameter to the command statement.
                command.Parameters.Add("@USERNAME", SqlDbType.VarChar, 100).Value = user.username;

                //Opening the connection.
                conn.Open();

                //Creating a data reader.
                SqlDataReader reader = command.ExecuteReader();

                //Checking if the reader found any rows within the database.
                if (reader.HasRows)
                {
                    //Iterating through the results.
                    while(reader.Read())
                    {
                        if(reader.GetSqlByte(6) == 1)
                        {
                            conn.Close();
                            return false;
                        }
                        //Checking the stored password against the passed password.
                        if(!VerifyHash(reader.GetString(5), user.password))
                        {
                            authenticatedUser = false;
                        }
                        else
                        {
                            //Returning true if a match is found.
                            UserModel logUser = new UserModel();
                            logUser.userID = reader.GetInt32(0);
                            logUser.firstName = reader.GetString(1);
                            logUser.lastName = reader.GetString(2);
                            logUser.emailAddress = reader.GetString(3);
                            logUser.username = reader.GetString(4);
                            if(reader["role"] != DBNull.Value)
                            {
                                logUser.role = reader.GetInt32(7);
                            }
                            //logUser.role = reader.GetInt32(7);
                            UserManagement.Instance._loggedUser = logUser;
                            authenticatedUser = true;
                        }
                    }
                }
                //Closing the reader.
                reader.Close();
            }
            catch(SqlException e)
            {
                Debug.WriteLine("Error generated in retrieval. Details: " + e.ToString());
            }
            finally
            {
                //Closing the connection.
                conn.Close();
            }

            //Returning the results.
            return authenticatedUser;
        }

        //CheckUsername method returns if a username exists within the persistent database.
        public bool CheckUsername(UserModel user)
        {
            //SQL query.
            string query = "SELECT * FROM dbo.Users WHERE USERNAME = @Username";

            //Establishing connection and creating a command.
            SqlConnection conn = new SqlConnection(connectionStr);
            SqlCommand command = new SqlCommand(query, conn);

            //Exception handling.
            try
            {
                //Adding a parameter to the command statement.
                command.Parameters.Add("@USERNAME", SqlDbType.VarChar, 50).Value = user.username;

                //Opening the connection.
                conn.Open();

                //Creating a data reader.
                SqlDataReader reader = command.ExecuteReader();

                //Checking if the reader found any rows within the database.
                if(reader.HasRows)
                {
                    //Closing the reader and the connection.
                    reader.Close();
                    conn.Close();

                    //Returning the results.
                    return true;
                }
            }
            catch(SqlException e)
            {
                Debug.WriteLine("Error generated in retrieval. Details: " + e.ToString());
            }
            finally
            {
                //Closing the connection.
                conn.Close();
            }
            //Returning the results.
            return false;
        }

        //CheckEmail method returns if a email address exists within the persistent database.
        public bool CheckEmail(UserModel user)
        {
            //SQL query.
            string query = "SELECT * FROM dbo.Users WHERE email_address = @Email";

            //Establishing connection and creating a command.
            SqlConnection conn = new SqlConnection(connectionStr);
            SqlCommand command = new SqlCommand(query, conn);

            //Exception handling.
            try
            {
                //Adding a parameter to the command statement.
                command.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = user.emailAddress;

                //Opening the connection.
                conn.Open();

                //Creating a data reader.
                SqlDataReader reader = command.ExecuteReader();

                //Checking if the reader found any rows within the database.
                if(reader.HasRows)
                {
                    //Closing the reader and the connection.
                    reader.Close();
                    conn.Close();

                    //Returning the results.
                    return true;
                }
            }
            catch(SqlException e)
            {
                Debug.WriteLine("Error generated in retrieval. Details: " + e.ToString());
            }
            finally
            {
                //Closing the connection.
                conn.Close();
            }

            //Returing the results.
            return false;
        }

        //RegisterUser method inserts an established UserModel into the persistent database.
        public bool RegisterUser(UserModel user)
        {
            //Establishing return value for outcome of the operation.
            int retValue = 0;

            //Establishing connection.
            SqlConnection conn = new SqlConnection(connectionStr);

            //Opening the connection.
            conn.Open();

            //SQL query.
            string query = "INSERT INTO dbo.Users(first_name, last_name, email_address, username, password, banned) " +
                "VALUES(@fName, @lName, @email, @username, @password, @banned)";

            //Creating a command.
            SqlCommand command = new SqlCommand(query, conn);

            //Adding parameters to the command statement and setting values based on the passed model.
            command.Parameters.Add(new SqlParameter("@fName", SqlDbType.VarChar, 50)).Value = user.firstName;
            command.Parameters.Add(new SqlParameter("@lName", SqlDbType.VarChar, 50)).Value = user.lastName;
            command.Parameters.Add(new SqlParameter("@email", SqlDbType.VarChar, 100)).Value = user.emailAddress;
            command.Parameters.Add(new SqlParameter("@username", SqlDbType.VarChar, 100)).Value = user.username;
            command.Parameters.Add(new SqlParameter("@password", SqlDbType.VarChar, 100)).Value = Hash(user.password);
            command.Parameters.Add(new SqlParameter("@banned", SqlDbType.TinyInt)).Value = 0;

            //Preparing the command.
            command.Prepare();

            //Executing the command and retrieving if the operation was successful.
            retValue = command.ExecuteNonQuery();

            //Closing the connection.
            conn.Close();

            //Checkin the return value of the operation. If 0 the operation failed. If 1 the operation passed.
            if (retValue == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string Hash(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[20]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[40];
            Array.Copy(salt, 0, hashBytes, 0, 20);
            Array.Copy(hash, 0, hashBytes, 20, 20);
            string hashPass = Convert.ToBase64String(hashBytes);
            return hashPass;
        }

        public bool VerifyHash(string hashPass, string password)
        {
            byte[] hashBytes = Convert.FromBase64String(hashPass);
            byte[] salt = new byte[20];
            Array.Copy(hashBytes, 0, salt, 0, 20);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            for(int i = 0; i < 20; i++)
            {
                if(hashBytes[i + 20] != hash[i])
                {
                    return false;
                    throw new UnauthorizedAccessException();
                }
            }
            return true;
        }
    }
}