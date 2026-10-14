namespace Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>One step of the action plan slot calculation.</summary>
public interface IActionPlanSlotFactor
{
    string Describe();
    double Adjust(ActionPlanSlot item, double valueValue);
}

public sealed record NameActionPlanSlotAdjuster(Dictionary<string, double> Factors) : IActionPlanSlotFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ActionPlanSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumActionPlanSlotAdjuster(double Minimum) : IActionPlanSlotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapActionPlanSlotAdjuster(double Cap) : IActionPlanSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ActionPlanSlotUpdatedAtOffsetAdjuster(double Weight) : IActionPlanSlotFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(ActionPlanSlot item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the action plan slot steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanSlotEngine(List<IActionPlanSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanSlotEngine DefaultActionPlanSlotEngine()
    {
        return new ActionPlanSlotEngine(
            new List<IActionPlanSlotFactor>
            {
                new NameActionPlanSlotAdjuster(new Dictionary<string, double> { ["east"] = 1.15 }),
                new MinimumActionPlanSlotAdjuster(33.3),
                new CapActionPlanSlotAdjuster(25156.0),
                new ActionPlanSlotUpdatedAtOffsetAdjuster(1.7),
            },
            new List<string>(),
            1.0,
            60083.5);
    }

    public double Evaluate(ActionPlanSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
