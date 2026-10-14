namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit group calculation.</summary>
public interface IAuditGroupModifier
{
    string Describe();
    double Adjust(AuditGroup item, double valueValue);
}

public sealed record MinimumAuditGroupAdjuster(double Minimum) : IAuditGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAuditGroupAdjuster(double Percent) : IAuditGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AuditGroupRateOffsetAdjuster(double Weight) : IAuditGroupModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(AuditGroup item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record RoundingAuditGroupAdjuster(double Step) : IAuditGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAuditGroupAdjuster(List<double> Bounds, List<double> Rates) : IAuditGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditGroup item, double valueValue)
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

/// <summary>Runs the audit group steps in order and keeps a trace of each.</summary>
public sealed class AuditGroupEngine(List<IAuditGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditGroupEngine DefaultAuditGroupEngine()
    {
        return new AuditGroupEngine(
            new List<IAuditGroupModifier>
            {
                new MinimumAuditGroupAdjuster(2.5),
                new PercentAuditGroupAdjuster(-8.3),
                new AuditGroupRateOffsetAdjuster(0.21),
                new RoundingAuditGroupAdjuster(0.1),
                new TieredAuditGroupAdjuster(new List<double> { 2351.0, 7879.0 }, new List<double> { 0.88, 1.08 }),
            },
            new List<string>(),
            1.0,
            32029.4);
    }

    public double Compute(AuditGroup item)
    {
        var current = item.NetVolume();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AuditGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
