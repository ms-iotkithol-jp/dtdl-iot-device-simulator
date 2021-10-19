using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIoTDeviceSimulator
{
    class DeviceDTDLParser
    {
        List<TelemetryPrameterSpec> telemetryPrameterSpecs = new List<TelemetryPrameterSpec>();
        private string timestampPropertyName = "";
        public string Id { get { return _id; } }
        string _id;
        string _name;
        public string Name { get { return _name; } }

        public List<TelemetryPrameterSpec> TelemetryParameterSpecs { get { return telemetryPrameterSpecs; } }
        public string TimestampPropertyName { get { return timestampPropertyName; } }

        public async Task ParseDTDLModel(List<string> modelJson)
        {
            var parser = new ModelParser();
            var parseResult = await parser.ParseAsync(modelJson);
            foreach (var k in parseResult.Keys)
            {
                var v = parseResult[k];
                if (v.EntityKind == DTEntityKind.Interface)
                {
                    var ifDef = (DTInterfaceInfo)v;
                    _id = ifDef.Id.ToString();
                    _name = ifDef.DisplayName["en"].ToString();
                    foreach (var ckey in ifDef.Contents.Keys)
                    {
                        var c = ifDef.Contents[ckey];
                        if (c.EntityKind == DTEntityKind.Telemetry)
                        {
                            var telemetryDef = (DTTelemetryInfo)c;
                            ParseTelemetryDef(telemetryDef, telemetryPrameterSpecs);
                        }
                        else if (c.EntityKind == DTEntityKind.Property)
                        {
                            var propertyDef = (DTPropertyInfo)c;
                        }
                        else if (c.EntityKind == DTEntityKind.Command)
                        {
                            var commandDef = (DTCommandInfo)c;
                        }
                    }
                }
            }
        }

        private void ParseTelemetryDef(DTTelemetryInfo telemetryDef, List<TelemetryPrameterSpec> tpRepo)
        {
            var schema = telemetryDef.Schema;
            if (schema.EntityKind == DTEntityKind.Object)
            {
                ParseTelemetryFieldObjectDef(telemetryDef.Name, (DTObjectInfo)schema, tpRepo);
            }
            else if (schema.EntityKind == DTEntityKind.Enum)
            {
                var eparam = ParseTelemetryFieldEnumDef((DTEnumInfo)schema);
                eparam.Name = telemetryDef.Name;
                tpRepo.Add(eparam);
            }
            else
            {
                bool isTimestamp;
                var tparam = ParseTelemetryValueDef(telemetryDef, schema, out isTimestamp);
                if (isTimestamp)
                {
                    timestampPropertyName = telemetryDef.Name;
                }
                tparam.Name = telemetryDef.Name;
                tpRepo.Add(tparam);
            }
        }
        private EnumTelemetryParameterSpec ParseTelemetryFieldEnumDef(DTEnumInfo enumDef)
        {
            EnumTelemetryParameterSpec etParam = new EnumTelemetryParameterSpec();
            if (enumDef.ValueSchema.EntityKind == DTEntityKind.String)
            {
                etParam.TypeName = "string";
            }
            else
            {
                etParam.TypeName = "integer";
            }
            foreach (var ev in enumDef.EnumValues)
            {
                var value = $"{ev.Name}:{ev.EnumValue}";
                etParam.Values.Add(value);
                if (ev.Description.ContainsKey("en") && !string.IsNullOrEmpty(ev.Description["en"]))
                {
                    var evds = ev.Description["en"].Split(new char[] { '=' });
                    if (evds[0] != "initial")
                    {
                        throw new ArgumentOutOfRangeException("enum description","should be starting by 'initial=true|false'");
                    }
                    bool initialValue = false;
                    if (!bool.TryParse(evds[1], out initialValue))
                    {
                        throw new ArgumentOutOfRangeException("enum description", "should be starting by 'initial=true|false'");
                    }
                    else
                    {
                        if (initialValue)
                        {
                            etParam.Initial = value;
                        }
                    }
                }
            }
            return etParam;
        }
        private TelemetryPrameterSpec ParseTelemetryValueDef(DTEntityInfo parameter, DTSchemaInfo schema, out bool isTimestamp)
        {
            isTimestamp = false;
            if (schema.EntityKind == DTEntityKind.Array || schema.EntityKind == DTEntityKind.Object || schema.EntityKind == DTEntityKind.Map || schema.EntityKind == DTEntityKind.Enum)
            {
                return null;
            }
            string descrip = GetDescription(parameter);
            TelemetryPrameterSpec tParam = null;
            var dFrags = descrip.Split(new char[] { ';' });
            var qDType = dFrags.Where(p => p.ToLower().StartsWith("kind"));
            if (qDType.Count() == 0)
            {
                throw new ArgumentOutOfRangeException("telemetry(except for enum) description should include 'kind=delta-prop|circular|discrete|timestamp'");
            }

            var dType = qDType.First();
            var dt = dType.Split(new char[] { '=' })[1].ToLower();
            if (dt == "delta-prop")
            {
                var dptParam = new DeltaPropTelemetryParameterSpec();
                foreach (var df in dFrags)
                {
                    var dds = df.Split(new char[] { '=' });
                    switch (dds[0])
                    {
                        case "initial":
                            dptParam.Initial = double.Parse(dds[1]);
                            break;
                        case "target":
                            dptParam.Target = double.Parse(dds[1]);
                            break;
                        case "coef":
                            dptParam.Coef = double.Parse(dds[1]);
                            break;
                        case "noise":
                            dptParam.Noise = double.Parse(dds[1]);
                            break;
                    }
                }
                tParam = dptParam;

            }
            else if (dt == "circular")
            {
                var ctParam = new CircularTelemetryParameterSpec();
                foreach (var df in dFrags)
                {
                    var dds = df.Split(new char[] { '=' });
                    switch (dds[0])
                    {
                        case "initial":
                            ctParam.Initial = double.Parse(dds[1]);
                            break;
                        case "radius":
                            ctParam.Radius = double.Parse(dds[1]);
                            break;
                        case "phase":
                            double phase = 0.0;
                            if (dds[1].IndexOf("pi") >= 0)
                            {
                                if (dds[1].IndexOf("*") > 0)
                                {
                                    var ddss = dds[1].Split(new char[] { '*' });
                                    if (ddss.Length != 2)
                                    {
                                        throw new ArgumentOutOfRangeException("description", "should be num*pi | pi/num | num");
                                    }
                                    if (ddss[0] == "pi")
                                    {
                                        throw new ArgumentOutOfRangeException("description", "should be num*pi | pi/num | num");
                                    }
                                    phase = Math.PI * double.Parse(ddss[0]);
                                }
                                else if (dds[1].IndexOf("/") > 0)
                                {
                                    var ddss = dds[1].Split(new char[] { '/' });
                                    if (ddss.Length != 2)
                                    {
                                        throw new ArgumentOutOfRangeException("description", "should be num*pi | pi/num | num");
                                    }
                                    if (ddss[1] == "pi" || ddss[1] == "0")
                                    {
                                        throw new ArgumentOutOfRangeException("description", "should be num*pi | pi/num | num");
                                    }
                                    phase = Math.PI / double.Parse(ddss[1]);
                                }
                                else if (dds[1] == "pi")
                                {
                                    phase = Math.PI;
                                }
                            }
                            else
                            {
                                phase = double.Parse(dds[1]);
                            }
                            ctParam.Phase = phase;
                            break;
                        case "frequency":
                            ctParam.Frequency = double.Parse(dds[1]);
                            break;
                        case "noise":
                            ctParam.Noise = double.Parse(dds[1]);
                            break;
                    }
                }
                tParam = ctParam;
            }
            else if (dt == "discrete")
            {
                var dtParam = new DiscreteTelemetryParameterSpec();
                foreach (var df in dFrags)
                {
                    var dds = df.Split(new char[] { '=' });
                    switch (dds[0])
                    {
                        case "initial":
                            dtParam.Initial = double.Parse(dds[1]);
                            break;
                        case "typename":
                            if (dds[1] != "integer")
                            {
                                dtParam.TypeName = dds[1];
                            }
                            break;
                        case "noise":
                            if (dds[1] != "integer")
                            {
                                dtParam.Noise = double.Parse(dds[1]);
                            }
                            break;
                    }
                }
                tParam = dtParam;
            }
            else if (dt == "timestamp")
            {
                isTimestamp = true;
                tParam = new TimestampTelemetryParameterSpec();
            }
            return tParam;
        }

        private string GetDescription(DTEntityInfo entity)
        {
            string descrip = "";
            if (entity.Description.ContainsKey("en"))
            {
                descrip = entity.Description["en"];
            }
            return descrip;
        }
        private void ParseTelemetryFieldObjectDef(string parentName, DTObjectInfo objectDef, List<TelemetryPrameterSpec> tpRepo)
        {
            foreach (var f in objectDef.Fields)
            {
                var fieldName = $"{parentName}.{f.Name}";
                if (f.Schema.EntityKind == DTEntityKind.Object)
                {
                    ParseTelemetryFieldObjectDef(fieldName, (DTObjectInfo)f.Schema, tpRepo);
                }
                else if (f.Schema.EntityKind == DTEntityKind.Enum)
                {
                    var eparam = ParseTelemetryFieldEnumDef((DTEnumInfo)f.Schema);
                    eparam.Name = fieldName;
                    tpRepo.Add(eparam);
                }
                else
                {
                    bool isTimestamp;
                    var tparam = ParseTelemetryValueDef(f, f.Schema, out isTimestamp);
                    if (isTimestamp)
                    {
                        timestampPropertyName = fieldName;
                    }
                    else
                    {
                        tparam.Name = fieldName;
                        tpRepo.Add(tparam);
                    }
                }
            }
        }
    }
}
