using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Parser.Model;
using Parser.Processors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Parser.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParserController : ControllerBase
    {
        private readonly ILogger<ParserController> _logger;

        public ParserController(ILogger<ParserController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("Success");
        }

        [HttpPost]
        public ActionResult POST([FromBody] MessageDto message, [FromServices] MessageExtractor extractor)
        {
            message.operation = extractor.GetServiceIdentifier(message);
            message.request.formatted_data =  extractor.AddHeadersToDictionary(extractor.ElementStructure(message?.request?.raw_data), message.request.headers);

            return Ok(message);
        }
    }
}
