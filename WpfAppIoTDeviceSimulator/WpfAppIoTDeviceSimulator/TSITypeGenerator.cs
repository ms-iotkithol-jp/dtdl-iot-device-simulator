using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIoTDeviceSimulator
{
    class IndentWriter:IDisposable
    {
        StringWriter writer;
        string u = "";
        public IndentWriter(StringBuilder sb, int spaces = 2)
        {
            writer = new StringWriter(sb);
            for (int i = 0; i < spaces; i++)
            {
                u += " ";
            }
        }
        string indent = "";
        List<string> indents = new List<string>();
        public void inc()
        {
            if (!string.IsNullOrEmpty(indent))
            {
                indents.Insert(0, indent);
            }
            indent += u;
        }
        public void dec()
        {
            if (indents.Count > 0)
            {
                indent = indents[0];
                indents.RemoveAt(0);
            } else
            {
                indent = "";
            }
        }

        public void UpdateIndent(string s, bool pre)
        {
            var l = s.Trim();
            if (l.EndsWith(","))
            {
                l = l.Substring(0, l.Length - 1);
            }
            if (pre)
            {
                if (l.EndsWith("}") || l.EndsWith("]"))
                {
                    dec();
                }

            }
            else
            {
                if (l.EndsWith("{") || l.EndsWith("["))
                {
                    inc();
                }
            }
        }

        public void Write(string s, bool addIndent =true)
        {
            UpdateIndent(s, true);
            string line = s;
            if (addIndent)
            {
                line = indent + line;
            }
            writer.Write(line);
            UpdateIndent(s, false);
        }
        public void WriteLine(string s, bool addIndent=true)
        {
            UpdateIndent(s, true);
            string line = s;
            if (addIndent)
            {
                line = indent + line;
            }
            writer.WriteLine(line);
            UpdateIndent(s, false);
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }

    abstract class TSIGenerator
    {
        protected void GenerateHead(IndentWriter writer)
        {
            writer.WriteLine("{");
            writer.WriteLine("\"put\": [");
        }

        protected void GenerateTail(IndentWriter writer)
        {
            writer.WriteLine("]");
            writer.WriteLine("}");
        }
    }

    class TSITypeGenerator : TSIGenerator
    {

        public TSITypeGenerator(string id, string name, List<TelemetryPrameterSpec> tpSpecs)
        {
            TypeId = id;
            TypeName = name;
            TPSpecs = tpSpecs;
        }

        public string TypeId { get; set; }
        public string TypeName { get; set; }
        List<TelemetryPrameterSpec> TPSpecs { get; set; }
        public string GenerateTypeDef()
        {
            bool isFirst = true;
            var sb = new StringBuilder();
            using (var writer = new IndentWriter(sb))
            {
                GenerateHead(writer);

                writer.WriteLine("{");
                writer.WriteLine($"\"id\": \"{TypeId}\",");
                writer.WriteLine($"\"name\": \"{TypeName}\",");
                writer.WriteLine($"\"description\": \"Generated - {DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss")}\",");
                writer.WriteLine($"\"variables\": " + "{");
                foreach (var tps in TPSpecs)
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
                        writer.WriteLine(",",false);
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
                    GenerateTypeVariableDef(writer, kind, typeName, tps.Name);
                }
                writer.WriteLine("", false);
                writer.WriteLine("}");
                writer.WriteLine("}");

                GenerateTail(writer);
            }
            return sb.ToString();
        }

        void GenerateTypeVariableDef(IndentWriter writer, string kind, string typeName, string paramName)
        {
            string defTypeName = typeName.Substring(0, 1).ToUpper() + typeName.Substring(1).ToLower();
            string endMark = "";
            if (kind == "numeric")
            {
                endMark = ",";
            }
            writer.WriteLine($"\"{paramName}\":" + " {");
            writer.WriteLine($"\"kind\": \"{kind}\",");
            writer.WriteLine($"\"value\":" + " {");
            writer.WriteLine($"\"tsx\": \"$event.[{paramName}].{defTypeName}\"");
            writer.WriteLine("}" + endMark);
            if (kind == "numeric")
            {
                writer.WriteLine("\"aggregation\": {");
                writer.WriteLine("\"tsx\": \"avg($value)\"");
                writer.WriteLine("}");
            }
            writer.Write("}");
        }
    }

    class TSIHierachiesGenerator : TSIGenerator
    {
        public TSIHierachiesGenerator(string id, string name, string[] instances)
        {
            hierachiesId = id;
            hierachiesName = name;
            hierachiesInstances = instances;
        }
        string hierachiesId;
        string hierachiesName;
        string[] hierachiesInstances;

        public string Generate()
        {
            var sb = new StringBuilder();
            using (var writer = new IndentWriter(sb))
            {
                GenerateHead(writer);
                writer.WriteLine("{");
                writer.WriteLine($"\"id\": \"{hierachiesId}\",");
                writer.WriteLine($"\"name\": \"{hierachiesName}\",");
                writer.WriteLine("\"source\": {");
                writer.WriteLine("\"instanceFieldNames\": [");
                bool isFirst = true;
                foreach(var instance in hierachiesInstances)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        writer.WriteLine(",", false);
                    }
                    writer.Write($"\"{instance}\"");
                }
                writer.WriteLine("",false  );
                writer.WriteLine("]");
                writer.WriteLine("}");
                writer.WriteLine("}");

                GenerateTail(writer);
            }
            return sb.ToString();
        }
    }

    class TSIInstanceGenerator : TSIGenerator
    {
        public string TimeSeriesId { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public string HierarchyId { get; set; }
        public Dictionary<string,string> HierarchyInstances { get; set; }
        public TSIInstanceGenerator(string tsId,string name, string typeId, string hierarchyId, Dictionary<string,string> hierarchyInstances)
        {
            TimeSeriesId = tsId;
            Name = name;
            TypeId = typeId;
            HierarchyId = hierarchyId;
            HierarchyInstances = hierarchyInstances;
        }

        public string Generate()
        {
            var sb = new StringBuilder();
            using (var writer = new IndentWriter(sb))
            {
                GenerateHead(writer);

                writer.WriteLine("{");
                writer.WriteLine("\"timeSeriesId\": [");
                writer.WriteLine($"\"{TimeSeriesId}\"");
                writer.WriteLine($"],");
                writer.WriteLine($"\"typeId\": \"{TypeId}\",");
                writer.WriteLine($"\"name\": \"{Name}\",");
                writer.WriteLine("\"description\": \"Generated\",");
                writer.WriteLine("\"hierarchyIds\": [");
                writer.WriteLine($"\"{HierarchyId}\"");
                writer.WriteLine("],");
                writer.WriteLine("\"instanceFields\": " + "{");
                bool isFirst = true;
                foreach(var key in HierarchyInstances.Keys)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        writer.WriteLine(",", false);
                    }
                    writer.Write($"\"{key}\": \"{HierarchyInstances[key]}\"");
                }
                writer.WriteLine("",false);
                writer.WriteLine("}");
                writer.WriteLine("}");

                GenerateTail(writer);
            }
            return sb.ToString();
        }
    }

}
