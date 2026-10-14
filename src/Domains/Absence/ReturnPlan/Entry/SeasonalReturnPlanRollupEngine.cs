namespace Acme.Workforce.Absence.ReturnPlan.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal return plan rollup calculation.</summary>
public interface ISeasonalReturnPlanRollupAdjuster
{
    string Describe();
    double Adjust(SeasonalReturnPlanRollup item, double valueValue);
}

public sealed record SeasonalReturnPlanRollupWeightOffsetAdjuster(double Weight) : ISeasonalReturnPlanRollupAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(SeasonalReturnPlanRollup item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record RoundingSeasonalReturnPlanRollupAdjuster(double Step) : ISeasonalReturnPlanRollupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalReturnPlanRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSeasonalReturnPlanRollupAdjuster(double Minimum) : ISeasonalReturnPlanRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalReturnPlanRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSeasonalReturnPlanRollupAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalReturnPlanRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalReturnPlanRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal return plan rollup steps in order and keeps a trace of each.</summary>
public sealed class SeasonalReturnPlanRollupEngine(List<ISeasonalReturnPlanRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalReturnPlanRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalReturnPlanRollupEngine DefaultSeasonalReturnPlanRollupEngine(Registry flags)
    {
        return new SeasonalReturnPlanRollupEngine(
            new List<ISeasonalReturnPlanRollupAdjuster>
            {
                new SeasonalReturnPlanRollupWeightOffsetAdjuster(1.79),
                new RoundingSeasonalReturnPlanRollupAdjuster(5.0),
                new MinimumSeasonalReturnPlanRollupAdjuster(41.4),
                new FlaggedSeasonalReturnPlanRollupAdjuster(flags, "seasonal-return-plan-rollup-default", -9.7),
            },
            new List<string>(),
            4.4,
            5114.3);
    }

    public double Run(SeasonalReturnPlanRollup item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalReturnPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
