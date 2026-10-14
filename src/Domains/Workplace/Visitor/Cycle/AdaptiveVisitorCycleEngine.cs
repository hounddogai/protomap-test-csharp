namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive visitor cycle calculation.</summary>
public interface IAdaptiveVisitorCycleFactor
{
    string Describe();
    double Adjust(AdaptiveVisitorCycle item, double valueValue);
}

public sealed record ChannelAdaptiveVisitorCycleAdjuster(Dictionary<string, double> Factors) : IAdaptiveVisitorCycleFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(AdaptiveVisitorCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptiveVisitorCycleAdjuster(double Step) : IAdaptiveVisitorCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveVisitorCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveVisitorCycleAdjuster(double Percent) : IAdaptiveVisitorCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveVisitorCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveVisitorCycleAdjuster(double Minimum) : IAdaptiveVisitorCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveVisitorCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdaptiveVisitorCycleLeaveDaysOffsetAdjuster(double Weight) : IAdaptiveVisitorCycleFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(AdaptiveVisitorCycle item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedAdaptiveVisitorCycleAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveVisitorCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveVisitorCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive visitor cycle steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveVisitorCycleEngine(List<IAdaptiveVisitorCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveVisitorCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveVisitorCycleEngine DefaultAdaptiveVisitorCycleEngine(Registry flags)
    {
        return new AdaptiveVisitorCycleEngine(
            new List<IAdaptiveVisitorCycleFactor>
            {
                new ChannelAdaptiveVisitorCycleAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.99, ["central"] = 0.91 }),
                new RoundingAdaptiveVisitorCycleAdjuster(1.0),
                new PercentAdaptiveVisitorCycleAdjuster(11.6),
                new MinimumAdaptiveVisitorCycleAdjuster(40.1),
                new AdaptiveVisitorCycleLeaveDaysOffsetAdjuster(1.73),
                new FlaggedAdaptiveVisitorCycleAdjuster(flags, "adaptive-visitor-cycle-seasonal", -4.4),
            },
            new List<string>(),
            3.1,
            25639.9);
    }

    public double Evaluate(AdaptiveVisitorCycle item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveVisitorCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
