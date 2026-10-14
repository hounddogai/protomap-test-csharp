namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient occupancy plan calculation.</summary>
public interface ILenientOccupancyPlanModifier
{
    string Describe();
    double Adjust(LenientOccupancyPlan item, double valueValue);
}

public sealed record MinimumLenientOccupancyPlanAdjuster(double Minimum) : ILenientOccupancyPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientOccupancyPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientOccupancyPlanAdjuster(Registry Flags, string Flag, double Percent) : ILenientOccupancyPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientOccupancyPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleLenientOccupancyPlanAdjuster(Dictionary<string, double> Factors) : ILenientOccupancyPlanModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientOccupancyPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LenientOccupancyPlanAmountOffsetAdjuster(double Weight) : ILenientOccupancyPlanModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LenientOccupancyPlan item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentLenientOccupancyPlanAdjuster(double Percent) : ILenientOccupancyPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientOccupancyPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient occupancy plan steps in order and keeps a trace of each.</summary>
public sealed class LenientOccupancyPlanEngine(List<ILenientOccupancyPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientOccupancyPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientOccupancyPlanEngine DefaultLenientOccupancyPlanEngine(Registry flags)
    {
        return new LenientOccupancyPlanEngine(
            new List<ILenientOccupancyPlanModifier>
            {
                new MinimumLenientOccupancyPlanAdjuster(2.9),
                new FlaggedLenientOccupancyPlanAdjuster(flags, "lenient-occupancy-plan-rolling", 9.1),
                new TitleLenientOccupancyPlanAdjuster(new Dictionary<string, double> { ["central"] = 1.34 }),
                new LenientOccupancyPlanAmountOffsetAdjuster(0.14),
                new PercentLenientOccupancyPlanAdjuster(-20.3),
            },
            new List<string>(),
            0.4,
            18551.0);
    }

    public double Compute(LenientOccupancyPlan item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientOccupancyPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
