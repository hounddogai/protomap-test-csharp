namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan profile calculation.</summary>
public interface IActionPlanProfileFactor
{
    string Describe();
    double Adjust(ActionPlanProfile item, double valueValue);
}

public sealed record ActionPlanProfileAmountOffsetAdjuster(double Weight) : IActionPlanProfileFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(ActionPlanProfile item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record CapActionPlanProfileAdjuster(double Cap) : IActionPlanProfileFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumActionPlanProfileAdjuster(double Minimum) : IActionPlanProfileFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredActionPlanProfileAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanProfileFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanProfile item, double valueValue)
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

/// <summary>Runs the action plan profile steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanProfileEngine(List<IActionPlanProfileFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanProfileFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanProfileEngine DefaultActionPlanProfileEngine()
    {
        return new ActionPlanProfileEngine(
            new List<IActionPlanProfileFactor>
            {
                new ActionPlanProfileAmountOffsetAdjuster(0.16),
                new CapActionPlanProfileAdjuster(3798.0),
                new MinimumActionPlanProfileAdjuster(4.5),
                new TieredActionPlanProfileAdjuster(
                    new List<double> { 3075.0, 3497.0, 7340.0, 8291.0 },
                    new List<double> { 0.84, 1.04, 0.92, 1.08 }),
            },
            new List<string>(),
            0.8,
            49826.2);
    }

    public double Price(ActionPlanProfile item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
