namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling reorg summary calculation.</summary>
public interface IRollingReorgSummaryStep
{
    string Describe();
    double Adjust(RollingReorgSummary item, double valueValue);
}

public sealed record CountryRollingReorgSummaryAdjuster(Dictionary<string, double> Factors) : IRollingReorgSummaryStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RollingReorgSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRollingReorgSummaryAdjuster(double Minimum) : IRollingReorgSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingReorgSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRollingReorgSummaryAdjuster(double Percent) : IRollingReorgSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingReorgSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingReorgSummaryAdjuster(double Cap) : IRollingReorgSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingReorgSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingReorgSummaryAdjuster(Registry Flags, string Flag, double Percent) : IRollingReorgSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingReorgSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling reorg summary steps in order and keeps a trace of each.</summary>
public sealed class RollingReorgSummaryEngine(List<IRollingReorgSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingReorgSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingReorgSummaryEngine DefaultRollingReorgSummaryEngine(Registry flags)
    {
        return new RollingReorgSummaryEngine(
            new List<IRollingReorgSummaryStep>
            {
                new CountryRollingReorgSummaryAdjuster(new Dictionary<string, double> { ["north"] = 0.96 }),
                new MinimumRollingReorgSummaryAdjuster(25.5),
                new PercentRollingReorgSummaryAdjuster(-2.1),
                new CapRollingReorgSummaryAdjuster(32790.0),
                new FlaggedRollingReorgSummaryAdjuster(flags, "rolling-reorg-summary-scheduled", -1.2),
            },
            new List<string>(),
            0.6,
            34927.8);
    }

    public double Compute(RollingReorgSummary item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingReorgSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
