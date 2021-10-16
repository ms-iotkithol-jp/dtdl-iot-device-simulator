using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIoTDeviceSimulator
{
    interface TelemetryParameter
    {
        void Spec(TelemetryPrameterSpec tps);
        void Update();
        string GetMessageJSON();
        string GetTSITypeJSON();
        bool IsTimestamp();

        void Reset();
        void Store();
    }


    class TelemetryParameterUtility
    {
        public static string GetJSONName(string paramName)
        {
            var names = paramName.Split(new char[] { '.' });
            return $"{names[names.Length-1]}";
        }

    }
    public class DeltaPropTelemetryParameter: TelemetryParameter, INotifyPropertyChanged
    {
        Random random;
        DeltaPropTelemetryParameterSpec tpSpec;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string paramName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(paramName));
            }
        }

        public DeltaPropTelemetryParameter()
        {
            random = new Random(DateTime.Now.Millisecond);
        }
        public DeltaPropTelemetryParameter(DeltaPropTelemetryParameterSpec spec):this()
        {
            Spec(spec);
        }

        public void Spec(TelemetryPrameterSpec spec)
        {
            if (spec is not DeltaPropTelemetryParameterSpec)
            {
                throw new ArgumentOutOfRangeException("spec", "spec should be DeltaPropTelemetryParameterSpec");
            }
            tpSpec = (DeltaPropTelemetryParameterSpec)spec;
            Current = tpSpec.Initial;
            Target = tpSpec.Target;
            Coef = tpSpec.Coef;
            Noise = tpSpec.Noise;
        }

        double _current;
        double _target;
        double _coef;
        double _noise;
        double _initial;
        
        public string TPName
        {
            get { return tpSpec.Name; }
        }

        public double Current
        {
            get { return _current; }
            set
            {
                _current = value;
                OnPropertyChanged(nameof(Current));
            }
        }
        public double Target
        {
            get { return _target; }
            set
            {
                _target = value;
                OnPropertyChanged(nameof(Target));
            }
        }
        public double Coef
        {
            get { return _coef; }
            set
            {
                _coef = value;
                OnPropertyChanged(nameof(Coef));
            }
        }
        public double Noise
        {
            get { return _noise; }
            set
            {
                _noise = value;
                OnPropertyChanged(nameof(Noise));
            }
        }
        public double Initial
        {
            get { return _initial; }
            set
            {
                _initial = value;
                Current = value;
                OnPropertyChanged(nameof(Initial));
            }
        }

        public void Update()
        {
            lock (this)
            {
                double delta = (tpSpec.Target - Current) * tpSpec.Coef;
                Current += delta;
                Current += tpSpec.Noise * (random.NextDouble() - 0.5);
            }
        }

        public string GetMessageJSON()
        {
            return $"\"{TelemetryParameterUtility.GetJSONName(tpSpec.Name)}\":{Current}";
        }

        public bool IsTimestamp()
        {
            return false;
        }

        public void Reset()
        {
            Initial = tpSpec.Initial;
            Target = tpSpec.Initial;
            Coef = tpSpec.Coef;
            Noise = tpSpec.Noise;
        }

        public void Store()
        {
            lock (this)
            {
                tpSpec.Initial = Initial;
                tpSpec.Target = Target;
                tpSpec.Coef = Coef;
                tpSpec.Noise = Noise;
            }
        }

        public string GetTSITypeJSON()
        {
            throw new NotImplementedException();
        }
    }

    public class CircularTelemetryParameter:TelemetryParameter, INotifyPropertyChanged
    {
        Random random;
        CircularTelemetryParameterSpec tpSpec;
        public CircularTelemetryParameter()
        {
            random = new Random(DateTime.Now.Millisecond);
        }
        public CircularTelemetryParameter(CircularTelemetryParameterSpec spec):this()
        {
            Spec(spec);
        }
        public void Spec(TelemetryPrameterSpec spec)
        {
            if (spec is not CircularTelemetryParameterSpec)
            {
                throw new ArgumentOutOfRangeException("spec", "spec should be CircularTelemetryParameterSpec");
            }
            tpSpec = (CircularTelemetryParameterSpec)spec;
            Current = tpSpec.Initial;
            Initial = tpSpec.Initial;
            Radius = tpSpec.Radius;
            Phase = tpSpec.Phase;
            Frequency = tpSpec.Frequency;
            Noise = tpSpec.Noise;
        }

        long initialTick = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string paramName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(paramName));
            }
        }

        public string TPName { get { return tpSpec.Name; } }

        double _current;
        double _radius;
        double _phase;
        double _frequency;
        double _noise;
        double _initial;

        public double Current
        {
            get { return _current; }
            set
            {
                _current=value;
                OnPropertyChanged(nameof(Current));
            }
        }

        public double Initial
        {
            get { return _initial; }
            set
            {
                _initial = value;
                Current = value;
                OnPropertyChanged(nameof(Initial));
            }
        }
        public double Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                OnPropertyChanged(nameof(Radius));
            }
        }
        public double Phase
        {
            get { return _phase; }
            set
            {
                _phase = value;
                OnPropertyChanged(nameof(Phase));
            }
        }
        public double Frequency
        {
            get { return _frequency; }
            set
            {
                _frequency = value;
                OnPropertyChanged(nameof(Frequency));
            }
        }
        public  double Noise
        {
            get { return _noise; }
            set
            {
                _noise = value;
                OnPropertyChanged(nameof(Noise));
            }
        }

        public void Update()
        {
            lock (this)
            {
                if (initialTick < 0)
                {
                    initialTick = DateTime.Now.Ticks;
                }
                double currentTickMSec = (DateTime.Now.Ticks - initialTick) / 10000;
                double p = currentTickMSec % tpSpec.Frequency;
                p = 2 * Math.PI * p / tpSpec.Frequency + tpSpec.Phase;
                p %= 2 * Math.PI;
                Current = tpSpec.Radius * Math.Sin(p) + tpSpec.Initial;
                Current += tpSpec.Noise * (random.NextDouble() - 0.5);
            }
        }

        public string GetMessageJSON()
        {
            return $"\"{TelemetryParameterUtility.GetJSONName(tpSpec.Name)}\":{Current}";
        }

        public bool IsTimestamp()
        {
            return false;
        }

        public void Reset()
        {
            Initial = tpSpec.Initial;
            Radius = tpSpec.Radius;
            Phase = tpSpec.Phase;
            Frequency = tpSpec.Frequency;
            Noise = tpSpec.Noise;
        }

        public void Store()
        {
            lock (this)
            {
                tpSpec.Initial = Initial;
                tpSpec.Radius = Radius;
                tpSpec.Phase = Phase;
                tpSpec.Frequency = Frequency;
                tpSpec.Noise = Noise;
            }
        }

        public string GetTSITypeJSON()
        {
            throw new NotImplementedException();
        }
    }

    public class DiscreteTelemetryParameter : TelemetryParameter, INotifyPropertyChanged
    {
        DiscreteTelemetryParameterSpec tpSpec;
        Random random;

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string paramName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(paramName));
            }
        }
        public DiscreteTelemetryParameter()
        {
            random = new Random(DateTime.Now.Millisecond);
        }
        public DiscreteTelemetryParameter(DiscreteTelemetryParameterSpec spec):this()
        {
            tpSpec = spec;
            Current = tpSpec.Initial;
        }

        public void Spec(TelemetryPrameterSpec spec)
        {
            if (spec is not DiscreteTelemetryParameterSpec)
            {
                throw new ArgumentOutOfRangeException("spec", "spec should be DiscreteTelemetryParameterSpec");
            }
            tpSpec = (DiscreteTelemetryParameterSpec)spec;
            Current = tpSpec.Initial;
            Noise = tpSpec.Noise;
        }

        double _current;
        double _noise;

        public double Current
        {
            get { return _current; }
            set
            {
                _current = value;
                OnPropertyChanged(nameof(Current));
            }
        }
        public double Noise
        {
            get { return _noise; }
            set
            {
                _noise = value;
                OnPropertyChanged(nameof(Noise));
            }
        }

        public string TPName { get { return tpSpec.Name; } }
        public string TPTypeName { get { return tpSpec.TypeName; } }

        public void Update()
        {
            lock (this)
            {
                if (tpSpec.TypeName != "integer")
                {
                    Current = tpSpec.Initial + (0.5 - random.NextDouble()) * tpSpec.Noise;
                }
            }
        }

        public string GetMessageJSON()
        {
            string json = $"\"{TelemetryParameterUtility.GetJSONName(tpSpec.Name)}\":";
            if (tpSpec.TypeName == "integer")
            {
                json += $"{(int)Current}";
            }else
            {
                json += $"{Current}";
            }
            return json;
        }

        public bool IsTimestamp()
        {
            return false;
        }

        public void Reset()
        {
            Current = tpSpec.Initial;
            _noise = tpSpec.Noise;
        }

        public void Store()
        {
            lock (this)
            {
                tpSpec.Initial = Current;
                tpSpec.Noise = _noise;
            }
        }

        public string GetTSITypeJSON()
        {
            throw new NotImplementedException();
        }
    }

    public class EnumTelemetryParameter: TelemetryParameter, INotifyPropertyChanged
    {
        EnumTelemetryParameterSpec tpSpec;
        public EnumTelemetryParameter()
        {
        }
        public EnumTelemetryParameter(EnumTelemetryParameterSpec spec)
        {
            tpSpec = spec;
            Current = tpSpec.Initial;
        }
        public void Spec(TelemetryPrameterSpec spec)
        {
            if (spec is not EnumTelemetryParameterSpec)
            {
                throw new ArgumentOutOfRangeException("spec", "spec should be EnumTelemetryParameterSpec");
            }
            tpSpec = (EnumTelemetryParameterSpec)spec;
            Current = tpSpec.Initial;
        }

        public string TPName { get { return tpSpec.Name; } }
        public List<string> GetEnumValues()
        {
            return tpSpec.Values;
        }

        string _current;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string paramName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(paramName));
            }
        }

        public string Current
        {
            get { return _current; }
            set
            {
                _current = value;
                OnPropertyChanged(nameof(Current));
            }
        }
        public void Update()
        {
            ;
        }

        public string GetMessageJSON()
        {
            var value = Current.Split(new char[] { ':' })[1];
            if (tpSpec.TypeName == "string")
            {
                value = $"\"{value}\"";
            }
            return  $"\"{TelemetryParameterUtility.GetJSONName(tpSpec.Name)}\":{value}";
        }

        public bool IsTimestamp()
        {
            return false;
        }

        public void Reset()
        {
            Current = tpSpec.Initial ;
        }

        public void Store()
        {
            lock (this)
            {
                tpSpec.Initial = Current;
            }
        }

        public string GetTSITypeJSON()
        {
            throw new NotImplementedException();
        }
    }
    class TimestampTelemetryParameter:TelemetryParameter
    {
        TimestampTelemetryParameterSpec tpSpec;
        public TimestampTelemetryParameter()
        {
        }
        public TimestampTelemetryParameter(TimestampTelemetryParameterSpec spec)
        {
            tpSpec = spec;
        }
        public void Spec(TelemetryPrameterSpec spec)
        {
            if (spec is not TimestampTelemetryParameterSpec)
            {
                throw new ArgumentOutOfRangeException("spec", "spec should be TimestampTelemetryParameterSpec");
            }
            tpSpec = (TimestampTelemetryParameterSpec)spec;
        }
        
        public string GetMessageJSON()
        {
            return $"\"{tpSpec.Name}\":\"{Current.ToUniversalTime().ToString("yyyy/MM/ddTHH:mm:ss.fffZ")}\"";
        }
        DateTime Current { get; set; }
        public void Update()
        {
            Current = DateTime.Now;
        }

        public bool IsTimestamp()
        {
            return true;
        }

        public void Reset()
        {
        }

        public void Store()
        {
        }

        public string GetTSITypeJSON()
        {
            throw new NotImplementedException();
        }
    }
}
