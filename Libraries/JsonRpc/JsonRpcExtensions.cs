using System.Collections.Generic;
using System.Text;

namespace SFAuction.JsonRpc
{
    internal static class JsonRpcExtensions {
        // JSON ECMA Specification (section 9): http://www.ecma-international.org/publications/files/ECMA-ST/ECMA-404.pdf
        public static string JsonEncode(this string stringValue) {
            var sb = new StringBuilder();
            for (var c = 0; c < stringValue.Length; c++) {
                // Escape special characters
                if (CEscapeChars.IndexOf(stringValue[c]) >= 0) sb.Append("\\");
                sb.Append(stringValue[c]);
            }
            return sb.ToString();
        }
        private const string CEscapeChars = "\"\\/\b\f\n\r\t";
        public static string JsonDecode(this string jsonString) {
            var sb = new StringBuilder();
            for (var c = 0; c < jsonString.Length; c++) {
                var ch = jsonString[c];
                if (ch == '\\' && (c < jsonString.Length - 2) && CEscapeChars.IndexOf(jsonString[c + 1]) >= 0) continue;   // Skip the '\\'
                sb.Append(ch);
            }
            return sb.ToString();
        }

        internal static StringBuilder AppendParameters(this StringBuilder json, IEnumerable<KeyValuePair<string, object>> parameters) {
            if (parameters != null) {
                json.Append(", \"params\": {");
                var firstParam = true;
                foreach (var p in parameters) {
                    if (!firstParam) json.Append(", "); else firstParam = false;
                    json.Append($"\"{p.Key}\": ").AppendJson(p.Value);
                }
                json.Append("}"); // Close the parameters OBJECT
            }
            return json;
        }
        internal static StringBuilder AppendParameters(this StringBuilder json, IEnumerable<object> parameters) {
            if (parameters == null)
                return json;
            json.Append(", \"params\": [");
            var firstParam = true;
            foreach (var p in parameters) {
                if (!firstParam) json.Append(", "); else firstParam = false;
                json.AppendJson(p);
            }
            json.Append("]"); // Close the parameters ARRAY
            return json;
        }
        internal static StringBuilder AppendId(this StringBuilder json, JsonRpcMessageId id) {
            if (id.IsString) {
                if (id.String == null) return json;
                return json.Append($", \"id\": \"{id.String}\"");
            }
            return json.Append($", \"id\": {id.Number.ToString()}");
        }
#if true
        internal static StringBuilder AppendJson(this StringBuilder json, object o) {
            JsonRpc.JsonRpcMessage.Serializer.Serialize(o, json); return json;
        }
#endif
    }
}