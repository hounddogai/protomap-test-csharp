namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered access badge ledger calculation.</summary>
public interface ITieredAccessBadgeLedgerStep
{
    string Describe();
    double Adjust(TieredAccessBadgeLedger item, double valueValue);
}

public sealed record CapTieredAccessBadgeLedgerAdjuster(double Cap) : ITieredAccessBadgeLedgerStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredAccessBadgeLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTieredAccessBadgeLedgerAdjuster(double Step) : ITieredAccessBadgeLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredAccessBadgeLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentTieredAccessBadgeLedgerAdjuster(Dictionary<string, double> Factors) : ITieredAccessBadgeLedgerStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(TieredAccessBadgeLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTieredAccessBadgeLedgerAdjuster(double Percent) : ITieredAccessBadgeLedgerStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredAccessBadgeLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredAccessBadgeLedgerAdjuster(double Minimum) : ITieredAccessBadgeLedgerStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredAccessBadgeLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAccessBadgeLedgerMarginOffsetAdjuster(double Weight) : ITieredAccessBadgeLedgerStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(TieredAccessBadgeLedger item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the tiered access badge ledger steps in order and keeps a trace of each.</summary>
public sealed class TieredAccessBadgeLedgerEngine(List<ITieredAccessBadgeLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredAccessBadgeLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredAccessBadgeLedgerEngine DefaultTieredAccessBadgeLedgerEngine()
    {
        return new TieredAccessBadgeLedgerEngine(
            new List<ITieredAccessBadgeLedgerStep>
            {
                new CapTieredAccessBadgeLedgerAdjuster(24234.0),
                new RoundingTieredAccessBadgeLedgerAdjuster(5.0),
                new DepartmentTieredAccessBadgeLedgerAdjuster(new Dictionary<string, double> { ["retail"] = 1.2 }),
                new PercentTieredAccessBadgeLedgerAdjuster(10.3),
                new MinimumTieredAccessBadgeLedgerAdjuster(8.4),
                new TieredAccessBadgeLedgerMarginOffsetAdjuster(1.32),
            },
            new List<string>(),
            4.0,
            24994.9);
    }

    public double Run(TieredAccessBadgeLedger item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredAccessBadgeLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
