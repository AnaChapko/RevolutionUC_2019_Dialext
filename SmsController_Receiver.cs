/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RevolutionUC_Twilio_Receive_Response_V1.Controllers
{
    public class SmsController : Controller
    {
        // GET: Sms
        public ActionResult Index()
        {
            return View();
        }
    }
}
*/

/*
 Created By Anastasiya Chapko
 Program Description: 
 */

//for twilio
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;
using Twilio.Rest.Api.V2010.Account;
//for List<>
using System.Collections.Generic;
//for accdb
using System.Data;
using System.Data.OleDb;

namespace WebApplication1.Controllers
{
    public class SmsController: TwilioController
    {
        public TwiMLResult Index(SmsRequest incomingMessage)
        {
            //create a new text file in textfiles_temp folder (make sure to not overwrite but to increment)
            string[] lines = { incomingMessage.From.ToString(), incomingMessage.Body.ToString()};
            // WriteAllLines creates a file, writes a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            System.IO.File.WriteAllLines(@"C:\XXXXXXXXXXXXXXX\RevolutionUC_2019\Temporary_Txt\" + incomingMessage.SmsSid.ToString() + ".txt", lines);

            //copy/paste temp text file to final text file folder destination (where always running portion checks)
            System.IO.File.Copy(@"C:\XXXXXXXXXXXX\RevolutionUC_2019\Temporary_Txt\" + incomingMessage.SmsSid.ToString() + ".txt", @"C:\XXXXXXXX\RevolutionUC_2019\Final_Txt\" + incomingMessage.SmsSid.ToString() + ".txt");

            //reuturn nothing hopefully
            var messagingResponse = new MessagingResponse();
            messagingResponse = null;

            return TwiML(messagingResponse);
        }
    }
}