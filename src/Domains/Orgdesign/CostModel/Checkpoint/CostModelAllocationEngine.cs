namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model allocation calculation.</summary>
public interface ICostModelAllocationFactor
{
    string Describe();
    double Adjust(CostModelAllocation item, double valueValue);
}

public sealed record LabelCostModelAllocationAdjuster(Dictionary<string, double> Factors) : ICostModelAllocationFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CostModelAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CostModelAllocationScoreOffsetAdjuster(double Weight) : ICostModelAllocationFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(CostModelAllocation item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record TieredCostModelAllocationAdjuster(List<double> Bounds, List<double> Rates) : ICostModelAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelAllocation item, double valueValue)
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

public sealed record PercentCostModelAllocationAdjuster(double Percent) : ICostModelAllocationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCostModelAllocationAdjuster(double Step) : ICostModelAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCostModelAllocationAdjuster(double Minimum) : ICostModelAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the cost model allocation steps in order and keeps a trace of each.</summary>
public sealed class CostModelAllocationEngine(List<ICostModelAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelAllocationEngine DefaultCostModelAllocationEngine()
    {
        return new CostModelAllocationEngine(
            new List<ICostModelAllocationFactor>
            {
                new LabelCostModelAllocationAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.03, ["central"] = 0.77 }),
                new CostModelAllocationScoreOffsetAdjuster(0.88),
                new TieredCostModelAllocationAdjuster(
                    new List<double> { 2515.0, 3778.0, 6590.0, 8765.0 },
                    new List<double> { 0.9, 1.06, 0.85, 1.09 }),
                new PercentCostModelAllocationAdjuster(0.4),
                new RoundingCostModelAllocationAdjuster(1.0),
                new MinimumCostModelAllocationAdjuster(23.0),
            },
            new List<string>(),
            4.4,
            59440.6);
    }

    public double Compute(CostModelAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<CostModelAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
