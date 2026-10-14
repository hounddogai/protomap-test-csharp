namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>One step of the cost model variance calculation.</summary>
public interface ICostModelVarianceStep
{
    string Describe();
    double Adjust(CostModelVariance item, double valueValue);
}

public sealed record MinimumCostModelVarianceAdjuster(double Minimum) : ICostModelVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCostModelVarianceAdjuster(List<double> Bounds, List<double> Rates) : ICostModelVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelVariance item, double valueValue)
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

public sealed record PercentCostModelVarianceAdjuster(double Percent) : ICostModelVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the cost model variance steps in order and keeps a trace of each.</summary>
public sealed class CostModelVarianceEngine(List<ICostModelVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelVarianceEngine DefaultCostModelVarianceEngine()
    {
        return new CostModelVarianceEngine(
            new List<ICostModelVarianceStep>
            {
                new MinimumCostModelVarianceAdjuster(9.9),
                new TieredCostModelVarianceAdjuster(
                    new List<double> { 5337.0, 5458.0, 6321.0, 7110.0 },
                    new List<double> { 1.13, 1.16, 1.05, 1.15 }),
                new PercentCostModelVarianceAdjuster(6.1),
            },
            new List<string>(),
            3.7,
            29984.8);
    }

    public double Price(CostModelVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<CostModelVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
