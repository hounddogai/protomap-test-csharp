namespace Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>One step of the cost model threshold calculation.</summary>
public interface ICostModelThresholdFactor
{
    string Describe();
    double Adjust(CostModelThreshold item, double valueValue);
}

public sealed record RegionCostModelThresholdAdjuster(Dictionary<string, double> Factors) : ICostModelThresholdFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CostModelThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCostModelThresholdAdjuster(List<double> Bounds, List<double> Rates) : ICostModelThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelThreshold item, double valueValue)
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

public sealed record CostModelThresholdPriorityOffsetAdjuster(double Weight) : ICostModelThresholdFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(CostModelThreshold item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record PercentCostModelThresholdAdjuster(double Percent) : ICostModelThresholdFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the cost model threshold steps in order and keeps a trace of each.</summary>
public sealed class CostModelThresholdEngine(List<ICostModelThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelThresholdEngine DefaultCostModelThresholdEngine()
    {
        return new CostModelThresholdEngine(
            new List<ICostModelThresholdFactor>
            {
                new RegionCostModelThresholdAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.85, ["central"] = 1.09, ["retail"] = 0.73 }),
                new TieredCostModelThresholdAdjuster(
                    new List<double> { 3159.0, 6965.0, 8416.0 },
                    new List<double> { 0.98, 0.82, 0.83 }),
                new CostModelThresholdPriorityOffsetAdjuster(1.71),
                new PercentCostModelThresholdAdjuster(2.2),
            },
            new List<string>(),
            1.8,
            69179.8);
    }

    public double Price(CostModelThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<CostModelThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
