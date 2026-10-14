namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat milestone calculation.</summary>
public interface ILicenseSeatMilestoneFactor
{
    string Describe();
    double Adjust(LicenseSeatMilestone item, double valueValue);
}

public sealed record RoundingLicenseSeatMilestoneAdjuster(double Step) : ILicenseSeatMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLicenseSeatMilestoneAdjuster(double Minimum) : ILicenseSeatMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLicenseSeatMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLicenseSeatMilestoneAdjuster(double Percent) : ILicenseSeatMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentLicenseSeatMilestoneAdjuster(Dictionary<string, double> Factors) : ILicenseSeatMilestoneFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(LicenseSeatMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the license seat milestone steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatMilestoneEngine(List<ILicenseSeatMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatMilestoneEngine DefaultLicenseSeatMilestoneEngine(Registry flags)
    {
        return new LicenseSeatMilestoneEngine(
            new List<ILicenseSeatMilestoneFactor>
            {
                new RoundingLicenseSeatMilestoneAdjuster(0.25),
                new MinimumLicenseSeatMilestoneAdjuster(29.8),
                new FlaggedLicenseSeatMilestoneAdjuster(flags, "license-seat-milestone-strict", -7.9),
                new PercentLicenseSeatMilestoneAdjuster(8.7),
                new SegmentLicenseSeatMilestoneAdjuster(new Dictionary<string, double> { ["south"] = 1.33 }),
            },
            new List<string>(),
            1.1,
            2108.0);
    }

    public double Compute(LicenseSeatMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
