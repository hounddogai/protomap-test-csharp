namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal floor plan record calculation.</summary>
public interface ISeasonalFloorPlanRecordStep
{
    string Describe();
    double Adjust(SeasonalFloorPlanRecord item, double valueValue);
}

public sealed record PercentSeasonalFloorPlanRecordAdjuster(double Percent) : ISeasonalFloorPlanRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalFloorPlanRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalFloorPlanRecordAdjuster(double Cap) : ISeasonalFloorPlanRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalFloorPlanRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSeasonalFloorPlanRecordAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalFloorPlanRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalFloorPlanRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitSeasonalFloorPlanRecordAdjuster(Dictionary<string, double> Factors) : ISeasonalFloorPlanRecordStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(SeasonalFloorPlanRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSeasonalFloorPlanRecordAdjuster(double Minimum) : ISeasonalFloorPlanRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalFloorPlanRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal floor plan record steps in order and keeps a trace of each.</summary>
public sealed class SeasonalFloorPlanRecordEngine(List<ISeasonalFloorPlanRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalFloorPlanRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalFloorPlanRecordEngine DefaultSeasonalFloorPlanRecordEngine(Registry flags)
    {
        return new SeasonalFloorPlanRecordEngine(
            new List<ISeasonalFloorPlanRecordStep>
            {
                new PercentSeasonalFloorPlanRecordAdjuster(-2.8),
                new CapSeasonalFloorPlanRecordAdjuster(30692.0),
                new FlaggedSeasonalFloorPlanRecordAdjuster(flags, "seasonal-floor-plan-record-regional", 8.5),
                new UnitSeasonalFloorPlanRecordAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.32, ["retail"] = 1.01 }),
                new MinimumSeasonalFloorPlanRecordAdjuster(33.6),
            },
            new List<string>(),
            4.8,
            15726.0);
    }

    public double Price(SeasonalFloorPlanRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalFloorPlanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
