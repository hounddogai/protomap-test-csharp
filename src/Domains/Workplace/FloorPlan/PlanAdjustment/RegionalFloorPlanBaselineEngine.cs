namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional floor plan baseline calculation.</summary>
public interface IRegionalFloorPlanBaselineFactor
{
    string Describe();
    double Adjust(RegionalFloorPlanBaseline item, double valueValue);
}

public sealed record CapRegionalFloorPlanBaselineAdjuster(double Cap) : IRegionalFloorPlanBaselineFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalFloorPlanBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRegionalFloorPlanBaselineAdjuster(Registry Flags, string Flag, double Percent) : IRegionalFloorPlanBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalFloorPlanBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRegionalFloorPlanBaselineAdjuster(double Percent) : IRegionalFloorPlanBaselineFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalFloorPlanBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelRegionalFloorPlanBaselineAdjuster(Dictionary<string, double> Factors) : IRegionalFloorPlanBaselineFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalFloorPlanBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RegionalFloorPlanBaselineHoursOffsetAdjuster(double Weight) : IRegionalFloorPlanBaselineFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(RegionalFloorPlanBaseline item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record MinimumRegionalFloorPlanBaselineAdjuster(double Minimum) : IRegionalFloorPlanBaselineFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalFloorPlanBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional floor plan baseline steps in order and keeps a trace of each.</summary>
public sealed class RegionalFloorPlanBaselineEngine(List<IRegionalFloorPlanBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalFloorPlanBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalFloorPlanBaselineEngine DefaultRegionalFloorPlanBaselineEngine(Registry flags)
    {
        return new RegionalFloorPlanBaselineEngine(
            new List<IRegionalFloorPlanBaselineFactor>
            {
                new CapRegionalFloorPlanBaselineAdjuster(16350.0),
                new FlaggedRegionalFloorPlanBaselineAdjuster(flags, "regional-floor-plan-baseline-batched", -7.6),
                new PercentRegionalFloorPlanBaselineAdjuster(-2.5),
                new LabelRegionalFloorPlanBaselineAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.96, ["east"] = 1.32, ["central"] = 1.34 }),
                new RegionalFloorPlanBaselineHoursOffsetAdjuster(0.62),
                new MinimumRegionalFloorPlanBaselineAdjuster(8.9),
            },
            new List<string>(),
            4.7,
            76083.4);
    }

    public double Run(RegionalFloorPlanBaseline item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalFloorPlanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
