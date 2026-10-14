namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional holiday adjustment calculation.</summary>
public interface IRegionalHolidayAdjustmentFactor
{
    string Describe();
    double Adjust(RegionalHolidayAdjustment item, double valueValue);
}

public sealed record CapRegionalHolidayAdjustmentAdjuster(double Cap) : IRegionalHolidayAdjustmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalHolidayAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleRegionalHolidayAdjustmentAdjuster(Dictionary<string, double> Factors) : IRegionalHolidayAdjustmentFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RegionalHolidayAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRegionalHolidayAdjustmentAdjuster(double Percent) : IRegionalHolidayAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalHolidayAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalHolidayAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IRegionalHolidayAdjustmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalHolidayAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional holiday adjustment steps in order and keeps a trace of each.</summary>
public sealed class RegionalHolidayAdjustmentEngine(List<IRegionalHolidayAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalHolidayAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalHolidayAdjustmentEngine DefaultRegionalHolidayAdjustmentEngine(Registry flags)
    {
        return new RegionalHolidayAdjustmentEngine(
            new List<IRegionalHolidayAdjustmentFactor>
            {
                new CapRegionalHolidayAdjustmentAdjuster(13389.0),
                new LocaleRegionalHolidayAdjustmentAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.14, ["retail"] = 1.22, ["online"] = 1.35 }),
                new PercentRegionalHolidayAdjustmentAdjuster(-20.4),
                new FlaggedRegionalHolidayAdjustmentAdjuster(flags, "regional-holiday-adjustment-strict", -6.8),
            },
            new List<string>(),
            3.9,
            58865.4);
    }

    public double Evaluate(RegionalHolidayAdjustment item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalHolidayAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
