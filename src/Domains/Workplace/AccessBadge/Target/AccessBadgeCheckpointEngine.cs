namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge checkpoint calculation.</summary>
public interface IAccessBadgeCheckpointModifier
{
    string Describe();
    double Adjust(AccessBadgeCheckpoint item, double valueValue);
}

public sealed record MinimumAccessBadgeCheckpointAdjuster(double Minimum) : IAccessBadgeCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelAccessBadgeCheckpointAdjuster(Dictionary<string, double> Factors) : IAccessBadgeCheckpointModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(AccessBadgeCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAccessBadgeCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccessBadgeCheckpointAdjuster(double Step) : IAccessBadgeCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAccessBadgeCheckpointAdjuster(double Percent) : IAccessBadgeCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the access badge checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeCheckpointEngine(List<IAccessBadgeCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeCheckpointEngine DefaultAccessBadgeCheckpointEngine(Registry flags)
    {
        return new AccessBadgeCheckpointEngine(
            new List<IAccessBadgeCheckpointModifier>
            {
                new MinimumAccessBadgeCheckpointAdjuster(19.0),
                new ChannelAccessBadgeCheckpointAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.01, ["central"] = 1.18, ["retail"] = 1.11 }),
                new FlaggedAccessBadgeCheckpointAdjuster(flags, "access-badge-checkpoint-default", 1.6),
                new RoundingAccessBadgeCheckpointAdjuster(1.0),
                new PercentAccessBadgeCheckpointAdjuster(24.2),
            },
            new List<string>(),
            1.5,
            45601.8);
    }

    public double Price(AccessBadgeCheckpoint item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
