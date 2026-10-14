namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the floor plan score calculation.</summary>
public interface IFloorPlanScoreAdjuster
{
    string Describe();
    double Adjust(FloorPlanScore item, double valueValue);
}

public sealed record CapFloorPlanScoreAdjuster(double Cap) : IFloorPlanScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumFloorPlanScoreAdjuster(double Minimum) : IFloorPlanScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FloorPlanScoreCreatedAtOffsetAdjuster(double Weight) : IFloorPlanScoreAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(FloorPlanScore item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record PercentFloorPlanScoreAdjuster(double Percent) : IFloorPlanScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFloorPlanScoreAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan score steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanScoreEngine(List<IFloorPlanScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanScoreEngine DefaultFloorPlanScoreEngine(Registry flags)
    {
        return new FloorPlanScoreEngine(
            new List<IFloorPlanScoreAdjuster>
            {
                new CapFloorPlanScoreAdjuster(31674.0),
                new MinimumFloorPlanScoreAdjuster(3.9),
                new FloorPlanScoreCreatedAtOffsetAdjuster(1.38),
                new PercentFloorPlanScoreAdjuster(24.3),
                new FlaggedFloorPlanScoreAdjuster(flags, "floor-plan-score-regional", -3.8),
            },
            new List<string>(),
            2.7,
            59107.1);
    }

    public double Evaluate(FloorPlanScore item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FloorPlanScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
