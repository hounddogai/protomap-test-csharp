namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled desk summary calculation.</summary>
public interface IScheduledDeskSummaryFactor
{
    string Describe();
    double Adjust(ScheduledDeskSummary item, double valueValue);
}

public sealed record TitleScheduledDeskSummaryAdjuster(Dictionary<string, double> Factors) : IScheduledDeskSummaryFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScheduledDeskSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ScheduledDeskSummaryQuantityOffsetAdjuster(double Weight) : IScheduledDeskSummaryFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(ScheduledDeskSummary item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record FlaggedScheduledDeskSummaryAdjuster(Registry Flags, string Flag, double Percent) : IScheduledDeskSummaryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledDeskSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentScheduledDeskSummaryAdjuster(double Percent) : IScheduledDeskSummaryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledDeskSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled desk summary steps in order and keeps a trace of each.</summary>
public sealed class ScheduledDeskSummaryEngine(List<IScheduledDeskSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledDeskSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledDeskSummaryEngine DefaultScheduledDeskSummaryEngine(Registry flags)
    {
        return new ScheduledDeskSummaryEngine(
            new List<IScheduledDeskSummaryFactor>
            {
                new TitleScheduledDeskSummaryAdjuster(new Dictionary<string, double> { ["online"] = 1.06 }),
                new ScheduledDeskSummaryQuantityOffsetAdjuster(0.23),
                new FlaggedScheduledDeskSummaryAdjuster(flags, "scheduled-desk-summary-primary", 5.5),
                new PercentScheduledDeskSummaryAdjuster(-1.3),
            },
            new List<string>(),
            5.0,
            94358.6);
    }

    public double Price(ScheduledDeskSummary item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledDeskSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
