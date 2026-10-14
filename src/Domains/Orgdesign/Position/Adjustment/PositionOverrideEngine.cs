namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position override calculation.</summary>
public interface IPositionOverrideAdjuster
{
    string Describe();
    double Adjust(PositionOverride item, double valueValue);
}

public sealed record NamePositionOverrideAdjuster(Dictionary<string, double> Factors) : IPositionOverrideAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PositionOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPositionOverrideAdjuster(double Cap) : IPositionOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPositionOverrideAdjuster(Registry Flags, string Flag, double Percent) : IPositionOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPositionOverrideAdjuster(double Minimum) : IPositionOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPositionOverrideAdjuster(double Percent) : IPositionOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the position override steps in order and keeps a trace of each.</summary>
public sealed class PositionOverrideEngine(List<IPositionOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionOverrideEngine DefaultPositionOverrideEngine(Registry flags)
    {
        return new PositionOverrideEngine(
            new List<IPositionOverrideAdjuster>
            {
                new NamePositionOverrideAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.15, ["west"] = 1.14 }),
                new CapPositionOverrideAdjuster(10373.0),
                new FlaggedPositionOverrideAdjuster(flags, "position-override-scheduled", 2.9),
                new MinimumPositionOverrideAdjuster(46.2),
                new PercentPositionOverrideAdjuster(5.8),
            },
            new List<string>(),
            2.3,
            47235.5);
    }

    public double Evaluate(PositionOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<PositionOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
