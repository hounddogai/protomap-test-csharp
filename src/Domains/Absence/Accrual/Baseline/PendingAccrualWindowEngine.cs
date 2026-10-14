namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending accrual window calculation.</summary>
public interface IPendingAccrualWindowStep
{
    string Describe();
    double Adjust(PendingAccrualWindow item, double valueValue);
}

public sealed record CategoryPendingAccrualWindowAdjuster(Dictionary<string, double> Factors) : IPendingAccrualWindowStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PendingAccrualWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingAccrualWindowAdjuster(double Percent) : IPendingAccrualWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingAccrualWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingAccrualWindowStartsAtOffsetAdjuster(double Weight) : IPendingAccrualWindowStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(PendingAccrualWindow item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record FlaggedPendingAccrualWindowAdjuster(Registry Flags, string Flag, double Percent) : IPendingAccrualWindowStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingAccrualWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending accrual window steps in order and keeps a trace of each.</summary>
public sealed class PendingAccrualWindowEngine(List<IPendingAccrualWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingAccrualWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingAccrualWindowEngine DefaultPendingAccrualWindowEngine(Registry flags)
    {
        return new PendingAccrualWindowEngine(
            new List<IPendingAccrualWindowStep>
            {
                new CategoryPendingAccrualWindowAdjuster(new Dictionary<string, double> { ["south"] = 1.34 }),
                new PercentPendingAccrualWindowAdjuster(-1.8),
                new PendingAccrualWindowStartsAtOffsetAdjuster(0.62),
                new FlaggedPendingAccrualWindowAdjuster(flags, "pending-accrual-window-fallback", 0.4),
            },
            new List<string>(),
            4.4,
            23100.9);
    }

    public double Compute(PendingAccrualWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingAccrualWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
