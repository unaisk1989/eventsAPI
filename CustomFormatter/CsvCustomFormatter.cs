using EventsDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsDemo.CustomFormatter
{
    public class CsvCustomFormatter: TextOutputFormatter
    {
        public CsvCustomFormatter()
        {
            this.SupportedEncodings.Add(Encoding.UTF8);
            this.SupportedEncodings.Add(Encoding.Unicode);
            this.SupportedMediaTypes.Add("text/csv");
            this.SupportedMediaTypes.Add("application/csv");
        }

        protected override bool CanWriteType(Type type) //To check whether the given object is writable or not
        {
            if (typeof(EventInfo).IsAssignableFrom(type) || typeof(IEnumerable<EventInfo>).IsAssignableFrom(type))
            {
                return true;
            }
            return false;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();

            if (context.Object is EventInfo)
            {
                EventInfo ev = context.Object as EventInfo;
                buffer.AppendLine("Id,Ttitle,StartDate,EndDate,StartTime,EndTime,Speaker,Host,RegistrationUrl");
                buffer.AppendLine($"{ev.Id},{ev.Title},{ev.StartDate},{ev.EndDate},{ev.StartTime},{ev.EndTime},{ev.Speaker},{ev.Host},{ev.RegistrationUrl}");
            }
            else if (context.Object is IEnumerable<EventInfo>)
            {
                var edataList = context.Object as IEnumerable<EventInfo>;
                buffer.AppendLine("Id,Ttitle,StartDate,EndDate,StartTime,EndTime,Speaker,Host,RegistrationUrl");
                foreach (var ev in edataList)
                {
                    buffer.AppendLine($"{ev.Id},{ev.Title},{ev.StartDate},{ev.EndDate},{ev.StartTime},{ev.EndTime},{ev.Speaker},{ev.Host},{ev.RegistrationUrl}");
                }

            }
            await response.WriteAsync(buffer.ToString());
        }
    }
}
