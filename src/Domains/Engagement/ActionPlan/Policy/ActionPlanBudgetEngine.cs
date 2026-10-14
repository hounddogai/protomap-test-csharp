namespace Acme.Workforce.Engagement.ActionPlan.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan budget calculation.</summary>
public interface IActionPlanBudgetStep
{
    string Describe();
    double Adjust(ActionPlanBudget item, double valueValue);
}

public sealed record ActionPlanBudgetDurationMinutesOffsetAdjuster(double Weight) : IActionPlanBudgetStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(ActionPlanBudget item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record CapActionPlanBudgetAdjuster(double Cap) : IActionPlanBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredActionPlanBudgetAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanBudget item, double valueValue)
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

/// <summary>Runs the action plan budget steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanBudgetEngine(List<IActionPlanBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanBudgetEngine DefaultActionPlanBudgetEngine()
    {
        return new ActionPlanBudgetEngine(
            new List<IActionPlanBudgetStep>
            {
                new ActionPlanBudgetDurationMinutesOffsetAdjuster(1.05),
                new CapActionPlanBudgetAdjuster(38231.0),
                new TieredActionPlanBudgetAdjuster(
                    new List<double> { 3399.0, 5995.0, 8032.0, 8090.0 },
                    new List<double> { 1.08, 1.17, 1.18, 0.9 }),
            },
            new List<string>(),
            3.0,
            47542.8);
    }

    public double Evaluate(ActionPlanBudget item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
