namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment schedule calculation.</summary>
public interface IGarnishmentScheduleModifier
{
    string Describe();
    double Adjust(GarnishmentSchedule item, double valueValue);
}

public sealed record GarnishmentScheduleCostOffsetAdjuster(double Weight) : IGarnishmentScheduleModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(GarnishmentSchedule item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record MinimumGarnishmentScheduleAdjuster(double Minimum) : IGarnishmentScheduleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedGarnishmentScheduleAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentScheduleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentScheduleAdjuster(double Cap) : IGarnishmentScheduleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamGarnishmentScheduleAdjuster(Dictionary<string, double> Factors) : IGarnishmentScheduleModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(GarnishmentSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredGarnishmentScheduleAdjuster(List<double> Bounds, List<double> Rates) : IGarnishmentScheduleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GarnishmentSchedule item, double valueValue)
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

/// <summary>Runs the garnishment schedule steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentScheduleEngine(List<IGarnishmentScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentScheduleEngine DefaultGarnishmentScheduleEngine(Registry flags)
    {
        return new GarnishmentScheduleEngine(
            new List<IGarnishmentScheduleModifier>
            {
                new GarnishmentScheduleCostOffsetAdjuster(0.39),
                new MinimumGarnishmentScheduleAdjuster(40.9),
                new FlaggedGarnishmentScheduleAdjuster(flags, "garnishment-schedule-weighted", -6.2),
                new CapGarnishmentScheduleAdjuster(48216.0),
                new TeamGarnishmentScheduleAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.35, ["south"] = 1.3, ["online"] = 1.16 }),
                new TieredGarnishmentScheduleAdjuster(
                    new List<double> { 1199.0, 1395.0, 5345.0 },
                    new List<double> { 1.03, 0.87, 0.97 }),
            },
            new List<string>(),
            2.3,
            72566.8);
    }

    public double Price(GarnishmentSchedule item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
