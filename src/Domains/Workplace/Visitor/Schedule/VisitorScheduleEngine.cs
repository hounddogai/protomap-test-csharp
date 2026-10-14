namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor schedule calculation.</summary>
public interface IVisitorScheduleStep
{
    string Describe();
    double Adjust(VisitorSchedule item, double valueValue);
}

public sealed record TieredVisitorScheduleAdjuster(List<double> Bounds, List<double> Rates) : IVisitorScheduleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorSchedule item, double valueValue)
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

public sealed record CapVisitorScheduleAdjuster(double Cap) : IVisitorScheduleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingVisitorScheduleAdjuster(double Step) : IVisitorScheduleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentVisitorScheduleAdjuster(Dictionary<string, double> Factors) : IVisitorScheduleStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(VisitorSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumVisitorScheduleAdjuster(double Minimum) : IVisitorScheduleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedVisitorScheduleAdjuster(Registry Flags, string Flag, double Percent) : IVisitorScheduleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the visitor schedule steps in order and keeps a trace of each.</summary>
public sealed class VisitorScheduleEngine(List<IVisitorScheduleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorScheduleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorScheduleEngine DefaultVisitorScheduleEngine(Registry flags)
    {
        return new VisitorScheduleEngine(
            new List<IVisitorScheduleStep>
            {
                new TieredVisitorScheduleAdjuster(
                    new List<double> { 578.0, 5045.0 },
                    new List<double> { 1.18, 0.97 }),
                new CapVisitorScheduleAdjuster(39599.0),
                new RoundingVisitorScheduleAdjuster(0.05),
                new SegmentVisitorScheduleAdjuster(new Dictionary<string, double> { ["west"] = 1.2 }),
                new MinimumVisitorScheduleAdjuster(7.9),
                new FlaggedVisitorScheduleAdjuster(flags, "visitor-schedule-default", 7.2),
            },
            new List<string>(),
            2.3,
            92904.2);
    }

    public double Evaluate(VisitorSchedule item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
