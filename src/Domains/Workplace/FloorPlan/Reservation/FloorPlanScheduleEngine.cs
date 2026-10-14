namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan schedule calculation.</summary>
public interface IFloorPlanScheduleModifier
{
    string Describe();
    double Adjust(FloorPlanSchedule item, double valueValue);
}

public sealed record FloorPlanScheduleWindowDaysOffsetAdjuster(double Weight) : IFloorPlanScheduleModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(FloorPlanSchedule item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record TieredFloorPlanScheduleAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanSchedule item, double valueValue)
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

public sealed record FlaggedFloorPlanScheduleAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanScheduleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFloorPlanScheduleAdjuster(double Percent) : IFloorPlanScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelFloorPlanScheduleAdjuster(Dictionary<string, double> Factors) : IFloorPlanScheduleModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(FloorPlanSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFloorPlanScheduleAdjuster(double Step) : IFloorPlanScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the floor plan schedule steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanScheduleEngine(List<IFloorPlanScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanScheduleEngine DefaultFloorPlanScheduleEngine(Registry flags)
    {
        return new FloorPlanScheduleEngine(
            new List<IFloorPlanScheduleModifier>
            {
                new FloorPlanScheduleWindowDaysOffsetAdjuster(1.02),
                new TieredFloorPlanScheduleAdjuster(
                    new List<double> { 3072.0, 4137.0, 9305.0, 9923.0 },
                    new List<double> { 1.19, 0.85, 1.14, 0.94 }),
                new FlaggedFloorPlanScheduleAdjuster(flags, "floor-plan-schedule-seasonal", 6.0),
                new PercentFloorPlanScheduleAdjuster(-1.0),
                new ChannelFloorPlanScheduleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.97, ["retail"] = 0.94, ["central"] = 1.2 }),
                new RoundingFloorPlanScheduleAdjuster(0.25),
            },
            new List<string>(),
            2.0,
            59042.9);
    }

    public double Run(FloorPlanSchedule item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
