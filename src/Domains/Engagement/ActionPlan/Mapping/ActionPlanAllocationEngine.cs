namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan allocation calculation.</summary>
public interface IActionPlanAllocationModifier
{
    string Describe();
    double Adjust(ActionPlanAllocation item, double valueValue);
}

public sealed record TieredActionPlanAllocationAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanAllocation item, double valueValue)
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

public sealed record RoundingActionPlanAllocationAdjuster(double Step) : IActionPlanAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapActionPlanAllocationAdjuster(double Cap) : IActionPlanAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the action plan allocation steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanAllocationEngine(List<IActionPlanAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanAllocationEngine DefaultActionPlanAllocationEngine()
    {
        return new ActionPlanAllocationEngine(
            new List<IActionPlanAllocationModifier>
            {
                new TieredActionPlanAllocationAdjuster(
                    new List<double> { 3655.0, 4328.0 },
                    new List<double> { 1.05, 1.1 }),
                new RoundingActionPlanAllocationAdjuster(0.25),
                new CapActionPlanAllocationAdjuster(11466.0),
            },
            new List<string>(),
            4.7,
            82685.4);
    }

    public double Price(ActionPlanAllocation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
