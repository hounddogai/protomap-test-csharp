namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary access badge summary calculation.</summary>
public interface IPrimaryAccessBadgeSummaryFactor
{
    string Describe();
    double Adjust(PrimaryAccessBadgeSummary item, double valueValue);
}

public sealed record PrimaryAccessBadgeSummaryWeightOffsetAdjuster(double Weight) : IPrimaryAccessBadgeSummaryFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(PrimaryAccessBadgeSummary item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record PercentPrimaryAccessBadgeSummaryAdjuster(double Percent) : IPrimaryAccessBadgeSummaryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAccessBadgeSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryAccessBadgeSummaryAdjuster(double Step) : IPrimaryAccessBadgeSummaryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryAccessBadgeSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPrimaryAccessBadgeSummaryAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryAccessBadgeSummaryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryAccessBadgeSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocalePrimaryAccessBadgeSummaryAdjuster(Dictionary<string, double> Factors) : IPrimaryAccessBadgeSummaryFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PrimaryAccessBadgeSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the primary access badge summary steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAccessBadgeSummaryEngine(List<IPrimaryAccessBadgeSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAccessBadgeSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAccessBadgeSummaryEngine DefaultPrimaryAccessBadgeSummaryEngine(Registry flags)
    {
        return new PrimaryAccessBadgeSummaryEngine(
            new List<IPrimaryAccessBadgeSummaryFactor>
            {
                new PrimaryAccessBadgeSummaryWeightOffsetAdjuster(0.4),
                new PercentPrimaryAccessBadgeSummaryAdjuster(-11.8),
                new RoundingPrimaryAccessBadgeSummaryAdjuster(1.0),
                new FlaggedPrimaryAccessBadgeSummaryAdjuster(flags, "primary-access-badge-summary-primary", 4.0),
                new LocalePrimaryAccessBadgeSummaryAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.95, ["north"] = 0.79, ["retail"] = 0.82 }),
            },
            new List<string>(),
            1.6,
            9959.5);
    }

    public double Run(PrimaryAccessBadgeSummary item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAccessBadgeSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
