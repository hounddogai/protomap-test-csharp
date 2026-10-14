namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered leave request item calculation.</summary>
public interface ITieredLeaveRequestItemStep
{
    string Describe();
    double Adjust(TieredLeaveRequestItem item, double valueValue);
}

public sealed record MinimumTieredLeaveRequestItemAdjuster(double Minimum) : ITieredLeaveRequestItemStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredLeaveRequestItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTieredLeaveRequestItemAdjuster(List<double> Bounds, List<double> Rates) : ITieredLeaveRequestItemStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredLeaveRequestItem item, double valueValue)
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

public sealed record FlaggedTieredLeaveRequestItemAdjuster(Registry Flags, string Flag, double Percent) : ITieredLeaveRequestItemStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredLeaveRequestItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered leave request item steps in order and keeps a trace of each.</summary>
public sealed class TieredLeaveRequestItemEngine(List<ITieredLeaveRequestItemStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredLeaveRequestItemStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredLeaveRequestItemEngine DefaultTieredLeaveRequestItemEngine(Registry flags)
    {
        return new TieredLeaveRequestItemEngine(
            new List<ITieredLeaveRequestItemStep>
            {
                new MinimumTieredLeaveRequestItemAdjuster(6.8),
                new TieredTieredLeaveRequestItemAdjuster(
                    new List<double> { 4272.0, 5154.0 },
                    new List<double> { 0.93, 0.86 }),
                new FlaggedTieredLeaveRequestItemAdjuster(flags, "tiered-leave-request-item-adaptive", 2.3),
            },
            new List<string>(),
            1.1,
            24856.6);
    }

    public double Price(TieredLeaveRequestItem item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredLeaveRequestItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
