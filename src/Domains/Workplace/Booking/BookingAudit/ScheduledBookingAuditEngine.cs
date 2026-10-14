namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled booking audit calculation.</summary>
public interface IScheduledBookingAuditAdjuster
{
    string Describe();
    double Adjust(ScheduledBookingAudit item, double valueValue);
}

public sealed record TitleScheduledBookingAuditAdjuster(Dictionary<string, double> Factors) : IScheduledBookingAuditAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScheduledBookingAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumScheduledBookingAuditAdjuster(double Minimum) : IScheduledBookingAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledBookingAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScheduledBookingAuditCostOffsetAdjuster(double Weight) : IScheduledBookingAuditAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(ScheduledBookingAudit item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record RoundingScheduledBookingAuditAdjuster(double Step) : IScheduledBookingAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledBookingAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled booking audit steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBookingAuditEngine(List<IScheduledBookingAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBookingAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBookingAuditEngine DefaultScheduledBookingAuditEngine()
    {
        return new ScheduledBookingAuditEngine(
            new List<IScheduledBookingAuditAdjuster>
            {
                new TitleScheduledBookingAuditAdjuster(new Dictionary<string, double> { ["east"] = 1.01 }),
                new MinimumScheduledBookingAuditAdjuster(22.0),
                new ScheduledBookingAuditCostOffsetAdjuster(1.51),
                new RoundingScheduledBookingAuditAdjuster(0.05),
            },
            new List<string>(),
            0.8,
            59696.1);
    }

    public double Run(ScheduledBookingAudit item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBookingAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
