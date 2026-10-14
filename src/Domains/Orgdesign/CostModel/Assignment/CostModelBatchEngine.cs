namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model batch calculation.</summary>
public interface ICostModelBatchAdjuster
{
    string Describe();
    double Adjust(CostModelBatch item, double valueValue);
}

public sealed record RoundingCostModelBatchAdjuster(double Step) : ICostModelBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCostModelBatchAdjuster(Registry Flags, string Flag, double Percent) : ICostModelBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCostModelBatchAdjuster(List<double> Bounds, List<double> Rates) : ICostModelBatchAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelBatch item, double valueValue)
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

public sealed record MinimumCostModelBatchAdjuster(double Minimum) : ICostModelBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapCostModelBatchAdjuster(double Cap) : ICostModelBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the cost model batch steps in order and keeps a trace of each.</summary>
public sealed class CostModelBatchEngine(List<ICostModelBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelBatchEngine DefaultCostModelBatchEngine(Registry flags)
    {
        return new CostModelBatchEngine(
            new List<ICostModelBatchAdjuster>
            {
                new RoundingCostModelBatchAdjuster(5.0),
                new FlaggedCostModelBatchAdjuster(flags, "cost-model-batch-capped", 6.7),
                new TieredCostModelBatchAdjuster(
                    new List<double> { 5582.0, 6252.0, 8169.0 },
                    new List<double> { 0.87, 0.93, 1.15 }),
                new MinimumCostModelBatchAdjuster(5.8),
                new CapCostModelBatchAdjuster(33826.0),
            },
            new List<string>(),
            4.3,
            13411.7);
    }

    public double Compute(CostModelBatch item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
