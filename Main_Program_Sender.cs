
using System;
//for Azure Translate
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
//for Twilio
using Twilio;
using Twilio.Rest.Api.V2010.Account;
//for List<>
using System.Collections.Generic;
//for accdb
using System.Data;
using System.Data.OleDb;

class Program
{
    static void Main(string[] args)
    {
        bool Admin_Debug_Bool = false; //lets admin know when new people join chat session

        string Twilio_Phone = "+XXXXXXXX";
        string Admin_Phone = "+XXXXXXX";
        string Sender_Phone = "";

        string Sender_Body = "";
        string Sender_Language = "ENGLISH"; //english by default, will be updated with value from DB
        bool Sender_is_New = true; //true by default until found in DB
        string Sender_Message_Edited = "";

        string Default_Sender_Error_Message = "Your message encountered an error while sending - we apolagize for this inconvenience - an admin has been notified and this issue should be resolved soon.";

        const string Account_SID = "XXXXXXXX";
        const string Auth_Token = "XXXXXXXX";
        
        TwilioClient.Init(Account_SID, Auth_Token);

        string File_Path = "C:\\XXXXXXXX\\RevolutionUC_2019\\Final_Txt\\";
        System.IO.DirectoryInfo Final_Directory = new System.IO.DirectoryInfo(File_Path);

        //lists for storing each language's users' numbers
        var ENGLISH_NumbersToMessage = new List<string>();
        var PORTUGUESE_NumbersToMessage = new List<string>();
        var RUSSIAN_NumbersToMessage = new List<string>();
        var SPANISH_NumbersToMessage = new List<string>();
        var GERMAN_NumbersToMessage = new List<string>();
        var FRENCH_NumbersToMessage = new List<string>();

        //infinite loop to forever check for new txt files to respond to
        while (1 == 1)
        {
            //check the final text files folder for a new file
            //open file, look at the message info, run through below steps to send response with instructions or translate and pass on to others, close file, delete file
            foreach (System.IO.FileInfo Txt_File in Final_Directory.GetFiles())
            {
                System.Threading.Thread.Sleep(1000);
                if (Txt_File.Extension.ToLower() == ".txt")
                {
                    //*******************************************************************************
                    //BELOW: Extract/Decode information in Txt_File for later use
                    //*******************************************************************************

                    //reset variables
                    Sender_Phone = "";
                    Sender_Body = "";
                    Sender_Language = "ENGLISH"; //english by default, will be updated with value from DB
                    Sender_is_New = true; //true by default until found in DB

                    bool First_Line_Bool = true;
                    string Individual_Line = "";

                    // Read the file and fill in the variables 
                    System.IO.StreamReader Txt_FileStream = new System.IO.StreamReader(Txt_File.FullName);
                    while ((Individual_Line = Txt_FileStream.ReadLine()) != null)
                    {
                        if (First_Line_Bool) //first line is the phone number
                        {
                            Sender_Phone = Individual_Line;
                            First_Line_Bool = false;
                        }
                        else //rest of the lines are the message body (could be multiple lines)
                        {
                            Sender_Body += Individual_Line + " "; //replaces "/n" with " "
                        }
                    }
                    //close file and delete it since it is now taken care of 
                    Txt_FileStream.Close();
                    System.IO.File.Delete(Txt_File.FullName);
                    
                    //get edited body for comparison with common codes (**EXIT**), removes extra spaces and raises letters to uppercase
                    Sender_Message_Edited = Sender_Body.ToString().ToUpper().Replace(" ", "");

                    //! ANA IMPORTANT - if a non-textual message was sent (i.e. a file or image) then just pass it along to the rest of the group w/o translation! *Currently assuming only text-based messages*

                    //we don't need to connect to any databases, since this always runs and we can just check the List Items (which are not cleared)

                    if(ENGLISH_NumbersToMessage.Contains(Sender_Phone))
                    {
                        Sender_Language = "ENGLISH";
                        Sender_is_New = false;
                    }
                    else if(PORTUGUESE_NumbersToMessage.Contains(Sender_Phone))
                    {
                        Sender_Language = "PORTUGUESE";
                        Sender_is_New = false;
                    }
                    else if (RUSSIAN_NumbersToMessage.Contains(Sender_Phone))
                    {
                        Sender_Language = "RUSSIAN";
                        Sender_is_New = false;
                    }
                    else if (SPANISH_NumbersToMessage.Contains(Sender_Phone))
                    {
                        Sender_Language = "SPANISH";
                        Sender_is_New = false;
                    }
                    else if (GERMAN_NumbersToMessage.Contains(Sender_Phone))
                    {
                        Sender_Language = "GERMAN";
                        Sender_is_New = false;
                    }
                    else if (FRENCH_NumbersToMessage.Contains(Sender_Phone))
                    {
                        Sender_Language = "FRENCH";
                        Sender_is_New = false;
                    }


                    if(Sender_is_New == false && Sender_Message_Edited == "**EXIT**")
                    {
                        if (Sender_Language == "ENGLISH")
                        {
                            ENGLISH_NumbersToMessage.Remove(Sender_Phone);
                        }
                        else if (Sender_Language == "PORTUGUESE")
                        {
                            PORTUGUESE_NumbersToMessage.Remove(Sender_Phone);
                        }
                        else if (Sender_Language == "RUSSIAN")
                        {
                            RUSSIAN_NumbersToMessage.Remove(Sender_Phone);
                        }
                        else if (Sender_Language == "SPANISH")
                        {
                            SPANISH_NumbersToMessage.Remove(Sender_Phone);
                        }
                        else if (Sender_Language == "GERMAN")
                        {
                            GERMAN_NumbersToMessage.Remove(Sender_Phone);
                        }
                        else if (Sender_Language == "FRENCH")
                        {
                            FRENCH_NumbersToMessage.Remove(Sender_Phone);
                        }

                    }
                    
                    //*******************************************************************************
                    //BELOW: translate and send out message to users in list
                    //*******************************************************************************

                    //if sender is already in our database and is not trying to leave the chat, then translate and send out their message
                    if ((Sender_is_New == false) && (Sender_Message_Edited != "**EXIT**"))
                    {
                        //counter vairable for keeping track of how many messages were sent out/how many recipients there were
                        int recipientCount = 0;

                        //ENGLISH
                        if (ENGLISH_NumbersToMessage.Count > 0)
                        {
                            //let sender know at least one message was sent out
                            recipientCount = recipientCount + ENGLISH_NumbersToMessage.Count;
                            //Translate to English
                            string ENGLISH_Translation = Sender_Body;
                            if (Sender_Language != "ENGLISH")
                            {
                                ENGLISH_Translation = TranslateText(Sender_Body, "en");  
                            }
                            //send to each recipient in list
                            foreach (var English_Number in ENGLISH_NumbersToMessage)
                            {
                                if (English_Number != Sender_Phone)
                                {
                                    var message = MessageResource.Create(
                                        body: Sender_Phone + ": " + ENGLISH_Translation,
                                        from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                        to: new Twilio.Types.PhoneNumber(English_Number)
                                    );
                                }
                            }
                        }

                        //PORTUGUESE
                        if (PORTUGUESE_NumbersToMessage.Count > 0)
                        {
                            //let sender know at least one message was sent out
                            recipientCount = recipientCount + PORTUGUESE_NumbersToMessage.Count;
                            //Translate to Portuguese
                            string PORTUGUESE_Translation = Sender_Body;
                            if (Sender_Language != "PORTUGUESE")
                            {
                                PORTUGUESE_Translation = TranslateText(Sender_Body, "pt");
                            }
                            //send to each recipient in list
                            foreach (var Portuguese_Number in PORTUGUESE_NumbersToMessage)
                            {
                                if (Portuguese_Number != Sender_Phone)
                                {
                                    var message = MessageResource.Create(
                                        body: Sender_Phone + ": " + PORTUGUESE_Translation,
                                        from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                        to: new Twilio.Types.PhoneNumber(Portuguese_Number)
                                    );
                                }
                            }
                        }

                        //RUSSIAN
                        if (RUSSIAN_NumbersToMessage.Count > 0)
                        {
                            //let sender know at least one message was sent out
                            recipientCount = recipientCount + RUSSIAN_NumbersToMessage.Count;
                            //Translate to Russian
                            string RUSSIAN_Translation = Sender_Body;
                            if (Sender_Language != "RUSSIAN")
                            {
                                RUSSIAN_Translation = TranslateText(Sender_Body, "ru");
                            }
                            //send to each recipient in list
                            foreach (var Russian_Number in RUSSIAN_NumbersToMessage)
                            {
                                if (Russian_Number != Sender_Phone)
                                {
                                    var message = MessageResource.Create(
                                        body: Sender_Phone + ": " + RUSSIAN_Translation,
                                        from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                        to: new Twilio.Types.PhoneNumber(Russian_Number)
                                    );
                                }
                            }
                        }

                        //SPANISH
                        if (SPANISH_NumbersToMessage.Count > 0)
                        {
                            //let sender know at least one message was sent out
                            recipientCount = recipientCount + SPANISH_NumbersToMessage.Count;
                            //Translate to Spanish
                            string SPANISH_Translation = Sender_Body;
                            if (Sender_Language != "SPANISH")
                            {
                                SPANISH_Translation = TranslateText(Sender_Body, "es");
                            }
                            //send to each recipient in list
                            foreach (var Spanish_Number in SPANISH_NumbersToMessage)
                            {
                                if (Spanish_Number != Sender_Phone)
                                {
                                    var message = MessageResource.Create(
                                        body: Sender_Phone + ": " + SPANISH_Translation,
                                        from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                        to: new Twilio.Types.PhoneNumber(Spanish_Number)
                                    );
                                }
                            }
                        }

                        //GERMAN
                        if (GERMAN_NumbersToMessage.Count > 0)
                        {
                            //let sender know at least one message was sent out
                            recipientCount = recipientCount + GERMAN_NumbersToMessage.Count;
                            //Translate to GERMAN
                            string GERMAN_Translation = Sender_Body;
                            if (Sender_Language != "GERMAN")
                            {
                                GERMAN_Translation = TranslateText(Sender_Body, "de");
                            }
                            //send to each recipient in list
                            foreach (var German_Number in GERMAN_NumbersToMessage)
                            {
                                if (German_Number != Sender_Phone)
                                {
                                    var message = MessageResource.Create(
                                        body: Sender_Phone + ": " + GERMAN_Translation,
                                        from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                        to: new Twilio.Types.PhoneNumber(German_Number)
                                    );
                                }
                            }
                        }

                        //FRENCH
                        if (FRENCH_NumbersToMessage.Count > 0)
                        {
                            //let sender know at least one message was sent out
                            recipientCount = recipientCount + FRENCH_NumbersToMessage.Count;
                            //Translate to FRENCH
                            string FRENCH_Translation = Sender_Body;
                            if (Sender_Language != "FRENCH")
                            {
                                FRENCH_Translation = TranslateText(Sender_Body, "fr");
                            }
                            //send to each recipient in list
                            foreach (var French_Number in FRENCH_NumbersToMessage)
                            {
                                if (French_Number != Sender_Phone)
                                {
                                    var message = MessageResource.Create(
                                        body: Sender_Phone + ": " + FRENCH_Translation,
                                        from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                        to: new Twilio.Types.PhoneNumber(French_Number)
                                    );
                                }
                            }
                        }

                        //*******************************************************************************

                        //send a response to the sender to let them know whether or not their message was sent out/delivered
                        if (recipientCount == 0)
                        {
                            //let sender know a problem was encountered
                            var Sender_Message = MessageResource.Create(
                                   body: "**ERROR: There are no other users to chat with - nobody was there to receive your message... :'( **",
                                   from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                   to: new Twilio.Types.PhoneNumber(Sender_Phone)
                               );
                        }
                    }
                    else //Sender_is_New == true (sender is not in our database)
                    {
                        bool User_Added_Successfully = false;
                        //if the message is already telling us their desired language
                        //NEW ENGLISH USER
                        if (Sender_Message_Edited == "ENGLISH")
                        {
                            ENGLISH_NumbersToMessage.Add(Sender_Phone);
                            //give user welcome info and codes in ENGLISH
                            var Sender_Message_Welcome = MessageResource.Create(
                                            body: "**You have been added to the chat! If you ever want to leave the chat, just text '**EXIT**' without the quotes.**",
                                            from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                            to: new Twilio.Types.PhoneNumber(Sender_Phone)
                                        );
                            User_Added_Successfully = true;
                        }
                        //NEW PORTUGUESE USER
                        else if (Sender_Message_Edited == "PORTUGUESE")
                        {
                            PORTUGUESE_NumbersToMessage.Add(Sender_Phone);
                            //give user welcome info and codes in PORTUGUESE
                            var Sender_Message_Welcome = MessageResource.Create(
                                            body: "**Você foi adicionado ao bate-papo! Se você quiser deixar o bate-papo, apenas texto   '**EXIT**' sem as aspas.**",
                                            from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                            to: new Twilio.Types.PhoneNumber(Sender_Phone)
                                        );
                            User_Added_Successfully = true;
                        }
                        //NEW RUSSIAN USER
                        else if (Sender_Message_Edited == "RUSSIAN")
                        {
                            RUSSIAN_NumbersToMessage.Add(Sender_Phone);
                            //give user welcome info and codes in RUSSIAN
                            var Sender_Message_Welcome = MessageResource.Create(
                                            body: "**Вы были добавлены в чат! Если вы когда-нибудь хотите покинуть чат, просто текст '**EXIT**' без кавычек.**",
                                            from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                            to: new Twilio.Types.PhoneNumber(Sender_Phone)
                                        );
                            User_Added_Successfully = true;
                        }
                        //NEW SPANISH USER
                        else if (Sender_Message_Edited == "SPANISH")
                        {
                            SPANISH_NumbersToMessage.Add(Sender_Phone);
                            //give user welcome info and codes in SPANISH
                            var Sender_Message_Welcome = MessageResource.Create(
                                            body: "**Has sido añadido al chat! Si alguna vez quieres dejar el chat, sólo texto '**EXIT**' sin las comillas.**",
                                            from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                            to: new Twilio.Types.PhoneNumber(Sender_Phone)
                                        );
                            User_Added_Successfully = true;
                        }
                        //NEW GERMAN USER
                        else if (Sender_Message_Edited == "GERMAN")
                        {
                            GERMAN_NumbersToMessage.Add(Sender_Phone);
                            //give user welcome info and codes in GERMAN
                            var Sender_Message_Welcome = MessageResource.Create(
                                            body: "**Sie wurden in den Chat aufgenommen! Wenn Sie jemals den Chat verlassen wollen, dann nur Text '**EXIT**' ohne die Zitate.**",
                                            from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                            to: new Twilio.Types.PhoneNumber(Sender_Phone)
                                        );
                            User_Added_Successfully = true;
                        }
                        //NEW FRENCH USER
                        else if (Sender_Message_Edited == "FRENCH")
                        {
                            FRENCH_NumbersToMessage.Add(Sender_Phone);
                            //give user welcome info and codes in FRENCH
                            var Sender_Message_Welcome = MessageResource.Create(
                                            body: "**Vous avez été ajouté au chat!Si jamais vous voulez quitter le chat, juste le texte '**EXIT**' sans les guillemets.**",
                                            from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                            to: new Twilio.Types.PhoneNumber(Sender_Phone)
                                        );
                            User_Added_Successfully = true;
                        }
                        else //user did not send a valid code
                        {
                            //send introduction text 
                            var Sender_Message_Instructions = MessageResource.Create(
                                            body: "For English, text 'ENGLISH' without the quotes.\n\n" + "Para Português, texto 'PORTUGUESE' sem as aspas.\n\n" +
                                            "Для русского, текст 'RUSSIAN' без кавычек.\n\n" + "Para español, texto 'SPANISH' sin las cotizaciones.\n\n" +
                                            "Für Englisch, Text 'GERMAN' ohne die Zitate.\n\n" + "Pour l'anglais, le texte 'FRENCH' sans les citations. \n\n",
                                            from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                            to: new Twilio.Types.PhoneNumber(Sender_Phone)
                                        );
                            User_Added_Successfully = false;
                        }
                        if (User_Added_Successfully && Admin_Debug_Bool)
                        {
                            //let admin (Anastasiya) know that a new user has been added to the chat
                            var Admin_Message_User_Creation_confirmation = MessageResource.Create(
                                            body: "User with phone number " + Sender_Phone + " has been added to the DataBase",
                                            from: new Twilio.Types.PhoneNumber(Twilio_Phone),
                                            to: new Twilio.Types.PhoneNumber(Admin_Phone)
                                        );
                        }
                    }
                }
            } //end of foreach file statement


        } //end of infinite while


    }
   

