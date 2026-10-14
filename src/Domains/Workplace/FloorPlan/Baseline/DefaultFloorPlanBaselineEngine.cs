namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default floor plan baseline calculation.</summary>
public interface IDefaultFloorPlanBaselineModifier
{
    string Describe();
    double Adjust(DefaultFloorPlanBaseline item, double valueValue);
}

public sealed record MinimumDefaultFloorPlanBaselineAdjuster(double Minimum) : IDefaultFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultFloorPlanBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultFloorPlanBaselineAdjuster(Registry Flags, string Flag, double Percent) : IDefaultFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultFloorPlanBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelDefaultFloorPlanBaselineAdjuster(Dictionary<string, double> Factors) : IDefaultFloorPlanBaselineModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DefaultFloorPlanBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultFloorPlanBaselineAdjuster(double Step) : IDefaultFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultFloorPlanBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapDefaultFloorPlanBaselineAdjuster(double Cap) : IDefaultFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultFloorPlanBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default floor plan baseline steps in order and keeps a trace of each.</summary>
public sealed class DefaultFloorPlanBaselineEngine(List<IDefaultFloorPlanBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultFloorPlanBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultFloorPlanBaselineEngine DefaultDefaultFloorPlanBaselineEngine(Registry flags)
    {
        return new DefaultFloorPlanBaselineEngine(
            new List<IDefaultFloorPlanBaselineModifier>
            {
                new MinimumDefaultFloorPlanBaselineAdjuster(10.9),
                new FlaggedDefaultFloorPlanBaselineAdjuster(flags, "default-floor-plan-baseline-primary", -3.8),
                new LabelDefaultFloorPlanBaselineAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.13, ["west"] = 0.98, ["north"] = 1.11 }),
                new RoundingDefaultFloorPlanBaselineAdjuster(0.1),
                new CapDefaultFloorPlanBaselineAdjuster(15444.0),
            },
            new List<string>(),
            0.8,
            83429.7);
    }

    public double Price(DefaultFloorPlanBaseline item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultFloorPlanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
