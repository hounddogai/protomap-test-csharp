namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request digest calculation.</summary>
public interface ILeaveRequestDigestModifier
{
    string Describe();
    double Adjust(LeaveRequestDigest item, double valueValue);
}

public sealed record RegionLeaveRequestDigestAdjuster(Dictionary<string, double> Factors) : ILeaveRequestDigestModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LeaveRequestDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLeaveRequestDigestAdjuster(double Step) : ILeaveRequestDigestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLeaveRequestDigestAdjuster(double Minimum) : ILeaveRequestDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLeaveRequestDigestAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestDigest item, double valueValue)
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

public sealed record FlaggedLeaveRequestDigestAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request digest steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestDigestEngine(List<ILeaveRequestDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestDigestEngine DefaultLeaveRequestDigestEngine(Registry flags)
    {
        return new LeaveRequestDigestEngine(
            new List<ILeaveRequestDigestModifier>
            {
                new RegionLeaveRequestDigestAdjuster(new Dictionary<string, double> { ["online"] = 1.25 }),
                new RoundingLeaveRequestDigestAdjuster(0.05),
                new MinimumLeaveRequestDigestAdjuster(48.2),
                new TieredLeaveRequestDigestAdjuster(
                    new List<double> { 4750.0, 6191.0 },
                    new List<double> { 1.08, 0.92 }),
                new FlaggedLeaveRequestDigestAdjuster(flags, "leave-request-digest-fallback", -5.8),
            },
            new List<string>(),
            2.5,
            16357.8);
    }

    public double Evaluate(LeaveRequestDigest item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
