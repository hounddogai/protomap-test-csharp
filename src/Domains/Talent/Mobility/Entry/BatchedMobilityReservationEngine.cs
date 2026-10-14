namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the batched mobility reservation calculation.</summary>
public interface IBatchedMobilityReservationFactor
{
    string Describe();
    double Adjust(BatchedMobilityReservation item, double valueValue);
}

public sealed record FlaggedBatchedMobilityReservationAdjuster(Registry Flags, string Flag, double Percent) : IBatchedMobilityReservationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedMobilityReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBatchedMobilityReservationAdjuster(double Percent) : IBatchedMobilityReservationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedMobilityReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedMobilityReservationAdjuster(List<double> Bounds, List<double> Rates) : IBatchedMobilityReservationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedMobilityReservation item, double valueValue)
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

public sealed record LocaleBatchedMobilityReservationAdjuster(Dictionary<string, double> Factors) : IBatchedMobilityReservationFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(BatchedMobilityReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapBatchedMobilityReservationAdjuster(double Cap) : IBatchedMobilityReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedMobilityReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched mobility reservation steps in order and keeps a trace of each.</summary>
public sealed class BatchedMobilityReservationEngine(List<IBatchedMobilityReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedMobilityReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedMobilityReservationEngine DefaultBatchedMobilityReservationEngine(Registry flags)
    {
        return new BatchedMobilityReservationEngine(
            new List<IBatchedMobilityReservationFactor>
            {
                new FlaggedBatchedMobilityReservationAdjuster(flags, "batched-mobility-reservation-adaptive", -3.9),
                new PercentBatchedMobilityReservationAdjuster(12.1),
                new TieredBatchedMobilityReservationAdjuster(
                    new List<double> { 4654.0, 5119.0, 8109.0, 8722.0 },
                    new List<double> { 1.18, 0.8, 0.99, 0.88 }),
                new LocaleBatchedMobilityReservationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.35, ["west"] = 1.27 }),
                new CapBatchedMobilityReservationAdjuster(23907.0),
            },
            new List<string>(),
            2.6,
            34601.2);
    }

    public double Evaluate(BatchedMobilityReservation item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedMobilityReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
