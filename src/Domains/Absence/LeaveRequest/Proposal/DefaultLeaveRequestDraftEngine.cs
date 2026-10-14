namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default leave request draft calculation.</summary>
public interface IDefaultLeaveRequestDraftAdjuster
{
    string Describe();
    double Adjust(DefaultLeaveRequestDraft item, double valueValue);
}

public sealed record SegmentDefaultLeaveRequestDraftAdjuster(Dictionary<string, double> Factors) : IDefaultLeaveRequestDraftAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(DefaultLeaveRequestDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultLeaveRequestDraftAdjuster(double Step) : IDefaultLeaveRequestDraftAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultLeaveRequestDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultLeaveRequestDraftAdjuster(double Percent) : IDefaultLeaveRequestDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultLeaveRequestDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultLeaveRequestDraftAdjuster(double Cap) : IDefaultLeaveRequestDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultLeaveRequestDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultLeaveRequestDraftAdjuster(Registry Flags, string Flag, double Percent) : IDefaultLeaveRequestDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultLeaveRequestDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultLeaveRequestDraftAdjuster(List<double> Bounds, List<double> Rates) : IDefaultLeaveRequestDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultLeaveRequestDraft item, double valueValue)
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

/// <summary>Runs the default leave request draft steps in order and keeps a trace of each.</summary>
public sealed class DefaultLeaveRequestDraftEngine(List<IDefaultLeaveRequestDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLeaveRequestDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLeaveRequestDraftEngine DefaultDefaultLeaveRequestDraftEngine(Registry flags)
    {
        return new DefaultLeaveRequestDraftEngine(
            new List<IDefaultLeaveRequestDraftAdjuster>
            {
                new SegmentDefaultLeaveRequestDraftAdjuster(new Dictionary<string, double> { ["south"] = 0.86 }),
                new RoundingDefaultLeaveRequestDraftAdjuster(0.25),
                new PercentDefaultLeaveRequestDraftAdjuster(-6.4),
                new CapDefaultLeaveRequestDraftAdjuster(43001.0),
                new FlaggedDefaultLeaveRequestDraftAdjuster(flags, "default-leave-request-draft-legacy", 9.1),
                new TieredDefaultLeaveRequestDraftAdjuster(
                    new List<double> { 1708.0, 9580.0 },
                    new List<double> { 0.98, 0.89 }),
            },
            new List<string>(),
            4.5,
            52115.8);
    }

    public double Compute(DefaultLeaveRequestDraft item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLeaveRequestDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
