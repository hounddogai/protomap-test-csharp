namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position profile calculation.</summary>
public interface IPositionProfileModifier
{
    string Describe();
    double Adjust(PositionProfile item, double valueValue);
}

public sealed record TieredPositionProfileAdjuster(List<double> Bounds, List<double> Rates) : IPositionProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PositionProfile item, double valueValue)
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

public sealed record FlaggedPositionProfileAdjuster(Registry Flags, string Flag, double Percent) : IPositionProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPositionProfileAdjuster(double Cap) : IPositionProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PositionProfileMaximumOffsetAdjuster(double Weight) : IPositionProfileModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(PositionProfile item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the position profile steps in order and keeps a trace of each.</summary>
public sealed class PositionProfileEngine(List<IPositionProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionProfileEngine DefaultPositionProfileEngine(Registry flags)
    {
        return new PositionProfileEngine(
            new List<IPositionProfileModifier>
            {
                new TieredPositionProfileAdjuster(
                    new List<double> { 5160.0, 5907.0 },
                    new List<double> { 0.84, 1.08 }),
                new FlaggedPositionProfileAdjuster(flags, "position-profile-regional", -3.7),
                new CapPositionProfileAdjuster(46174.0),
                new PositionProfileMaximumOffsetAdjuster(1.31),
            },
            new List<string>(),
            3.4,
            97535.6);
    }

    public double Price(PositionProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<PositionProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
