using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIoTDeviceSimulator
{
    class TSNodeRepository : TSNode
    {
        Dictionary<string, TelemetryParameter> telemetryParameters = new Dictionary<string, TelemetryParameter>();
        public Dictionary<string, TelemetryParameter> TelemetryParameters { get { return telemetryParameters; } }

        public void Add(TelemetryPrameterSpec tpSpec)
        {
            TelemetryParameter tp = null;
            if (tpSpec is DeltaPropTelemetryParameterSpec)
            {
                tp = this.Add((DeltaPropTelemetryParameterSpec)tpSpec);
            }
            else if (tpSpec is CircularTelemetryParameterSpec)
            {
                tp = this.Add((CircularTelemetryParameterSpec)tpSpec);
            }
            else if (tpSpec is DiscreteTelemetryParameterSpec)
            {
                tp = this.Add((DiscreteTelemetryParameterSpec)tpSpec);
            }
            else if (tpSpec is EnumTelemetryParameterSpec)
            {
                tp = this.Add((EnumTelemetryParameterSpec)tpSpec);
            }
            else if (tpSpec is TimestampTelemetryParameterSpec)
            {
                tp = this.Add((TimestampTelemetryParameterSpec)tpSpec);
            }
            if (tp != null)
            {
                this.AddToNodeTree(tpSpec.Name, tp);
            }
        }
#if false
        private P Add<S,P>(S tpSpec) where S:TelemetryPrameterSpec where P:TelemetryParameter,new()
        {
            var tp = new P();
            tp.Spec(tpSpec);
            telemetryParameters.Add(tpSpec.Name, tp);
            return tp;
        }
#endif

        private DeltaPropTelemetryParameter Add(DeltaPropTelemetryParameterSpec tpSpec)
        {
            var tp = new DeltaPropTelemetryParameter(tpSpec);
            telemetryParameters.Add(tpSpec.Name, tp);
            return tp;
        }
        private CircularTelemetryParameter Add(CircularTelemetryParameterSpec tpSpec)
        {
            var tp = new CircularTelemetryParameter(tpSpec);
            telemetryParameters.Add(tpSpec.Name, tp);
            return tp;
        }
        private DiscreteTelemetryParameter Add(DiscreteTelemetryParameterSpec tpSpec)
        {
            var tp = new DiscreteTelemetryParameter(tpSpec);
            telemetryParameters.Add(tpSpec.Name, tp);
            return tp;
        }
        private EnumTelemetryParameter Add(EnumTelemetryParameterSpec tpSpec)
        {
            var tp = new EnumTelemetryParameter(tpSpec);
            telemetryParameters.Add(tpSpec.Name, tp);
            return tp;
        }
        private TimestampTelemetryParameter Add(TimestampTelemetryParameterSpec tpSpec)
        {
            var tp = new TimestampTelemetryParameter(tpSpec);
            telemetryParameters.Add(tpSpec.Name, tp);
            return tp;
        }

        private void AddToNodeTree(string name, TelemetryParameter tp)
        {
            var nameFrags = name.Split(new char[] { '.' });
            if (nameFrags.Length == 1)
            {
                children.Add(new TSNode() { Name = nameFrags[0], Parameter = tp });
            }
            else
            {
                var q = children.Where(n => n.Name == nameFrags[0]);
                if (q.Count() > 0)
                {
                    q.First().AddChild(name.Substring(name.IndexOf(".") + 1), tp);
                }
                else
                {
                    var newNode = new TSNode() { Name = nameFrags[0] };
                    children.Add(newNode);
                    newNode.AddChild(name.Substring(name.IndexOf(".") + 1), tp);
                }
            }
        }

        public void Update()
        {
            foreach (var key in telemetryParameters.Keys)
            {
                telemetryParameters[key].Update();
            }
        }

        public new string GetMessageJSON()
        {
            var sb = new StringBuilder("{");
            foreach (var n in children)
            {
                if (sb.ToString() != "{")
                {
                    sb.Append(",");
                }
                sb.Append(n.GetMessageJSON());
            }
            sb.Append("}");
            return sb.ToString();
        }

    }

    class TSNode
    {
        protected List<TSNode> children = new List<TSNode>();
        public string Name { get; set; }
        public TelemetryParameter Parameter { get; set; }
        public List<TSNode> Children { get { return children; } }

        public void AddChild(string name, TelemetryParameter p)
        {
            var nameFrags = name.Split(new char[] { '.' });
            if (nameFrags.Length == 1)
            {
                var q = children.Where(c => c.Name == name);
                if (q.Count() > 0)
                {
                    throw new ArgumentOutOfRangeException("telemetry","telemetry definition is mistaken!");
                }
                children.Add(new TSNode() { Name = name, Parameter = p });
            }
            else
            {
                if (nameFrags[0] == Name)
                {
                    TSNode child = null;
                    var q = children.Where(c => c.Name == nameFrags[1]);
                    if (q.Count() > 0)
                    {
                        child = q.First();
                    }
                    else
                    {
                        child = new TSNode() { Name = nameFrags[1] };
                    }
                    child.AddChild(name.Substring(name.IndexOf(".")+1), p);
                }
            }
        }

        public string GetMessageJSON()
        {
            var sb = new StringBuilder();
            if (this.Parameter != null)
            {
                return this.Parameter.GetMessageJSON();
            }
            else
            {
                sb.Append($"\"{this.Name}\":" + "{");
                var cb = new StringBuilder();
                foreach (var c in children)
                {
                    if (cb.ToString() != "")
                    {
                        cb.Append(",");
                    }
                    cb.Append(c.GetMessageJSON());
                }
                sb.Append(cb.ToString());
                sb.Append("}");
            }
            return sb.ToString();
        }
    }


}
