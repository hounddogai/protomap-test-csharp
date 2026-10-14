namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched laptop variance calculation.</summary>
public interface IBatchedLaptopVarianceModifier
{
    string Describe();
    double Adjust(BatchedLaptopVariance item, double valueValue);
}

public sealed record MinimumBatchedLaptopVarianceAdjuster(double Minimum) : IBatchedLaptopVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedLaptopVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBatchedLaptopVarianceAdjuster(double Cap) : IBatchedLaptopVarianceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedLaptopVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedLaptopVarianceAdjuster(List<double> Bounds, List<double> Rates) : IBatchedLaptopVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedLaptopVariance item, double valueValue)
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

public sealed record FlaggedBatchedLaptopVarianceAdjuster(Registry Flags, string Flag, double Percent) : IBatchedLaptopVarianceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedLaptopVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBatchedLaptopVarianceAdjuster(double Step) : IBatchedLaptopVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedLaptopVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBatchedLaptopVarianceAdjuster(double Percent) : IBatchedLaptopVarianceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedLaptopVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched laptop variance steps in order and keeps a trace of each.</summary>
public sealed class BatchedLaptopVarianceEngine(List<IBatchedLaptopVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedLaptopVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedLaptopVarianceEngine DefaultBatchedLaptopVarianceEngine(Registry flags)
    {
        return new BatchedLaptopVarianceEngine(
            new List<IBatchedLaptopVarianceModifier>
            {
                new MinimumBatchedLaptopVarianceAdjuster(7.1),
                new CapBatchedLaptopVarianceAdjuster(39224.0),
                new TieredBatchedLaptopVarianceAdjuster(
                    new List<double> { 1681.0, 2361.0 },
                    new List<double> { 0.87, 1.05 }),
                new FlaggedBatchedLaptopVarianceAdjuster(flags, "batched-laptop-variance-global", 5.5),
                new RoundingBatchedLaptopVarianceAdjuster(5.0),
                new PercentBatchedLaptopVarianceAdjuster(-3.9),
            },
            new List<string>(),
            2.3,
            33625.9);
    }

    public double Compute(BatchedLaptopVariance item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedLaptopVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
