namespace Acme.Workforce.Compliance.Exception.Audit;

/// <summary>One step of the legacy exception quota calculation.</summary>
public interface ILegacyExceptionQuotaModifier
{
    string Describe();
    double Adjust(LegacyExceptionQuota item, double valueValue);
}

public sealed record PercentLegacyExceptionQuotaAdjuster(double Percent) : ILegacyExceptionQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyExceptionQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyExceptionQuotaAdjuster(double Cap) : ILegacyExceptionQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyExceptionQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLegacyExceptionQuotaAdjuster(double Minimum) : ILegacyExceptionQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyExceptionQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy exception quota steps in order and keeps a trace of each.</summary>
public sealed class LegacyExceptionQuotaEngine(List<ILegacyExceptionQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyExceptionQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyExceptionQuotaEngine DefaultLegacyExceptionQuotaEngine()
    {
        return new LegacyExceptionQuotaEngine(
            new List<ILegacyExceptionQuotaModifier>
            {
                new PercentLegacyExceptionQuotaAdjuster(19.9),
                new CapLegacyExceptionQuotaAdjuster(38180.0),
                new MinimumLegacyExceptionQuotaAdjuster(23.6),
            },
            new List<string>(),
            1.3,
            29897.4);
    }

    public double Run(LegacyExceptionQuota item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyExceptionQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
