namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional holiday window calculation.</summary>
public interface IRegionalHolidayWindowStep
{
    string Describe();
    double Adjust(RegionalHolidayWindow item, double valueValue);
}

public sealed record PercentRegionalHolidayWindowAdjuster(double Percent) : IRegionalHolidayWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalHolidayWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalHolidayWindowAdjuster(double Step) : IRegionalHolidayWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalHolidayWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalHolidayWindowAdjuster(List<double> Bounds, List<double> Rates) : IRegionalHolidayWindowStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalHolidayWindow item, double valueValue)
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

public sealed record FlaggedRegionalHolidayWindowAdjuster(Registry Flags, string Flag, double Percent) : IRegionalHolidayWindowStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalHolidayWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional holiday window steps in order and keeps a trace of each.</summary>
public sealed class RegionalHolidayWindowEngine(List<IRegionalHolidayWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalHolidayWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalHolidayWindowEngine DefaultRegionalHolidayWindowEngine(Registry flags)
    {
        return new RegionalHolidayWindowEngine(
            new List<IRegionalHolidayWindowStep>
            {
                new PercentRegionalHolidayWindowAdjuster(21.8),
                new RoundingRegionalHolidayWindowAdjuster(0.05),
                new TieredRegionalHolidayWindowAdjuster(
                    new List<double> { 2469.0, 3562.0, 3714.0 },
                    new List<double> { 1.17, 0.99, 0.9 }),
                new FlaggedRegionalHolidayWindowAdjuster(flags, "regional-holiday-window-regional", 7.2),
            },
            new List<string>(),
            3.9,
            86678.2);
    }

    public double Evaluate(RegionalHolidayWindow item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalHolidayWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
