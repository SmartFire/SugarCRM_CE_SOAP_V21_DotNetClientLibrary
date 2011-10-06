/*
 *   SugarCRM SOAP Client Console Example
 *   Copyright (C) 2011  Antonio Musarra <antonio.musarra@gmail.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

/*
 * Created by SharpDevelop.
 * User: amusarra
 * Date: 28/09/2011
 * Time: 17.35
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.CodeDom;
using System.Text;
using System.Collections.Generic;
using shirus.crm.phpfogapp.com;

namespace SugarCRM_SOAP_Console_Client
{
	/// <summary>
	/// Description of ClientSugarCRM.
	/// </summary>
	class ClientSugarCRM
	{
		/// <summary>
		/// Gets the MD5 hash value for the passed in value parameter
		/// See Source at Blog of Nick Olsen's http://nickstips.wordpress.com/2010/07/30/c-md5-hash-method/
		/// </summary>
		/// <param name="value">The string value to hash</param>
		/// <param name="upperCase">Indicates whether or not the return value should be upper case</param>
		/// <returns>The MD5 hash of the value parameter</returns>
		public static string GetMD5Hash(string value, bool upperCase)
		{
			// Instantiate new MD5 Service Provider to perform the hash
			System.Security.Cryptography.MD5CryptoServiceProvider md5ServiceProdivder = new System.Security.Cryptography.MD5CryptoServiceProvider();

			// Get a byte array representing the value to be hashed and hash it
			byte[] data = System.Text.Encoding.ASCII.GetBytes(value);
			data = md5ServiceProdivder.ComputeHash(data);

			// Get the hashed string value
			StringBuilder hashedValue = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
				hashedValue.Append(data[i].ToString("x2"));

			// Return the string in all caps if desired
			if (upperCase)
				return hashedValue.ToString().ToUpper();

			return hashedValue.ToString();
		}

		public static void Main(string[] args)
		{
			String sugarCRMUserName = null;
			String sugarCRMPassword = null;
			Uri SugarCRM_URL = null;
			
			// Check Command line arguments
			try {
				if (args.Length < 2) {
					Console.Error.WriteLine("Usage: SugarCRMClient.exe {username} {password} [SugarCRM URL]");
					return;
				}
				
				if (args.Length == 3) {
					SugarCRM_URL = new Uri(args[2]);
				}
				
				sugarCRMPassword = args[0];
				sugarCRMUserName = args[1];
				
			} catch (Exception e) {
				Console.Error.WriteLine("+++ Errors occurred +++");
				Console.Error.WriteLine(e.Message);
				return;
			}
			
			try {
				Console.WriteLine("SugarCRM Console Client 1.0.0");
				
				// Create a new instance of SugarCRM SOAP Proxy Client
				shirus.crm.phpfogapp.com.sugarsoap client = new shirus.crm.phpfogapp.com.sugarsoap();
				
				if (SugarCRM_URL != null) {
					client.Url = SugarCRM_URL.AbsoluteUri + "service/v2_1/soap.php";
				}
				
				/**
				 * Step 1) Try login to SugarCRM istance with username and password
				 */
				
				// Prepare a User Auth Object
				shirus.crm.phpfogapp.com.user_auth userAuthInfo = new shirus.crm.phpfogapp.com.user_auth();
				userAuthInfo.user_name = sugarCRMUserName;
				userAuthInfo.password = GetMD5Hash(sugarCRMPassword,false);
				
				// Execute a login as admin
				shirus.crm.phpfogapp.com.entry_value userSession = client.login(userAuthInfo, "SugarCRM Console Client 1.0.0", null);
				
				/**
				 * Step 2) Get SugarCRM Server Info
				 */
				Console.WriteLine("SugarCRM EndPoint URL: " + client.Url);
				Console.WriteLine("SugarCRM Server Version: " + client.get_server_info().version);
				Console.WriteLine("SugarCRM Server Edition: " + client.get_server_info().flavor);
				Console.WriteLine("SugarCRM Server Time: " + client.get_server_info().gmt_time);
				
				/**
				 * Step 3) Get logged user info
				 */
				String userId = client.get_user_id(userSession.id);
				Console.WriteLine("Welcome Your SessionID: " + userSession.id);
				Console.WriteLine("Get user data...");
				Console.WriteLine("---------- BEGIN USER DATA ----------");
				shirus.crm.phpfogapp.com.get_entry_result_version2 userInfo = client.get_entry(userSession.id, userSession.module_name, userId, null, null);
				
				foreach (shirus.crm.phpfogapp.com.name_value nameValue in userInfo.entry_list[0].name_value_list) {
					if (nameValue.value.Length > 0 && nameValue.name != "user_hash") {
						Console.WriteLine(nameValue.name + ": " + nameValue.value);
					}
				}

				Console.WriteLine("---------- END USER DATA ----------");
				
				/**
				 * Step 4) Insert new contact
				 */
				Console.WriteLine("Try insert new Contact...");
				
				Dictionary<string, string> contactsData = new Dictionary<string, string>();
				contactsData.Add("first_name", "Antonio");
				contactsData.Add("last_name","Musarra");
				contactsData.Add("title", "IT Senior Consultant");
				contactsData.Add("description", "Contacts created bye .NET SOAP Client");
				contactsData.Add("email1","antonio.musarra@gmail.com");
				
				shirus.crm.phpfogapp.com.name_value[] name_value_list = new shirus.crm.phpfogapp.com.name_value[contactsData.Count];

				int i = 0;
				foreach (KeyValuePair<string, string> kvp in contactsData) {
					name_value_list[i] = new shirus.crm.phpfogapp.com.name_value();
					name_value_list[i].name = kvp.Key;
					name_value_list[i].value = kvp.Value;
					i++;
				}
				
				shirus.crm.phpfogapp.com.new_set_entry_result contactResult = client.set_entry(userSession.id, "Contacts", name_value_list);
				Console.WriteLine("New Contact as Id:" + contactResult.id);
				
				/**
				 * Step 5) Get My Contacts
				 */
				Console.WriteLine("Get data for Contacts: " + contactResult.id);
				Console.WriteLine("---------- BEGIN CONTACTS DATA ----------");
				shirus.crm.phpfogapp.com.get_entry_result_version2 myContacts = client.get_entry(userSession.id, "Contacts", contactResult.id, null, null);
				
				foreach (shirus.crm.phpfogapp.com.name_value nameValue in myContacts.entry_list[0].name_value_list) {
					if (nameValue.value.Length > 0 && nameValue.name != "user_hash") {
						Console.WriteLine(nameValue.name + ": " + nameValue.value);
					}
				}
				Console.WriteLine("---------- END CONTACTS DATA ----------");
				
				/**
				 * Step 6) Logout
				 */
				client.logout(userSession.id);
				Console.WriteLine("User disconnected");
				
			} catch (System.Web.Services.Protocols.SoapException e) {
				Console.Error.WriteLine("+++ Errors occurred +++");
				Console.Error.WriteLine("SugarCRM SOAP Fault Message: " + e.Message);
				Console.Error.WriteLine("SugarCRM SOAP Fault Code: " + e.Code);
			} catch (Exception e) {
				Console.Error.WriteLine("+++ Errors occurred +++");
				Console.Error.WriteLine(e.Message);
				Console.Error.WriteLine(e.InnerException);
			} finally {
				Console.WriteLine("Press any key to continue...");
				Console.ReadKey(true);
			}
		}
	}
}
