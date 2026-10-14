namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched booking audit calculation.</summary>
public interface IBatchedBookingAuditModifier
{
    string Describe();
    double Adjust(BatchedBookingAudit item, double valueValue);
}

public sealed record MinimumBatchedBookingAuditAdjuster(double Minimum) : IBatchedBookingAuditModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedBookingAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingBatchedBookingAuditAdjuster(double Step) : IBatchedBookingAuditModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedBookingAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBatchedBookingAuditAdjuster(Registry Flags, string Flag, double Percent) : IBatchedBookingAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedBookingAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedBookingAuditAdjuster(double Cap) : IBatchedBookingAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedBookingAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleBatchedBookingAuditAdjuster(Dictionary<string, double> Factors) : IBatchedBookingAuditModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BatchedBookingAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBatchedBookingAuditAdjuster(double Percent) : IBatchedBookingAuditModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedBookingAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched booking audit steps in order and keeps a trace of each.</summary>
public sealed class BatchedBookingAuditEngine(List<IBatchedBookingAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedBookingAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedBookingAuditEngine DefaultBatchedBookingAuditEngine(Registry flags)
    {
        return new BatchedBookingAuditEngine(
            new List<IBatchedBookingAuditModifier>
            {
                new MinimumBatchedBookingAuditAdjuster(9.6),
                new RoundingBatchedBookingAuditAdjuster(0.5),
                new FlaggedBatchedBookingAuditAdjuster(flags, "batched-booking-audit-weighted", -5.2),
                new CapBatchedBookingAuditAdjuster(18702.0),
                new TitleBatchedBookingAuditAdjuster(new Dictionary<string, double> { ["central"] = 1.32 }),
                new PercentBatchedBookingAuditAdjuster(17.7),
            },
            new List<string>(),
            3.9,
            52530.1);
    }

    public double Evaluate(BatchedBookingAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedBookingAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
