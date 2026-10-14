namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual access badge cycle calculation.</summary>
public interface IManualAccessBadgeCycleAdjuster
{
    string Describe();
    double Adjust(ManualAccessBadgeCycle item, double valueValue);
}

public sealed record SegmentManualAccessBadgeCycleAdjuster(Dictionary<string, double> Factors) : IManualAccessBadgeCycleAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ManualAccessBadgeCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedManualAccessBadgeCycleAdjuster(Registry Flags, string Flag, double Percent) : IManualAccessBadgeCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualAccessBadgeCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualAccessBadgeCycleAdjuster(double Cap) : IManualAccessBadgeCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualAccessBadgeCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingManualAccessBadgeCycleAdjuster(double Step) : IManualAccessBadgeCycleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAccessBadgeCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumManualAccessBadgeCycleAdjuster(double Minimum) : IManualAccessBadgeCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualAccessBadgeCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ManualAccessBadgeCycleThresholdOffsetAdjuster(double Weight) : IManualAccessBadgeCycleAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(ManualAccessBadgeCycle item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the manual access badge cycle steps in order and keeps a trace of each.</summary>
public sealed class ManualAccessBadgeCycleEngine(List<IManualAccessBadgeCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAccessBadgeCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAccessBadgeCycleEngine DefaultManualAccessBadgeCycleEngine(Registry flags)
    {
        return new ManualAccessBadgeCycleEngine(
            new List<IManualAccessBadgeCycleAdjuster>
            {
                new SegmentManualAccessBadgeCycleAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.86, ["retail"] = 0.92, ["central"] = 1.23 }),
                new FlaggedManualAccessBadgeCycleAdjuster(flags, "manual-access-badge-cycle-weighted", 9.9),
                new CapManualAccessBadgeCycleAdjuster(9932.0),
                new RoundingManualAccessBadgeCycleAdjuster(0.05),
                new MinimumManualAccessBadgeCycleAdjuster(32.6),
                new ManualAccessBadgeCycleThresholdOffsetAdjuster(0.15),
            },
            new List<string>(),
            0.2,
            71273.5);
    }

    public double Price(ManualAccessBadgeCycle item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualAccessBadgeCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
