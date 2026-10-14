namespace Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>One step of the certification schedule calculation.</summary>
public interface ICertificationScheduleAdjuster
{
    string Describe();
    double Adjust(CertificationSchedule item, double valueValue);
}

public sealed record CapCertificationScheduleAdjuster(double Cap) : ICertificationScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CertificationScheduleQuantityOffsetAdjuster(double Weight) : ICertificationScheduleAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(CertificationSchedule item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record PercentCertificationScheduleAdjuster(double Percent) : ICertificationScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the certification schedule steps in order and keeps a trace of each.</summary>
public sealed class CertificationScheduleEngine(List<ICertificationScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationScheduleEngine DefaultCertificationScheduleEngine()
    {
        return new CertificationScheduleEngine(
            new List<ICertificationScheduleAdjuster>
            {
                new CapCertificationScheduleAdjuster(19172.0),
                new CertificationScheduleQuantityOffsetAdjuster(1.56),
                new PercentCertificationScheduleAdjuster(-7.7),
            },
            new List<string>(),
            0.6,
            63041.2);
    }

    public double Run(CertificationSchedule item)
    {
        var current = item.NetTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CertificationSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
