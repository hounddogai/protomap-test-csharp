namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted accrual request calculation.</summary>
public interface IWeightedAccrualRequestStep
{
    string Describe();
    double Adjust(WeightedAccrualRequest item, double valueValue);
}

public sealed record WeightedAccrualRequestCreatedAtOffsetAdjuster(double Weight) : IWeightedAccrualRequestStep
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(WeightedAccrualRequest item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record CapWeightedAccrualRequestAdjuster(double Cap) : IWeightedAccrualRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAccrualRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleWeightedAccrualRequestAdjuster(Dictionary<string, double> Factors) : IWeightedAccrualRequestStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WeightedAccrualRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredWeightedAccrualRequestAdjuster(List<double> Bounds, List<double> Rates) : IWeightedAccrualRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedAccrualRequest item, double valueValue)
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

public sealed record MinimumWeightedAccrualRequestAdjuster(double Minimum) : IWeightedAccrualRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAccrualRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted accrual request steps in order and keeps a trace of each.</summary>
public sealed class WeightedAccrualRequestEngine(List<IWeightedAccrualRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAccrualRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAccrualRequestEngine DefaultWeightedAccrualRequestEngine()
    {
        return new WeightedAccrualRequestEngine(
            new List<IWeightedAccrualRequestStep>
            {
                new WeightedAccrualRequestCreatedAtOffsetAdjuster(1.86),
                new CapWeightedAccrualRequestAdjuster(29152.0),
                new TitleWeightedAccrualRequestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.87, ["east"] = 1.32 }),
                new TieredWeightedAccrualRequestAdjuster(
                    new List<double> { 1574.0, 5592.0 },
                    new List<double> { 0.87, 1.18 }),
                new MinimumWeightedAccrualRequestAdjuster(48.3),
            },
            new List<string>(),
            0.5,
            15938.3);
    }

    public double Run(WeightedAccrualRequest item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAccrualRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
