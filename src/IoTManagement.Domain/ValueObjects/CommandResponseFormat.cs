using System;
using System.Text.RegularExpressions;

namespace IoTManagement.Domain.ValueObjects
{
    public class CommandResponseFormat
    {
        public string FormatPattern { get; }

        public CommandResponseFormat(string formatPattern)
        {
            // An empty or null format pattern could mean "return raw" or "no formatting".
            // Forcing it to be non-empty might be too restrictive.
            // For now, let's allow it to be potentially simple.
            // if (string.IsNullOrWhiteSpace(formatPattern))
            // {
            //     throw new ArgumentException("Format pattern cannot be null or empty", nameof(formatPattern));
            // }
            FormatPattern = formatPattern ?? string.Empty;
        }

        public string FormatResult(string rawResult)
        {
            if (string.IsNullOrEmpty(rawResult))
            {
                return string.Empty;
            }

            // Remove trailing \r if present, as it's a Telnet terminator not part of data.
            if (rawResult.EndsWith("\r"))
            {
                rawResult = rawResult.Substring(0, rawResult.Length - 1);
            }

            if (string.IsNullOrWhiteSpace(FormatPattern) || FormatPattern.ToLower() == "raw")
            {
                return rawResult; // No formatting, return raw
            }

            try
            {
                // Example: FormatPattern = "Value is {0}" or "Temp: {Temperature}, Humidity: {Humidity}"
                // This is a simplified example. Real-world parsing might need more robust logic
                // based on whether FormatPattern is a regex, a JSON template, simple placeholder, etc.

                // If FormatPattern looks like a C# string format placeholder
                if (Regex.IsMatch(FormatPattern, @"\{\d+\}"))
                {
                    // Assuming rawResult is a single value or space-separated values
                    var values = rawResult.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    return string.Format(FormatPattern, values);
                }

                // If FormatPattern looks like named placeholders (e.g., for JSON-like structures)
                // This is a very basic example. For complex JSON, a proper JSON parser/serializer should be used.
                if (FormatPattern.Contains("{") && FormatPattern.Contains("}"))
                {
                    string formattedResult = FormatPattern;
                    // Example: rawResult = "25.5 60.1", FormatPattern = "Temperature: {val1}, Humidity: {val2}"
                    // This requires knowing how to map rawResult parts to placeholders.
                    // Let's assume a simple positional mapping for now if placeholders are generic like {0}, {1} or
                    // if a more complex regex is used on rawResult to extract named groups.

                    // This part is highly dependent on the actual complexity of FormatPattern
                    // and the structure of rawResult. The current example is naive.
                    var regex = new Regex(@"\{(\w+)\}"); // Matches {word}
                    var matches = regex.Matches(FormatPattern);
                    var values = rawResult.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (matches.Count > 0 && values.Length > 0)
                    {
                        // A more robust solution would involve parsing rawResult based on some delimiter
                        // or regex defined by the command's metadata, then mapping to named placeholders.
                        // For this example, let's just replace the first placeholder with the first value if names match loosely.
                        // This is insufficient for robust general-purpose formatting.
                        for (int i = 0; i < matches.Count && i < values.Length; i++)
                        {
                            // This replacement logic needs to be smarter, e.g., if FormatPattern is "Key1: {Data1} Key2: {Data2}"
                            // and rawResult is "value_for_Data1 value_for_Data2".
                            // The placeholder name (matches[i].Groups[1].Value) should map to an index in 'values'.
                            // This requires a convention.
                            formattedResult = formattedResult.Replace(matches[i].Value, values[i]);
                        }
                        return formattedResult;
                    }
                }

                // If no specific formatting rule matched, or for other types of patterns.
                // Fallback: return raw result or apply a default transformation if defined.
                return rawResult; // Default to raw if pattern is not understood by this simple logic
            }
            catch (Exception ex)
            {
                // Log the exception
                return $"Error formatting result: {ex.Message}. Raw result: {rawResult}";
            }
        }
    }
}