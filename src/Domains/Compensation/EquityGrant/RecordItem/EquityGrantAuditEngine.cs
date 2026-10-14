namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant audit calculation.</summary>
public interface IEquityGrantAuditFactor
{
    string Describe();
    double Adjust(EquityGrantAudit item, double valueValue);
}

public sealed record TieredEquityGrantAuditAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantAudit item, double valueValue)
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

public sealed record PercentEquityGrantAuditAdjuster(double Percent) : IEquityGrantAuditFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapEquityGrantAuditAdjuster(double Cap) : IEquityGrantAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceEquityGrantAuditAdjuster(Dictionary<string, double> Factors) : IEquityGrantAuditFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(EquityGrantAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedEquityGrantAuditAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEquityGrantAuditAdjuster(double Step) : IEquityGrantAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the equity grant audit steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantAuditEngine(List<IEquityGrantAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantAuditEngine DefaultEquityGrantAuditEngine(Registry flags)
    {
        return new EquityGrantAuditEngine(
            new List<IEquityGrantAuditFactor>
            {
                new TieredEquityGrantAuditAdjuster(
                    new List<double> { 3155.0, 8207.0, 8742.0, 9728.0 },
                    new List<double> { 1.1, 1.05, 0.97, 0.84 }),
                new PercentEquityGrantAuditAdjuster(13.9),
                new CapEquityGrantAuditAdjuster(43185.0),
                new SourceEquityGrantAuditAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.72, ["west"] = 1.18, ["central"] = 0.86 }),
                new FlaggedEquityGrantAuditAdjuster(flags, "equity-grant-audit-rolling", 5.8),
                new RoundingEquityGrantAuditAdjuster(5.0),
            },
            new List<string>(),
            1.3,
            67339.0);
    }

    public double Evaluate(EquityGrantAudit item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
