using System.Xml;
using System.Net;
using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TFS_Test_API
{
    class Program
    {
        public static void Main(string[] args)
        {
            Uri collectionUri = new Uri("http://localhost:8080/tfs/DefaultCollection/");
            NetworkCredential credential = new NetworkCredential("LOGIN", "PASSWORD");
            TfsTeamProjectCollection teamProjectCollection = new TfsTeamProjectCollection(collectionUri, credential);
            teamProjectCollection.EnsureAuthenticated();
            WorkItemStore workItemStore = teamProjectCollection.GetService<WorkItemStore>();
            WorkItemCollection workItemCollection = workItemStore.Query("Select * from WorkItems order by ID");
            var result = new List<IEnumerable<string>>();
            foreach (WorkItem item in workItemCollection)
            {
                result.Add(ScanFieldsForErrors(item));
                Console.WriteLine("item ID:" + item.Id + " Title: " + item.Title);
            }
            foreach(var arr in result)
            {
                foreach(var item in arr)
                {
                    Console.WriteLine(item);
                }
            }
        }
        public static IEnumerable<string> ScanFieldsForErrors(WorkItem workItem)
        {
            var fieldXMLErrors = new List<string>();
           
            foreach (Field field in workItem.Fields)
            {
                object value = field.Value ?? String.Empty;
                try
                {
                    string result = XmlConvert.VerifyXmlChars(value.ToString());
                    if (result == null)
                    {
                        fieldXMLErrors.Add($"Field error. WorkItem ID = {workItem.Id}, Field Name = {field.Name}, Field ReferenceName = {field.ReferenceName}, Message = invalid chars detected, XmlConvert.VerifyXmlChars return null");
                    }
                }
                catch (Exception e)
                {
                    fieldXMLErrors.Add($"Field error. WorkItem ID = {workItem.Id}, Field Name = {field.Name}, Field ReferenceName = {field.ReferenceName}, Message = {e.Message}, Line Position = {(e as XmlException).LinePosition}");
                }
            }
            foreach (Revision rev in workItem.Revisions)
            {
                if (rev.Index != 32)
                    continue;
                foreach (Field field in rev.Fields)
                {
                    object value = field.Value ?? String.Empty;
                    try
                    {
                        string result = XmlConvert.VerifyXmlChars(value.ToString());
                        if (result == null)
                        {
                            fieldXMLErrors.Add($"Revision error. WorkItem ID = {workItem.Id}, Revision Index = {rev.Index}, Field Name = {field.Name}, Field ReferenceName = {field.ReferenceName}, Message = invalid chars detected, XmlConvert.VerifyXmlChars return null");
                        }
                    }
                    catch (Exception e)
                    {
                        fieldXMLErrors.Add($"Revision error. WorkItem ID = {workItem.Id}, Revision Index = {rev.Index}, Field Name = {field.Name}, Field ReferenceName = {field.ReferenceName}, Message = {e.Message}, Line Position = {(e as XmlException).LinePosition}");
                    }
                }
            }
            return fieldXMLErrors;
        }

    }
}