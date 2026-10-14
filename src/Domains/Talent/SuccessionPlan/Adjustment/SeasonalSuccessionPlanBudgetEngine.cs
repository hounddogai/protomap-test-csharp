namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>One step of the seasonal succession plan budget calculation.</summary>
public interface ISeasonalSuccessionPlanBudgetFactor
{
    string Describe();
    double Adjust(SeasonalSuccessionPlanBudget item, double valueValue);
}

public sealed record SeasonalSuccessionPlanBudgetCostOffsetAdjuster(double Weight) : ISeasonalSuccessionPlanBudgetFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(SeasonalSuccessionPlanBudget item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record TieredSeasonalSuccessionPlanBudgetAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalSuccessionPlanBudgetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalSuccessionPlanBudget item, double valueValue)
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

public sealed record MinimumSeasonalSuccessionPlanBudgetAdjuster(double Minimum) : ISeasonalSuccessionPlanBudgetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalSuccessionPlanBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal succession plan budget steps in order and keeps a trace of each.</summary>
public sealed class SeasonalSuccessionPlanBudgetEngine(List<ISeasonalSuccessionPlanBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalSuccessionPlanBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalSuccessionPlanBudgetEngine DefaultSeasonalSuccessionPlanBudgetEngine()
    {
        return new SeasonalSuccessionPlanBudgetEngine(
            new List<ISeasonalSuccessionPlanBudgetFactor>
            {
                new SeasonalSuccessionPlanBudgetCostOffsetAdjuster(0.04),
                new TieredSeasonalSuccessionPlanBudgetAdjuster(
                    new List<double> { 5017.0, 6603.0, 9694.0 },
                    new List<double> { 0.89, 1.13, 0.82 }),
                new MinimumSeasonalSuccessionPlanBudgetAdjuster(0.5),
            },
            new List<string>(),
            1.8,
            14188.5);
    }

    public double Run(SeasonalSuccessionPlanBudget item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalSuccessionPlanBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
