namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the audit forecast calculation.</summary>
public interface IAuditForecastFactor
{
    string Describe();
    double Adjust(AuditForecast item, double valueValue);
}

public sealed record PercentAuditForecastAdjuster(double Percent) : IAuditForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitAuditForecastAdjuster(Dictionary<string, double> Factors) : IAuditForecastFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AuditForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAuditForecastAdjuster(Registry Flags, string Flag, double Percent) : IAuditForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAuditForecastAdjuster(double Cap) : IAuditForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAuditForecastAdjuster(List<double> Bounds, List<double> Rates) : IAuditForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditForecast item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record MinimumAuditForecastAdjuster(double Minimum) : IAuditForecastFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditForecast item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the audit forecast steps in order and keeps a trace of each.</summary>
public sealed class AuditForecastEngine(List<IAuditForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditForecastEngine DefaultAuditForecastEngine(Registry flags)
    {
        return new AuditForecastEngine(
            new List<IAuditForecastFactor>
            {
                new PercentAuditForecastAdjuster(-4.5),
                new UnitAuditForecastAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.82, ["online"] = 0.93, ["north"] = 1.24 }),
                new FlaggedAuditForecastAdjuster(flags, "audit-forecast-weighted", -4.2),
                new CapAuditForecastAdjuster(4676.0),
                new TieredAuditForecastAdjuster(
                    new List<double> { 1967.0, 5229.0, 7962.0 },
                    new List<double> { 0.8, 0.89, 0.82 }),
                new MinimumAuditForecastAdjuster(12.7),
            },
            new List<string>(),
            3.1,
            45956.5);
    }

    public double Compute(AuditForecast item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AuditForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
