namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled town hall override calculation.</summary>
public interface IScheduledTownHallOverrideFactor
{
    string Describe();
    double Adjust(ScheduledTownHallOverride item, double valueValue);
}

public sealed record PercentScheduledTownHallOverrideAdjuster(double Percent) : IScheduledTownHallOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledTownHallOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledTownHallOverrideAdjuster(double Step) : IScheduledTownHallOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledTownHallOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamScheduledTownHallOverrideAdjuster(Dictionary<string, double> Factors) : IScheduledTownHallOverrideFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ScheduledTownHallOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled town hall override steps in order and keeps a trace of each.</summary>
public sealed class ScheduledTownHallOverrideEngine(List<IScheduledTownHallOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledTownHallOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledTownHallOverrideEngine DefaultScheduledTownHallOverrideEngine()
    {
        return new ScheduledTownHallOverrideEngine(
            new List<IScheduledTownHallOverrideFactor>
            {
                new PercentScheduledTownHallOverrideAdjuster(-12.7),
                new RoundingScheduledTownHallOverrideAdjuster(0.05),
                new TeamScheduledTownHallOverrideAdjuster(new Dictionary<string, double> { ["central"] = 1.21 }),
            },
            new List<string>(),
            0.9,
            29789.3);
    }

    public double Compute(ScheduledTownHallOverride item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledTownHallOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
