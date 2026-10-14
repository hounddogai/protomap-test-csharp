namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted succession plan request calculation.</summary>
public interface IWeightedSuccessionPlanRequestStep
{
    string Describe();
    double Adjust(WeightedSuccessionPlanRequest item, double valueValue);
}

public sealed record PercentWeightedSuccessionPlanRequestAdjuster(double Percent) : IWeightedSuccessionPlanRequestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedSuccessionPlanRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedSuccessionPlanRequestPriorityOffsetAdjuster(double Weight) : IWeightedSuccessionPlanRequestStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(WeightedSuccessionPlanRequest item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record TieredWeightedSuccessionPlanRequestAdjuster(List<double> Bounds, List<double> Rates) : IWeightedSuccessionPlanRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedSuccessionPlanRequest item, double valueValue)
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

public sealed record MinimumWeightedSuccessionPlanRequestAdjuster(double Minimum) : IWeightedSuccessionPlanRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedSuccessionPlanRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWeightedSuccessionPlanRequestAdjuster(double Step) : IWeightedSuccessionPlanRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedSuccessionPlanRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted succession plan request steps in order and keeps a trace of each.</summary>
public sealed class WeightedSuccessionPlanRequestEngine(List<IWeightedSuccessionPlanRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedSuccessionPlanRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedSuccessionPlanRequestEngine DefaultWeightedSuccessionPlanRequestEngine()
    {
        return new WeightedSuccessionPlanRequestEngine(
            new List<IWeightedSuccessionPlanRequestStep>
            {
                new PercentWeightedSuccessionPlanRequestAdjuster(5.4),
                new WeightedSuccessionPlanRequestPriorityOffsetAdjuster(0.94),
                new TieredWeightedSuccessionPlanRequestAdjuster(
                    new List<double> { 961.0, 1979.0, 3780.0 },
                    new List<double> { 1.12, 0.87, 1.09 }),
                new MinimumWeightedSuccessionPlanRequestAdjuster(43.6),
                new RoundingWeightedSuccessionPlanRequestAdjuster(0.05),
            },
            new List<string>(),
            1.2,
            64864.2);
    }

    public double Price(WeightedSuccessionPlanRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedSuccessionPlanRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
