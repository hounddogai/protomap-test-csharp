namespace Acme.Workforce.Absence.Holiday.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday quota calculation.</summary>
public interface IHolidayQuotaAdjuster
{
    string Describe();
    double Adjust(HolidayQuota item, double valueValue);
}

public sealed record RoundingHolidayQuotaAdjuster(double Step) : IHolidayQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumHolidayQuotaAdjuster(double Minimum) : IHolidayQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameHolidayQuotaAdjuster(Dictionary<string, double> Factors) : IHolidayQuotaAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(HolidayQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentHolidayQuotaAdjuster(double Percent) : IHolidayQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedHolidayQuotaAdjuster(Registry Flags, string Flag, double Percent) : IHolidayQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHolidayQuotaAdjuster(double Cap) : IHolidayQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the holiday quota steps in order and keeps a trace of each.</summary>
public sealed class HolidayQuotaEngine(List<IHolidayQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayQuotaEngine DefaultHolidayQuotaEngine(Registry flags)
    {
        return new HolidayQuotaEngine(
            new List<IHolidayQuotaAdjuster>
            {
                new RoundingHolidayQuotaAdjuster(0.1),
                new MinimumHolidayQuotaAdjuster(2.1),
                new NameHolidayQuotaAdjuster(new Dictionary<string, double> { ["west"] = 1.02, ["central"] = 1.35 }),
                new PercentHolidayQuotaAdjuster(12.5),
                new FlaggedHolidayQuotaAdjuster(flags, "holiday-quota-capped", -8.3),
                new CapHolidayQuotaAdjuster(2138.0),
            },
            new List<string>(),
            3.5,
            5501.9);
    }

    public double Evaluate(HolidayQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<HolidayQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
