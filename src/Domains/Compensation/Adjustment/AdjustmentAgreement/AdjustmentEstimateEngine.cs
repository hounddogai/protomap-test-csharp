namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>One step of the adjustment estimate calculation.</summary>
public interface IAdjustmentEstimateFactor
{
    string Describe();
    double Adjust(AdjustmentEstimate item, double valueValue);
}

public sealed record AdjustmentEstimateRateOffsetAdjuster(double Weight) : IAdjustmentEstimateFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(AdjustmentEstimate item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record PercentAdjustmentEstimateAdjuster(double Percent) : IAdjustmentEstimateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdjustmentEstimateAdjuster(double Minimum) : IAdjustmentEstimateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdjustmentEstimateAdjuster(double Cap) : IAdjustmentEstimateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adjustment estimate steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentEstimateEngine(List<IAdjustmentEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentEstimateEngine DefaultAdjustmentEstimateEngine()
    {
        return new AdjustmentEstimateEngine(
            new List<IAdjustmentEstimateFactor>
            {
                new AdjustmentEstimateRateOffsetAdjuster(0.64),
                new PercentAdjustmentEstimateAdjuster(-17.9),
                new MinimumAdjustmentEstimateAdjuster(28.4),
                new CapAdjustmentEstimateAdjuster(26621.0),
            },
            new List<string>(),
            4.2,
            74172.6);
    }

    public double Compute(AdjustmentEstimate item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
