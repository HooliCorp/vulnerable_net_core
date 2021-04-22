using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using vulnerable_asp_net_core.Models;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Text.Encodings.Web;
using System.Xml;
using System.Xml.Serialization;
using vulnerable_asp_net_core.Utils;

namespace vulnerable_asp_net_core.Controllers
{
    public class SL : Controller
    {
        JavaScriptEncoder _javaScriptEncoder = JavaScriptEncoder.Default;
        public void Show(String s)
        {
            @ViewData["result"] = s;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var id = HttpContext.Request.Query["id"];
            @ViewData["cookie"] = HttpContext.Request.Cookies["slcookie"];
            @ViewData["result"] = id;
            return View();
        }

        [HttpGet]
        public IActionResult SQLInjection()
        {
            string name = RequestUtils.GetIfDefined(Request, "name");
            string pw = RequestUtils.GetIfDefined(Request, "pw");
            string res = "";

            if (name.Length > 0)
            {
                var command = new SQLiteCommand($"SELECT * FROM users WHERE name = '{name}' and pw = '{pw}'",
                    DatabaseUtils._con);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res += reader["name"] + "";
                    }
                }

                Show("Successfully logged in as " + _javaScriptEncoder.Encode(res));
            }


            if (res.Length == 0)
                Show("Please login by providing a valid username and password");