    static string TranslateText(string Text_To_Translate, string To_Language)
    {
        string host = "https://api.cognitive.microsofttranslator.com";
        string route = "/translate?api-version=3.0&to=" + To_Language + "";
        string subscriptionKey = "XXXXXXXX";

        System.Object[] body = new System.Object[] { new { Text = Text_To_Translate } };
        var requestBody = JsonConvert.SerializeObject(body);

        using (var client = new HttpClient())
        using (var request = new HttpRequestMessage())
        {
            // Set the method to POST
            request.Method = HttpMethod.Post;

            // Construct the full URI
            request.RequestUri = new Uri(host + route);

            // Add the serialized JSON object to your request
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // Add the authorization header
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Send request, get response
            var response = client.SendAsync(request).Result;
            var jsonResponse = response.Content.ReadAsStringAsync().Result;

            string Full_JSON_Response = jsonResponse.ToString();
            string Partial_JSON_Response = getBetween(Full_JSON_Response, "\"text\":\"", "\",\"to\":\"" + To_Language + "\"}]}]");
            
            return Partial_JSON_Response;
        }
    }

    public static string getBetween(string strSource, string strStart, string strEnd)
    {
        int Start, End;
        if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        {
            Start = strSource.IndexOf(strStart, 0) + strStart.Length;
            End = strSource.IndexOf(strEnd, Start);
            return strSource.Substring(Start, End - Start);
        }
        else
        {
            return "";
        }
    }

}
