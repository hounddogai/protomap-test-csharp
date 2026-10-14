namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending visitor profile calculation.</summary>
public interface IPendingVisitorProfileAdjuster
{
    string Describe();
    double Adjust(PendingVisitorProfile item, double valueValue);
}

public sealed record TitlePendingVisitorProfileAdjuster(Dictionary<string, double> Factors) : IPendingVisitorProfileAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PendingVisitorProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPendingVisitorProfileAdjuster(Registry Flags, string Flag, double Percent) : IPendingVisitorProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingVisitorProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingVisitorProfileAdjuster(double Step) : IPendingVisitorProfileAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingVisitorProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending visitor profile steps in order and keeps a trace of each.</summary>
public sealed class PendingVisitorProfileEngine(List<IPendingVisitorProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingVisitorProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingVisitorProfileEngine DefaultPendingVisitorProfileEngine(Registry flags)
    {
        return new PendingVisitorProfileEngine(
            new List<IPendingVisitorProfileAdjuster>
            {
                new TitlePendingVisitorProfileAdjuster(new Dictionary<string, double> { ["retail"] = 1.25 }),
                new FlaggedPendingVisitorProfileAdjuster(flags, "pending-visitor-profile-default", 7.0),
                new RoundingPendingVisitorProfileAdjuster(5.0),
            },
            new List<string>(),
            1.4,
            43628.7);
    }

    public double Price(PendingVisitorProfile item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingVisitorProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
