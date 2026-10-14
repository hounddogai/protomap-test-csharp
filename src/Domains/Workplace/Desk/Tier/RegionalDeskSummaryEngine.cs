namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional desk summary calculation.</summary>
public interface IRegionalDeskSummaryStep
{
    string Describe();
    double Adjust(RegionalDeskSummary item, double valueValue);
}

public sealed record TieredRegionalDeskSummaryAdjuster(List<double> Bounds, List<double> Rates) : IRegionalDeskSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalDeskSummary item, double valueValue)
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

public sealed record PercentRegionalDeskSummaryAdjuster(double Percent) : IRegionalDeskSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalDeskSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalDeskSummaryAdjuster(double Cap) : IRegionalDeskSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalDeskSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleRegionalDeskSummaryAdjuster(Dictionary<string, double> Factors) : IRegionalDeskSummaryStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RegionalDeskSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRegionalDeskSummaryAdjuster(Registry Flags, string Flag, double Percent) : IRegionalDeskSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalDeskSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalDeskSummaryAdjuster(double Minimum) : IRegionalDeskSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalDeskSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional desk summary steps in order and keeps a trace of each.</summary>
public sealed class RegionalDeskSummaryEngine(List<IRegionalDeskSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalDeskSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalDeskSummaryEngine DefaultRegionalDeskSummaryEngine(Registry flags)
    {
        return new RegionalDeskSummaryEngine(
            new List<IRegionalDeskSummaryStep>
            {
                new TieredRegionalDeskSummaryAdjuster(
                    new List<double> { 2867.0, 6486.0, 7198.0, 8768.0 },
                    new List<double> { 1.17, 0.81, 0.88, 0.97 }),
                new PercentRegionalDeskSummaryAdjuster(-13.3),
                new CapRegionalDeskSummaryAdjuster(7656.0),
                new TitleRegionalDeskSummaryAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.9, ["retail"] = 0.91 }),
                new FlaggedRegionalDeskSummaryAdjuster(flags, "regional-desk-summary-strict", 2.1),
                new MinimumRegionalDeskSummaryAdjuster(29.8),
            },
            new List<string>(),
            0.8,
            34270.7);
    }

    public double Run(RegionalDeskSummary item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalDeskSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
