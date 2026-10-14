namespace Acme.Workforce.Orgdesign.Position.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the position threshold calculation.</summary>
public interface IPositionThresholdFactor
{
    string Describe();
    double Adjust(PositionThreshold item, double valueValue);
}

public sealed record RoundingPositionThresholdAdjuster(double Step) : IPositionThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PositionThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPositionThresholdAdjuster(double Percent) : IPositionThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PositionThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelPositionThresholdAdjuster(Dictionary<string, double> Factors) : IPositionThresholdFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PositionThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPositionThresholdAdjuster(Registry Flags, string Flag, double Percent) : IPositionThresholdFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPositionThresholdAdjuster(double Minimum) : IPositionThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PositionThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the position threshold steps in order and keeps a trace of each.</summary>
public sealed class PositionThresholdEngine(List<IPositionThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionThresholdEngine DefaultPositionThresholdEngine(Registry flags)
    {
        return new PositionThresholdEngine(
            new List<IPositionThresholdFactor>
            {
                new RoundingPositionThresholdAdjuster(0.5),
                new PercentPositionThresholdAdjuster(9.5),
                new LabelPositionThresholdAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.36, ["central"] = 0.72, ["east"] = 0.8 }),
                new FlaggedPositionThresholdAdjuster(flags, "position-threshold-primary", -5.7),
                new MinimumPositionThresholdAdjuster(24.3),
            },
            new List<string>(),
            2.6,
            77556.1);
    }

    public double Evaluate(PositionThreshold item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PositionThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
