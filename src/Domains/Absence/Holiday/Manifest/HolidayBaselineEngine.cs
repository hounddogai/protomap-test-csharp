namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday baseline calculation.</summary>
public interface IHolidayBaselineAdjuster
{
    string Describe();
    double Adjust(HolidayBaseline item, double valueValue);
}

public sealed record TeamHolidayBaselineAdjuster(Dictionary<string, double> Factors) : IHolidayBaselineAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(HolidayBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedHolidayBaselineAdjuster(Registry Flags, string Flag, double Percent) : IHolidayBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHolidayBaselineAdjuster(double Cap) : IHolidayBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidayBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingHolidayBaselineAdjuster(double Step) : IHolidayBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredHolidayBaselineAdjuster(List<double> Bounds, List<double> Rates) : IHolidayBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayBaseline item, double valueValue)
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

/// <summary>Runs the holiday baseline steps in order and keeps a trace of each.</summary>
public sealed class HolidayBaselineEngine(List<IHolidayBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayBaselineEngine DefaultHolidayBaselineEngine(Registry flags)
    {
        return new HolidayBaselineEngine(
            new List<IHolidayBaselineAdjuster>
            {
                new TeamHolidayBaselineAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.98, ["west"] = 1.04, ["south"] = 1.35 }),
                new FlaggedHolidayBaselineAdjuster(flags, "holiday-baseline-strict", -2.1),
                new CapHolidayBaselineAdjuster(21827.0),
                new RoundingHolidayBaselineAdjuster(0.05),
                new TieredHolidayBaselineAdjuster(
                    new List<double> { 539.0, 970.0, 7055.0 },
                    new List<double> { 1.01, 0.82, 0.84 }),
            },
            new List<string>(),
            0.2,
            48001.3);
    }

    public double Evaluate(HolidayBaseline item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
