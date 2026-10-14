namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default reorg tier calculation.</summary>
public interface IDefaultReorgTierModifier
{
    string Describe();
    double Adjust(DefaultReorgTier item, double valueValue);
}

public sealed record CapDefaultReorgTierAdjuster(double Cap) : IDefaultReorgTierModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultReorgTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedDefaultReorgTierAdjuster(Registry Flags, string Flag, double Percent) : IDefaultReorgTierModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultReorgTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultReorgTierAdjuster(double Step) : IDefaultReorgTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultReorgTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDefaultReorgTierAdjuster(double Minimum) : IDefaultReorgTierModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultReorgTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultReorgTierDurationMinutesOffsetAdjuster(double Weight) : IDefaultReorgTierModifier
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(DefaultReorgTier item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the default reorg tier steps in order and keeps a trace of each.</summary>
public sealed class DefaultReorgTierEngine(List<IDefaultReorgTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultReorgTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultReorgTierEngine DefaultDefaultReorgTierEngine(Registry flags)
    {
        return new DefaultReorgTierEngine(
            new List<IDefaultReorgTierModifier>
            {
                new CapDefaultReorgTierAdjuster(13214.0),
                new FlaggedDefaultReorgTierAdjuster(flags, "default-reorg-tier-global", 3.9),
                new RoundingDefaultReorgTierAdjuster(0.05),
                new MinimumDefaultReorgTierAdjuster(20.6),
                new DefaultReorgTierDurationMinutesOffsetAdjuster(1.72),
            },
            new List<string>(),
            0.8,
            23286.5);
    }

    public double Run(DefaultReorgTier item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultReorgTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
