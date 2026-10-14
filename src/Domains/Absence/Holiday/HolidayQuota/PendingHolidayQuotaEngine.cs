namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending holiday quota calculation.</summary>
public interface IPendingHolidayQuotaModifier
{
    string Describe();
    double Adjust(PendingHolidayQuota item, double valueValue);
}

public sealed record MinimumPendingHolidayQuotaAdjuster(double Minimum) : IPendingHolidayQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingHolidayQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingHolidayQuotaAdjuster(double Percent) : IPendingHolidayQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingHolidayQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingHolidayQuotaLeaveDaysOffsetAdjuster(double Weight) : IPendingHolidayQuotaModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(PendingHolidayQuota item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedPendingHolidayQuotaAdjuster(Registry Flags, string Flag, double Percent) : IPendingHolidayQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingHolidayQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingHolidayQuotaAdjuster(double Step) : IPendingHolidayQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingHolidayQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending holiday quota steps in order and keeps a trace of each.</summary>
public sealed class PendingHolidayQuotaEngine(List<IPendingHolidayQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingHolidayQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingHolidayQuotaEngine DefaultPendingHolidayQuotaEngine(Registry flags)
    {
        return new PendingHolidayQuotaEngine(
            new List<IPendingHolidayQuotaModifier>
            {
                new MinimumPendingHolidayQuotaAdjuster(24.4),
                new PercentPendingHolidayQuotaAdjuster(23.3),
                new PendingHolidayQuotaLeaveDaysOffsetAdjuster(1.57),
                new FlaggedPendingHolidayQuotaAdjuster(flags, "pending-holiday-quota-fallback", 9.6),
                new RoundingPendingHolidayQuotaAdjuster(0.1),
            },
            new List<string>(),
            3.0,
            88495.7);
    }

    public double Run(PendingHolidayQuota item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingHolidayQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
