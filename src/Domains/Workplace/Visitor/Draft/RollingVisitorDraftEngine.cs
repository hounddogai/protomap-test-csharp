namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling visitor draft calculation.</summary>
public interface IRollingVisitorDraftAdjuster
{
    string Describe();
    double Adjust(RollingVisitorDraft item, double valueValue);
}

public sealed record FlaggedRollingVisitorDraftAdjuster(Registry Flags, string Flag, double Percent) : IRollingVisitorDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingVisitorDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingVisitorDraftPriorityOffsetAdjuster(double Weight) : IRollingVisitorDraftAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(RollingVisitorDraft item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record RoundingRollingVisitorDraftAdjuster(double Step) : IRollingVisitorDraftAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingVisitorDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRollingVisitorDraftAdjuster(double Percent) : IRollingVisitorDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingVisitorDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingVisitorDraftAdjuster(double Cap) : IRollingVisitorDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingVisitorDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRollingVisitorDraftAdjuster(double Minimum) : IRollingVisitorDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingVisitorDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling visitor draft steps in order and keeps a trace of each.</summary>
public sealed class RollingVisitorDraftEngine(List<IRollingVisitorDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingVisitorDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingVisitorDraftEngine DefaultRollingVisitorDraftEngine(Registry flags)
    {
        return new RollingVisitorDraftEngine(
            new List<IRollingVisitorDraftAdjuster>
            {
                new FlaggedRollingVisitorDraftAdjuster(flags, "rolling-visitor-draft-weighted", -1.2),
                new RollingVisitorDraftPriorityOffsetAdjuster(1.22),
                new RoundingRollingVisitorDraftAdjuster(0.05),
                new PercentRollingVisitorDraftAdjuster(22.0),
                new CapRollingVisitorDraftAdjuster(34672.0),
                new MinimumRollingVisitorDraftAdjuster(35.7),
            },
            new List<string>(),
            4.0,
            55156.7);
    }

    public double Compute(RollingVisitorDraft item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingVisitorDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
