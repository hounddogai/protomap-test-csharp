namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional leave request baseline calculation.</summary>
public interface IRegionalLeaveRequestBaselineModifier
{
    string Describe();
    double Adjust(RegionalLeaveRequestBaseline item, double valueValue);
}

public sealed record FlaggedRegionalLeaveRequestBaselineAdjuster(Registry Flags, string Flag, double Percent) : IRegionalLeaveRequestBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalLeaveRequestBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalLeaveRequestBaselineAdjuster(List<double> Bounds, List<double> Rates) : IRegionalLeaveRequestBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalLeaveRequestBaseline item, double valueValue)
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

public sealed record PercentRegionalLeaveRequestBaselineAdjuster(double Percent) : IRegionalLeaveRequestBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalLeaveRequestBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalLeaveRequestBaselineScoreOffsetAdjuster(double Weight) : IRegionalLeaveRequestBaselineModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(RegionalLeaveRequestBaseline item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record RoundingRegionalLeaveRequestBaselineAdjuster(double Step) : IRegionalLeaveRequestBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalLeaveRequestBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRegionalLeaveRequestBaselineAdjuster(double Minimum) : IRegionalLeaveRequestBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalLeaveRequestBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional leave request baseline steps in order and keeps a trace of each.</summary>
public sealed class RegionalLeaveRequestBaselineEngine(List<IRegionalLeaveRequestBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalLeaveRequestBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalLeaveRequestBaselineEngine DefaultRegionalLeaveRequestBaselineEngine(Registry flags)
    {
        return new RegionalLeaveRequestBaselineEngine(
            new List<IRegionalLeaveRequestBaselineModifier>
            {
                new FlaggedRegionalLeaveRequestBaselineAdjuster(
                    flags,
                    "regional-leave-request-baseline-manual",
                    -5.0),
                new TieredRegionalLeaveRequestBaselineAdjuster(
                    new List<double> { 3508.0, 5139.0, 8366.0 },
                    new List<double> { 1.05, 1.05, 0.9 }),
                new PercentRegionalLeaveRequestBaselineAdjuster(-19.2),
                new RegionalLeaveRequestBaselineScoreOffsetAdjuster(0.73),
                new RoundingRegionalLeaveRequestBaselineAdjuster(0.1),
                new MinimumRegionalLeaveRequestBaselineAdjuster(36.4),
            },
            new List<string>(),
            0.1,
            84864.3);
    }

    public double Evaluate(RegionalLeaveRequestBaseline item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalLeaveRequestBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