            return View();
        }

        [HttpGet]
        public IActionResult XXE()
        {
            string xml = RequestUtils.GetIfDefined(Request, "xml");

            if (xml.Length <= 0)
            {
                @ViewData["result"] = "upload your request";
            }
            else
            {
                var resolver = new XmlUrlResolver();
                resolver.Credentials = CredentialCache.DefaultCredentials;
                var xmlDoc = new XmlDocument();
                xmlDoc.XmlResolver = resolver;

                try
                {
                    xmlDoc.LoadXml(xml);
                }
                catch (Exception)
                {
                }

                Show("Results of your request: " + string.Empty);

                foreach (XmlNode xn in xmlDoc)
                {
                    if (xn.Name == "user") Show("Results of your request: " + _javaScriptEncoder.Encode(xn.InnerText));
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult XSS()
        {
            var comment = RequestUtils.GetIfDefined(Request, "comment");

            Show("your comment is " + comment);

            return View();
        }

        private static readonly Dictionary<string, string> Users =
            new Dictionary<string, string> {{"evan", "abc"}, {"marta", "001"}};

        [HttpGet]
        public IActionResult BrokenAuthentication()
        {
            var login = RequestUtils.GetIfDefined(Request, "name");
            var pw = RequestUtils.GetIfDefined(Request, "pw");

            if (Users.ContainsKey(login) && Users[login] == pw)
            {
                Show("Successfully logged in as " + _javaScriptEncoder.Encode(login));
            }
            else
            {
                Show("Please login by providing a valid username and password");
            }

            return View();
        }

        private const string UserPwPlain = @"<data>
									 <user>
									 <name>claire</name>
									 <password>clairepw</password>
									 <account>admin</account>
									 </user>
									 <user>
									 <name>alice</name>
									 <password>alicepw</password>
									 <account>user</account>
									 </user>
									 <user>
									 <name>bob</name>
									 <password>bobpw</password>
									 <account>bob</account>
									 </user>
									 </data>";

        [HttpGet]
        public IActionResult XPATHInjection()
        {
            var XmlDoc = new XmlDocument();
            XmlDoc.LoadXml(UserPwPlain);
            var nav = XmlDoc.CreateNavigator();

            var name = RequestUtils.GetIfDefined(Request, "name");
            var pw = RequestUtils.GetIfDefined(Request, "pw");

            var query = "string(//user[name/text()='"
                        + name
                        + "' and password/text() ='"
                        + pw + "']/account/text())";

            var expr = nav.Compile(query);
            var account = Convert.ToString(nav.Evaluate(expr));

            if (account.Length <= 0)
            {
                Show("Please login by providing a valid username and password");
            }
            else
            {
                Show("Successfully logged in as " + _javaScriptEncoder.Encode(account));
            }

            return View();
        }

        // credit card security codes are stored encrypted
        private const string UserCreditCardInfo = @"<data>
									 <user>
									 <name>claire</name>
									 <cardno>11111111</cardno>
									 <secno>ba1f2511fc30423bdbb183fe33f3dd0f</secno>
									 </user>
									 <user>
									 <name>alice</name>
									 <cardno>2222222</cardno>
									 <secno>d2d362cdc6579390f1c0617d74a7913d</secno>
									 </user>
									 <user>
									 <name>bob</name>
									 <cardno>33333333</cardno>
									 <secno>aa3f5bb8c988fa9b75a1cdb1dc4d93fc</secno>
									 </user>
									 </data>";

        [HttpGet]
        public IActionResult SensitiveDataExposure()
        {
            var userDoc = new XmlDocument();
            userDoc.LoadXml(UserPwPlain);
            var loginNav = userDoc.CreateNavigator();

            var creditCardDoc = new XmlDocument();
            creditCardDoc.LoadXml(UserCreditCardInfo);
            var creditCardNav = creditCardDoc.CreateNavigator();

            var login = RequestUtils.GetIfDefined(Request, "name");
            var pw = RequestUtils.GetIfDefined(Request, "pw");
            var cardprop = RequestUtils.GetIfDefined(Request, "cardprop");

            if (cardprop.Length == 0)
                cardprop = "cardno";

            // authenticate user
            var authQuery = "string(//user[name/text()='"
                            + login
                            + "' and password/text() ='"
                            + pw + "']/account/text())";

            var account = Convert.ToString(loginNav.Evaluate(loginNav.Compile(authQuery)));
            if (account.Length <= 0)
            {
                Show("Please login by providing a valid username and password");
            }
            else
            {
                var cardno = "string(//user[name/text()='"
                             + login
                             + "']/" + cardprop + "/text())";

                var creditCard = Convert.ToString(creditCardNav.Evaluate(creditCardNav.Compile(cardno)));
                Show("'" + _javaScriptEncoder.Encode(login) 
                          + "' successfully logged in; your card-number is '" 
                          + _javaScriptEncoder.Encode(creditCard) 
                          + "'");
            }

            return View();
        }

        [HttpGet]
        public IActionResult SecurityMisconfiguration()
        {
            var command = new SQLiteCommand("SELECT * FROM user WHERE id = 10", DatabaseUtils._con);
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Show("Hello " + _javaScriptEncoder.Encode(reader["Name"] + "") + "!");
                    }
                    else
                    {
                        Show(string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                Show(e.Message);
            }

            return View();
        }

        [HttpGet]
        public IActionResult BrokenAccessControl()
        {
            var role = RequestUtils.GetIfDefined(Request, "role");

            if (role != "admin")
                role = "user";

            var id = RequestUtils.GetIfDefined(Request, "id");
            if (id.Length == 0)
                id = "0";

            if (role.Equals("admin"))
            {
                return LocalRedirect("/SL/Admin?id=" + _javaScriptEncoder.Encode(id));
            }

            Show("Logged in as '" + _javaScriptEncoder.Encode(role) + "'");

            return View();
        }

        [HttpGet]
        public IActionResult Admin()
        {
            var id = RequestUtils.GetIfDefined(Request, "id");
            if (id.Length == 0)
                id = "0";

            if (id == "0") return View();

            var command = new SQLiteCommand($"DELETE FROM users WHERE id = {id}",
                DatabaseUtils._con);

            if (command.ExecuteNonQuery() > 0)
            {
                Show("Deleted user with id " + _javaScriptEncoder.Encode(id));
            }
            else
            {
                Show(string.Empty);
            }

            return View();
        }

        [HttpGet]
        public IActionResult InsecureDeserialization()
        {
            // TODO:
            //https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.formatters.binary.binaryformatter?view=netframework-4.7.2
            var xml = RequestUtils.GetIfDefined(Request, "xml");

            if (xml.Length == 0)
                return View();
            
            var ser_xml = new XmlSerializer(typeof(Executable));
            try
            {
                var sreader = new StringReader(xml);
                var xread = XmlReader.Create(sreader);
                var exe = (Executable)ser_xml.Deserialize(xread);
                Show("Request results: \'" + _javaScriptEncoder.Encode(exe.Run()) + "\'");
            }
            catch (Exception)
            {
                Show("Request results: \'\'");
            }


            return View();
        }

        [HttpGet]
        public IActionResult InsufficientLogging()
        {
            var log = "";

            string Msg(string msg)
            {
                return new DateTime() + ":" + msg + "</br>";
            }

            if (Request.Query.ContainsKey("showlogs"))
            {
                log += Msg("[info] user 'alice' logged in");
                log += Msg("[info] user 'claire' logged out");
                log += Msg("[info] user 'bob' logged in");
                log += Msg("[info] user 'bob' logged out");
                log += Msg("[warn] /data is almost full");
            }

            Show(log);
            return View();
        }

        [HttpGet]
        public IActionResult VulnerableComponent()
        {
            var comment = RequestUtils.GetIfDefined(Request, "comment");

            Show($"your comment is \'" + Utils.VulnerableComponent.process(comment) + "\'");

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
