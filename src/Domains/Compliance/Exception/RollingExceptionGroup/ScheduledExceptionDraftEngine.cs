namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled exception draft calculation.</summary>
public interface IScheduledExceptionDraftStep
{
    string Describe();
    double Adjust(ScheduledExceptionDraft item, double valueValue);
}

public sealed record FlaggedScheduledExceptionDraftAdjuster(Registry Flags, string Flag, double Percent) : IScheduledExceptionDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledExceptionDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledExceptionDraftFteOffsetAdjuster(double Weight) : IScheduledExceptionDraftStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(ScheduledExceptionDraft item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record RoundingScheduledExceptionDraftAdjuster(double Step) : IScheduledExceptionDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledExceptionDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled exception draft steps in order and keeps a trace of each.</summary>
public sealed class ScheduledExceptionDraftEngine(List<IScheduledExceptionDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledExceptionDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledExceptionDraftEngine DefaultScheduledExceptionDraftEngine(Registry flags)
    {
        return new ScheduledExceptionDraftEngine(
            new List<IScheduledExceptionDraftStep>
            {
                new FlaggedScheduledExceptionDraftAdjuster(flags, "scheduled-exception-draft-rolling", 4.4),
                new ScheduledExceptionDraftFteOffsetAdjuster(0.71),
                new RoundingScheduledExceptionDraftAdjuster(5.0),
            },
            new List<string>(),
            3.8,
            81332.6);
    }

    public double Evaluate(ScheduledExceptionDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledExceptionDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
