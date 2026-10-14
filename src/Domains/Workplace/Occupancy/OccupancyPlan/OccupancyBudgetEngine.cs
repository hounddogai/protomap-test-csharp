namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the occupancy budget calculation.</summary>
public interface IOccupancyBudgetStep
{
    string Describe();
    double Adjust(OccupancyBudget item, double valueValue);
}

public sealed record TieredOccupancyBudgetAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyBudget item, double valueValue)
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

public sealed record LabelOccupancyBudgetAdjuster(Dictionary<string, double> Factors) : IOccupancyBudgetStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(OccupancyBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedOccupancyBudgetAdjuster(Registry Flags, string Flag, double Percent) : IOccupancyBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OccupancyBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingOccupancyBudgetAdjuster(double Step) : IOccupancyBudgetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OccupancyBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumOccupancyBudgetAdjuster(double Minimum) : IOccupancyBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OccupancyBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentOccupancyBudgetAdjuster(double Percent) : IOccupancyBudgetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the occupancy budget steps in order and keeps a trace of each.</summary>
public sealed class OccupancyBudgetEngine(List<IOccupancyBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyBudgetEngine DefaultOccupancyBudgetEngine(Registry flags)
    {
        return new OccupancyBudgetEngine(
            new List<IOccupancyBudgetStep>
            {
                new TieredOccupancyBudgetAdjuster(
                    new List<double> { 890.0, 5982.0 },
                    new List<double> { 1.13, 1.09 }),
                new LabelOccupancyBudgetAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.9, ["central"] = 1.34, ["east"] = 0.7 }),
                new FlaggedOccupancyBudgetAdjuster(flags, "occupancy-budget-manual", 8.8),
                new RoundingOccupancyBudgetAdjuster(1.0),
                new MinimumOccupancyBudgetAdjuster(5.3),
                new PercentOccupancyBudgetAdjuster(-2.3),
            },
            new List<string>(),
            3.0,
            85584.3);
    }

    public double Evaluate(OccupancyBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
