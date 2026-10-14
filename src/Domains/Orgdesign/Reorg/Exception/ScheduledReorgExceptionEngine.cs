namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled reorg exception calculation.</summary>
public interface IScheduledReorgExceptionStep
{
    string Describe();
    double Adjust(ScheduledReorgException item, double valueValue);
}

public sealed record CapScheduledReorgExceptionAdjuster(double Cap) : IScheduledReorgExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledReorgException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionScheduledReorgExceptionAdjuster(Dictionary<string, double> Factors) : IScheduledReorgExceptionStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(ScheduledReorgException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedScheduledReorgExceptionAdjuster(Registry Flags, string Flag, double Percent) : IScheduledReorgExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledReorgException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledReorgExceptionAdjuster(double Step) : IScheduledReorgExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledReorgException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentScheduledReorgExceptionAdjuster(double Percent) : IScheduledReorgExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledReorgException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled reorg exception steps in order and keeps a trace of each.</summary>
public sealed class ScheduledReorgExceptionEngine(List<IScheduledReorgExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledReorgExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledReorgExceptionEngine DefaultScheduledReorgExceptionEngine(Registry flags)
    {
        return new ScheduledReorgExceptionEngine(
            new List<IScheduledReorgExceptionStep>
            {
                new CapScheduledReorgExceptionAdjuster(12469.0),
                new RegionScheduledReorgExceptionAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.3, ["east"] = 1.11, ["south"] = 0.94 }),
                new FlaggedScheduledReorgExceptionAdjuster(flags, "scheduled-reorg-exception-fallback", -5.5),
                new RoundingScheduledReorgExceptionAdjuster(0.5),
                new PercentScheduledReorgExceptionAdjuster(7.3),
            },
            new List<string>(),
            1.3,
            50084.7);
    }

    public double Evaluate(ScheduledReorgException item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledReorgException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
