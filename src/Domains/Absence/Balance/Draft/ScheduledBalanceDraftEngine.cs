namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled balance draft calculation.</summary>
public interface IScheduledBalanceDraftAdjuster
{
    string Describe();
    double Adjust(ScheduledBalanceDraft item, double valueValue);
}

public sealed record ScheduledBalanceDraftTenureMonthsOffsetAdjuster(double Weight) : IScheduledBalanceDraftAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(ScheduledBalanceDraft item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedScheduledBalanceDraftAdjuster(Registry Flags, string Flag, double Percent) : IScheduledBalanceDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledBalanceDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledBalanceDraftAdjuster(List<double> Bounds, List<double> Rates) : IScheduledBalanceDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledBalanceDraft item, double valueValue)
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

public sealed record RoundingScheduledBalanceDraftAdjuster(double Step) : IScheduledBalanceDraftAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledBalanceDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapScheduledBalanceDraftAdjuster(double Cap) : IScheduledBalanceDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledBalanceDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledBalanceDraftAdjuster(double Minimum) : IScheduledBalanceDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledBalanceDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled balance draft steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBalanceDraftEngine(List<IScheduledBalanceDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBalanceDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBalanceDraftEngine DefaultScheduledBalanceDraftEngine(Registry flags)
    {
        return new ScheduledBalanceDraftEngine(
            new List<IScheduledBalanceDraftAdjuster>
            {
                new ScheduledBalanceDraftTenureMonthsOffsetAdjuster(1.54),
                new FlaggedScheduledBalanceDraftAdjuster(flags, "scheduled-balance-draft-batched", -9.1),
                new TieredScheduledBalanceDraftAdjuster(
                    new List<double> { 3410.0, 7798.0 },
                    new List<double> { 1.2, 0.85 }),
                new RoundingScheduledBalanceDraftAdjuster(0.1),
                new CapScheduledBalanceDraftAdjuster(12458.0),
                new MinimumScheduledBalanceDraftAdjuster(25.1),
            },
            new List<string>(),
            3.5,
            52592.4);
    }

    public double Evaluate(ScheduledBalanceDraft item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBalanceDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
