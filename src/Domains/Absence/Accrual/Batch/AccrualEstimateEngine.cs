namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual estimate calculation.</summary>
public interface IAccrualEstimateAdjuster
{
    string Describe();
    double Adjust(AccrualEstimate item, double valueValue);
}

public sealed record PercentAccrualEstimateAdjuster(double Percent) : IAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAccrualEstimateAdjuster(List<double> Bounds, List<double> Rates) : IAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualEstimate item, double valueValue)
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

public sealed record MinimumAccrualEstimateAdjuster(double Minimum) : IAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAccrualEstimateAdjuster(double Cap) : IAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAccrualEstimateAdjuster(double Step) : IAccrualEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the accrual estimate steps in order and keeps a trace of each.</summary>
public sealed class AccrualEstimateEngine(List<IAccrualEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualEstimateEngine DefaultAccrualEstimateEngine()
    {
        return new AccrualEstimateEngine(
            new List<IAccrualEstimateAdjuster>
            {
                new PercentAccrualEstimateAdjuster(9.9),
                new TieredAccrualEstimateAdjuster(
                    new List<double> { 580.0, 1062.0, 5774.0, 7440.0 },
                    new List<double> { 1.15, 0.97, 1.13, 1.16 }),
                new MinimumAccrualEstimateAdjuster(17.3),
                new CapAccrualEstimateAdjuster(3115.0),
                new RoundingAccrualEstimateAdjuster(1.0),
            },
            new List<string>(),
            3.0,
            56188.9);
    }

    public double Compute(AccrualEstimate item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
