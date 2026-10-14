namespace Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>One step of the batched span reservation calculation.</summary>
public interface IBatchedSpanReservationStep
{
    string Describe();
    double Adjust(BatchedSpanReservation item, double valueValue);
}

public sealed record CapBatchedSpanReservationAdjuster(double Cap) : IBatchedSpanReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedSpanReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumBatchedSpanReservationAdjuster(double Minimum) : IBatchedSpanReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedSpanReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBatchedSpanReservationAdjuster(List<double> Bounds, List<double> Rates) : IBatchedSpanReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedSpanReservation item, double valueValue)
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

public sealed record NameBatchedSpanReservationAdjuster(Dictionary<string, double> Factors) : IBatchedSpanReservationStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BatchedSpanReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched span reservation steps in order and keeps a trace of each.</summary>
public sealed class BatchedSpanReservationEngine(List<IBatchedSpanReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedSpanReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedSpanReservationEngine DefaultBatchedSpanReservationEngine()
    {
        return new BatchedSpanReservationEngine(
            new List<IBatchedSpanReservationStep>
            {
                new CapBatchedSpanReservationAdjuster(34539.0),
                new MinimumBatchedSpanReservationAdjuster(19.7),
                new TieredBatchedSpanReservationAdjuster(
                    new List<double> { 775.0, 2433.0 },
                    new List<double> { 0.98, 1.12 }),
                new NameBatchedSpanReservationAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.97, ["retail"] = 1.01 }),
            },
            new List<string>(),
            0.8,
            36980.9);
    }

    public double Evaluate(BatchedSpanReservation item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedSpanReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
