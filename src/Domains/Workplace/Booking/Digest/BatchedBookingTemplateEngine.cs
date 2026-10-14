namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched booking template calculation.</summary>
public interface IBatchedBookingTemplateAdjuster
{
    string Describe();
    double Adjust(BatchedBookingTemplate item, double valueValue);
}

public sealed record RoundingBatchedBookingTemplateAdjuster(double Step) : IBatchedBookingTemplateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedBookingTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBatchedBookingTemplateAdjuster(double Percent) : IBatchedBookingTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedBookingTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedBookingTemplateAdjuster(Registry Flags, string Flag, double Percent) : IBatchedBookingTemplateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedBookingTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched booking template steps in order and keeps a trace of each.</summary>
public sealed class BatchedBookingTemplateEngine(List<IBatchedBookingTemplateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedBookingTemplateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedBookingTemplateEngine DefaultBatchedBookingTemplateEngine(Registry flags)
    {
        return new BatchedBookingTemplateEngine(
            new List<IBatchedBookingTemplateAdjuster>
            {
                new RoundingBatchedBookingTemplateAdjuster(5.0),
                new PercentBatchedBookingTemplateAdjuster(16.3),
                new FlaggedBatchedBookingTemplateAdjuster(flags, "batched-booking-template-fallback", 9.3),
            },
            new List<string>(),
            2.1,
            33087.0);
    }

    public double Price(BatchedBookingTemplate item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedBookingTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
