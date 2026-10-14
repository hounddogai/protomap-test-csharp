namespace Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>One step of the legacy recognition budget calculation.</summary>
public interface ILegacyRecognitionBudgetFactor
{
    string Describe();
    double Adjust(LegacyRecognitionBudget item, double valueValue);
}

public sealed record CapLegacyRecognitionBudgetAdjuster(double Cap) : ILegacyRecognitionBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyRecognitionBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyRecognitionBudgetUpdatedAtOffsetAdjuster(double Weight) : ILegacyRecognitionBudgetFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(LegacyRecognitionBudget item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record TieredLegacyRecognitionBudgetAdjuster(List<double> Bounds, List<double> Rates) : ILegacyRecognitionBudgetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyRecognitionBudget item, double valueValue)
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

/// <summary>Runs the legacy recognition budget steps in order and keeps a trace of each.</summary>
public sealed class LegacyRecognitionBudgetEngine(List<ILegacyRecognitionBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyRecognitionBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyRecognitionBudgetEngine DefaultLegacyRecognitionBudgetEngine()
    {
        return new LegacyRecognitionBudgetEngine(
            new List<ILegacyRecognitionBudgetFactor>
            {
                new CapLegacyRecognitionBudgetAdjuster(36869.0),
                new LegacyRecognitionBudgetUpdatedAtOffsetAdjuster(1.63),
                new TieredLegacyRecognitionBudgetAdjuster(
                    new List<double> { 696.0, 1863.0, 7281.0, 7661.0 },
                    new List<double> { 0.94, 1.09, 0.97, 1.19 }),
            },
            new List<string>(),
            3.5,
            20669.0);
    }

    public double Run(LegacyRecognitionBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyRecognitionBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
