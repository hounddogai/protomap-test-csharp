namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal span schedule calculation.</summary>
public interface ISeasonalSpanScheduleAdjuster
{
    string Describe();
    double Adjust(SeasonalSpanSchedule item, double valueValue);
}

public sealed record RegionSeasonalSpanScheduleAdjuster(Dictionary<string, double> Factors) : ISeasonalSpanScheduleAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(SeasonalSpanSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSeasonalSpanScheduleAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalSpanSchedule item, double valueValue)
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

public sealed record CapSeasonalSpanScheduleAdjuster(double Cap) : ISeasonalSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalSpanSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSeasonalSpanScheduleAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalSpanSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalSpanScheduleMarginOffsetAdjuster(double Weight) : ISeasonalSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(SeasonalSpanSchedule item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record MinimumSeasonalSpanScheduleAdjuster(double Minimum) : ISeasonalSpanScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalSpanSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal span schedule steps in order and keeps a trace of each.</summary>
public sealed class SeasonalSpanScheduleEngine(List<ISeasonalSpanScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalSpanScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalSpanScheduleEngine DefaultSeasonalSpanScheduleEngine(Registry flags)
    {
        return new SeasonalSpanScheduleEngine(
            new List<ISeasonalSpanScheduleAdjuster>
            {
                new RegionSeasonalSpanScheduleAdjuster(new Dictionary<string, double> { ["online"] = 1.16 }),
                new TieredSeasonalSpanScheduleAdjuster(
                    new List<double> { 1301.0, 4300.0, 8536.0 },
                    new List<double> { 0.96, 0.99, 1.05 }),
                new CapSeasonalSpanScheduleAdjuster(37787.0),
                new FlaggedSeasonalSpanScheduleAdjuster(flags, "seasonal-span-schedule-lenient", -1.7),
                new SeasonalSpanScheduleMarginOffsetAdjuster(1.18),
                new MinimumSeasonalSpanScheduleAdjuster(26.2),
            },
            new List<string>(),
            0.2,
            98969.0);
    }

    public double Evaluate(SeasonalSpanSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalSpanSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
