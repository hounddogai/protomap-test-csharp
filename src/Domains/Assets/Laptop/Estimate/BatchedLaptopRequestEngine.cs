namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched laptop request calculation.</summary>
public interface IBatchedLaptopRequestModifier
{
    string Describe();
    double Adjust(BatchedLaptopRequest item, double valueValue);
}

public sealed record RoundingBatchedLaptopRequestAdjuster(double Step) : IBatchedLaptopRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedLaptopRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record BatchedLaptopRequestFloorValueOffsetAdjuster(double Weight) : IBatchedLaptopRequestModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(BatchedLaptopRequest item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumBatchedLaptopRequestAdjuster(double Minimum) : IBatchedLaptopRequestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedLaptopRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBatchedLaptopRequestAdjuster(List<double> Bounds, List<double> Rates) : IBatchedLaptopRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedLaptopRequest item, double valueValue)
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

public sealed record PercentBatchedLaptopRequestAdjuster(double Percent) : IBatchedLaptopRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedLaptopRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedLaptopRequestAdjuster(double Cap) : IBatchedLaptopRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedLaptopRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched laptop request steps in order and keeps a trace of each.</summary>
public sealed class BatchedLaptopRequestEngine(List<IBatchedLaptopRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedLaptopRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedLaptopRequestEngine DefaultBatchedLaptopRequestEngine()
    {
        return new BatchedLaptopRequestEngine(
            new List<IBatchedLaptopRequestModifier>
            {
                new RoundingBatchedLaptopRequestAdjuster(0.05),
                new BatchedLaptopRequestFloorValueOffsetAdjuster(1.46),
                new MinimumBatchedLaptopRequestAdjuster(24.1),
                new TieredBatchedLaptopRequestAdjuster(
                    new List<double> { 1024.0, 2517.0, 3567.0, 6155.0 },
                    new List<double> { 0.92, 1.0, 1.0, 0.99 }),
                new PercentBatchedLaptopRequestAdjuster(-23.3),
                new CapBatchedLaptopRequestAdjuster(800.0),
            },
            new List<string>(),
            4.1,
            59805.2);
    }

    public double Price(BatchedLaptopRequest item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedLaptopRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
