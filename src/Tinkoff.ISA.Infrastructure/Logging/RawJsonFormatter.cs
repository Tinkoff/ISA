using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Tinkoff.ISA.Infrastructure.Logging
{
    public class RawJsonFormatter : ITextFormatter
    {
        private static readonly JsonValueFormatter ValueFormatter = new JsonValueFormatter();
        
        public void Format(LogEvent logEvent, TextWriter output)
        {
            FormatContent(logEvent, output);
            output.WriteLine();
        }

        private static void FormatContent(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.Write("{\"Timestamp\":\"");
            output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));
            output.Write("\",\"Level\":\"");
            output.Write(logEvent.Level);
            
            output.Write("\",\"Message\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            JsonValueFormatter.WriteQuotedJsonString(message, output);
            
            if (logEvent.Exception != null)
            {
                output.Write(",\"Exception\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            if (logEvent.Properties.Count != 0)
                WriteProperties(logEvent.Properties, output);

            output.Write('}');
        }

        private static void WriteProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            output.Write(",\"Properties\":{");

            var precedingDelimiter = "";
            foreach (var (key, value) in properties)
            {
                output.Write(precedingDelimiter);
                precedingDelimiter = ",";

                JsonValueFormatter.WriteQuotedJsonString(key, output);
                output.Write(':');
                ValueFormatter.Format(value, output);
            }

            output.Write('}');
        }
    }
}