namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the batched policy ack reservation calculation.</summary>
public interface IBatchedPolicyAckReservationStep
{
    string Describe();
    double Adjust(BatchedPolicyAckReservation item, double valueValue);
}

public sealed record MinimumBatchedPolicyAckReservationAdjuster(double Minimum) : IBatchedPolicyAckReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedPolicyAckReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelBatchedPolicyAckReservationAdjuster(Dictionary<string, double> Factors) : IBatchedPolicyAckReservationStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BatchedPolicyAckReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBatchedPolicyAckReservationAdjuster(double Percent) : IBatchedPolicyAckReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedPolicyAckReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedPolicyAckReservationAdjuster(Registry Flags, string Flag, double Percent) : IBatchedPolicyAckReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedPolicyAckReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedPolicyAckReservationAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPolicyAckReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPolicyAckReservation item, double valueValue)
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

/// <summary>Runs the batched policy ack reservation steps in order and keeps a trace of each.</summary>
public sealed class BatchedPolicyAckReservationEngine(List<IBatchedPolicyAckReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPolicyAckReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPolicyAckReservationEngine DefaultBatchedPolicyAckReservationEngine(Registry flags)
    {
        return new BatchedPolicyAckReservationEngine(
            new List<IBatchedPolicyAckReservationStep>
            {
                new MinimumBatchedPolicyAckReservationAdjuster(24.5),
                new LabelBatchedPolicyAckReservationAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.23, ["online"] = 0.88, ["central"] = 1.4 }),
                new PercentBatchedPolicyAckReservationAdjuster(10.1),
                new FlaggedBatchedPolicyAckReservationAdjuster(flags, "batched-policy-ack-reservation-rolling", -4.5),
                new TieredBatchedPolicyAckReservationAdjuster(
                    new List<double> { 823.0, 1453.0, 5201.0 },
                    new List<double> { 1.13, 0.99, 1.12 }),
            },
            new List<string>(),
            2.5,
            98955.3);
    }

    public double Evaluate(BatchedPolicyAckReservation item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPolicyAckReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
