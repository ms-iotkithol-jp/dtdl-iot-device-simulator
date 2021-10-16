using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIoTDeviceSimulator
{
    class TSITypeGenerator
    {

        public TSITypeGenerator(string id, string name)
        {
            TypeId = id;
            TypeName = name;
        }

        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public string GenerateTypeDef(List<TelemetryPrameterSpec>tpSpecs)
        {
            bool isFirst = true;
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                writer.WriteLine(GenerateTypeHead());
                foreach (var tps in tpSpecs)
                {
                    if (tps is TimestampTelemetryParameterSpec)
                    {
                        continue;
                    }
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        writer.WriteLine(",");
                    }
                    string kind = "numeric";
                    if (tps is EnumTelemetryParameterSpec)
                    {
                        kind = "categorical";
                    }
                    string typeName = tps.TypeName;
                    if (typeName.ToLower() == "real")
                    {
                        typeName = "Double";
                    }
                    writer.Write(GenerateTypeVariableDef(kind, typeName, tps.Name));
                }
                writer.WriteLine("");
                writer.WriteLine(GenerateTypeTail());
            }
            return sb.ToString();
        }

        string indent = "";

        string GenerateTypeHead()
        {
            var sb = new StringBuilder();
            indent = "";
            using (var writer = new StringWriter(sb))
            {
                writer.WriteLine($"{indent}"+"{");
                indent = "  ";
                writer.WriteLine($"{indent}\"put\": [");
                indent = "    ";
                writer.WriteLine($"{indent}"+"{");
                indent = "      ";
                writer.WriteLine($"{indent}\"id\": \"{TypeId}\"");
                writer.WriteLine($"{indent}\"name\": \"{TypeName}\"");
                writer.WriteLine($"{indent}\"description\": \"Generated - {DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")}\"");
                writer.WriteLine($"{indent}\"variables\": "+"{");
            }
            indent += "  ";
            return sb.ToString();
        }

        string GenerateTypeVariableDef(string kind, string typeName, string paramName)
        {
            var sb = new StringBuilder();
            string defTypeName = typeName.Substring(0, 1).ToUpper() + typeName.Substring(1).ToLower();
            string endMark = "";
            if (kind == "numeric")
            {
                endMark = ",";
            }
            using (var writer = new StringWriter(sb))
            {
                writer.WriteLine($"{indent}\"{paramName}\":" + " {");
                writer.WriteLine($"{indent}  \"kind\": \"{kind}\",");
                writer.WriteLine($"{indent}  \"value\":" + " {");
                writer.WriteLine($"{indent}    \"tsx\": \"$envent.[{paramName}].{defTypeName}\"");
                writer.WriteLine($"{indent}  "+ "}" + endMark);
                if (kind == "numeric")
                {
                    writer.WriteLine($"{indent}  \"aggregation\": " + "{");
                    writer.WriteLine($"{indent}    \"tsx\": \"avg($value)\"");
                    writer.WriteLine($"{indent}  "+ "}");
                }
                writer.Write($"{indent}" + "}");
            }
            return sb.ToString();
        }

        string GenerateTypeTail()
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                writer.WriteLine("    }");
                writer.WriteLine("  ]");
                writer.WriteLine("}");
            }
            return sb.ToString();
        }
    }
}
