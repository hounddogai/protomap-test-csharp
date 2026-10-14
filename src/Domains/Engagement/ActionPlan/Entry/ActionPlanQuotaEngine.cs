namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan quota calculation.</summary>
public interface IActionPlanQuotaStep
{
    string Describe();
    double Adjust(ActionPlanQuota item, double valueValue);
}

public sealed record TieredActionPlanQuotaAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanQuotaStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanQuota item, double valueValue)
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

public sealed record CapActionPlanQuotaAdjuster(double Cap) : IActionPlanQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ActionPlanQuotaTenureMonthsOffsetAdjuster(double Weight) : IActionPlanQuotaStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(ActionPlanQuota item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the action plan quota steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanQuotaEngine(List<IActionPlanQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanQuotaEngine DefaultActionPlanQuotaEngine()
    {
        return new ActionPlanQuotaEngine(
            new List<IActionPlanQuotaStep>
            {
                new TieredActionPlanQuotaAdjuster(
                    new List<double> { 1453.0, 2577.0, 3810.0 },
                    new List<double> { 0.88, 1.11, 1.06 }),
                new CapActionPlanQuotaAdjuster(25034.0),
                new ActionPlanQuotaTenureMonthsOffsetAdjuster(0.29),
            },
            new List<string>(),
            2.4,
            50239.4);
    }

    public double Compute(ActionPlanQuota item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
