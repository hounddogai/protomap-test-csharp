namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan entry calculation.</summary>
public interface IActionPlanEntryStep
{
    string Describe();
    double Adjust(ActionPlanEntry item, double valueValue);
}

public sealed record PercentActionPlanEntryAdjuster(double Percent) : IActionPlanEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingActionPlanEntryAdjuster(double Step) : IActionPlanEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredActionPlanEntryAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanEntry item, double valueValue)
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

public sealed record MinimumActionPlanEntryAdjuster(double Minimum) : IActionPlanEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapActionPlanEntryAdjuster(double Cap) : IActionPlanEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ActionPlanEntryThresholdOffsetAdjuster(double Weight) : IActionPlanEntryStep
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(ActionPlanEntry item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the action plan entry steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanEntryEngine(List<IActionPlanEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanEntryEngine DefaultActionPlanEntryEngine()
    {
        return new ActionPlanEntryEngine(
            new List<IActionPlanEntryStep>
            {
                new PercentActionPlanEntryAdjuster(-7.9),
                new RoundingActionPlanEntryAdjuster(1.0),
                new TieredActionPlanEntryAdjuster(
                    new List<double> { 4412.0, 5438.0, 6273.0 },
                    new List<double> { 1.17, 1.09, 0.89 }),
                new MinimumActionPlanEntryAdjuster(40.6),
                new CapActionPlanEntryAdjuster(45060.0),
                new ActionPlanEntryThresholdOffsetAdjuster(0.57),
            },
            new List<string>(),
            4.9,
            86198.6);
    }

    public double Run(ActionPlanEntry item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
