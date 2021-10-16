using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIoTDeviceSimulator
{
    public abstract class TelemetryPrameterSpec
    {
        public enum ParameterKind
        {
            DeltaProp,
            Circular,
            Discrete,
            Enumeration,
            Timestamp
        }
        public string Name { get; set; }
        protected ParameterKind Kind { get; set; }
        public string TypeName { get; set; }


    }

    public abstract class NumericTelemetryParameterSpec : TelemetryPrameterSpec
    {
        protected NumericTelemetryParameterSpec()
        {
        }

        public double Initial { get; set; }

        public double Noise { get; set; }

    }

    public class DeltaPropTelemetryParameterSpec: NumericTelemetryParameterSpec
    {
        public DeltaPropTelemetryParameterSpec():base()
        {
            Kind = ParameterKind.DeltaProp;
            TypeName = "real";
        }

        public double Target { get; set; }
        public double Coef { get; set; }

    }

    public class CircularTelemetryParameterSpec : NumericTelemetryParameterSpec
    {
        public CircularTelemetryParameterSpec():base()
        {
            Kind = ParameterKind.Circular;
            TypeName = "real";
        }

        public double Radius { get; set; }
        public double Phase { get; set; }
        public double Frequency { get; set; }

    }

    public class DiscreteTelemetryParameterSpec : NumericTelemetryParameterSpec
    {
        public DiscreteTelemetryParameterSpec()
        {
            Kind = ParameterKind.Discrete;
            Noise = 0;
            TypeName = "integer";
        }

    }

    public class EnumTelemetryParameterSpec : TelemetryPrameterSpec
    {
        public EnumTelemetryParameterSpec()
        {
            Kind = ParameterKind.Enumeration;
        }

        public List<string> Values = new List<string>();

        public string Initial { get; set; }
    }

    class TimestampTelemetryParameterSpec: TelemetryPrameterSpec
    {
        public TimestampTelemetryParameterSpec()
        {
            Kind = ParameterKind.Timestamp;
            TypeName = "datetime";
        }
    }
}
