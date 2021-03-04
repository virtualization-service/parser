using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Parser.Model;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using Parser.Util;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Parser.Processors
{
    public class MessageExtractor
    {
        PublishMessage _publisher;
        ILogger<MessageExtractor> _logger;

        public MessageExtractor(PublishMessage publisher, ILogger<MessageExtractor> logger)
        {
            _logger=logger;
            _publisher = publisher;
        }

        public void ConsumeMessage(BasicDeliverEventArgs message, ConnectionFactory connectionFactory)
        {
            var body = message.Body;
            var extactedMessage = Encoding.UTF8.GetString(body);

            _logger.LogInformation($"Received message with routing key {message.RoutingKey} exchange :{message.Exchange} & Body:{extactedMessage}");

            var  dataObject =  JsonConvert.DeserializeObject<MessageDto>(extactedMessage);

            dataObject.operation = GetServiceIdentifier(dataObject);

            dataObject.request.formatted_data = AddHeadersToDictionary(ElementStructure(dataObject?.request?.raw_data), dataObject.request.headers) ;
            AppendQueryParams(dataObject.request.formatted_data, dataObject.service);
            //if(dataObject.response != null)  dataObject.response.formatted_data = AddHeadersToDictionary(ElementStructure(dataObject?.response?.raw_data), dataObject.response.headers);

            _publisher.Publish(JsonConvert.SerializeObject(dataObject), connectionFactory, message.Exchange, message.BasicProperties);
        }

        public Dictionary<string, string[]> AddHeadersToDictionary(Dictionary<string, string[]> flattened, 
                    Dictionary<string, string> headers)
        {
            if(headers == null) headers = new Dictionary<string, string>();

            foreach(var item in headers)
            {
                flattened.Add("headers_" + item.Key.ToLower(), new[] {item.Value});
            }
            return flattened;
        }

        public Dictionary<string, string[]> ElementStructure(string value)
        {
            XElement xmlData;
            if(value.TryParseXml(out xmlData))
            {
                return new FlattenedXmlStructure().GetList(xmlData);
            }
            JToken json;
            if(value.TryParseJson(out json))
            {
                return new FlattenedJsonStructure().GetList(json);
            }

            return new Dictionary<string, string[]>();
        }

        public string GetServiceIdentifier(MessageDto message)
        {
            var headerComponent = string.Empty;

            if(message?.protocol == "soap"){

                var action =  message?.soapaction;
                action = action.Replace("\"", "");

                if(!string.IsNullOrEmpty(action)) return message.service_component + "-" + action;

                else return message.service_component + "-" + GetElementName(message.request.raw_data);
            }

            if(message?.protocol == "rest"){

                return message.service_component +"-" + message?.method;
                
            }

            return message.service_component;
        }
    

    private static string GetElementName(string body)
    {
        try{
            var xmlResponse = XElement.Parse(body);
            return xmlResponse.Descendants().Count() > 2 
                ? xmlResponse.Descendants().ElementAt(2).Name.LocalName
                : xmlResponse.Descendants().ElementAt(0).Name.LocalName;
        }
        catch(Exception)
        {
        }
        return string.Empty;
    }

    public void AppendQueryParams(Dictionary<string,string[]> formattedData, Uri url)
    {
        
        if(formattedData == null) formattedData = new Dictionary<string, string[]>();
            var queries = HttpUtility.ParseQueryString(url.Query);
            foreach(var query in queries)
            {
                formattedData.Add("queryString_" + query.ToString(),new[] {queries.Get(query.ToString()) } );
            }
    }
    }
}