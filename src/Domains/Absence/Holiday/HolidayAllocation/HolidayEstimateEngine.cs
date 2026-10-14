namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday estimate calculation.</summary>
public interface IHolidayEstimateAdjuster
{
    string Describe();
    double Adjust(HolidayEstimate item, double valueValue);
}

public sealed record HolidayEstimateFloorValueOffsetAdjuster(double Weight) : IHolidayEstimateAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(HolidayEstimate item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record FlaggedHolidayEstimateAdjuster(Registry Flags, string Flag, double Percent) : IHolidayEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentHolidayEstimateAdjuster(double Percent) : IHolidayEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredHolidayEstimateAdjuster(List<double> Bounds, List<double> Rates) : IHolidayEstimateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayEstimate item, double valueValue)
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

public sealed record RoundingHolidayEstimateAdjuster(double Step) : IHolidayEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidayEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the holiday estimate steps in order and keeps a trace of each.</summary>
public sealed class HolidayEstimateEngine(List<IHolidayEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayEstimateEngine DefaultHolidayEstimateEngine(Registry flags)
    {
        return new HolidayEstimateEngine(
            new List<IHolidayEstimateAdjuster>
            {
                new HolidayEstimateFloorValueOffsetAdjuster(1.98),
                new FlaggedHolidayEstimateAdjuster(flags, "holiday-estimate-adaptive", -3.5),
                new PercentHolidayEstimateAdjuster(19.9),
                new TieredHolidayEstimateAdjuster(
                    new List<double> { 8425.0, 8473.0 },
                    new List<double> { 0.93, 0.91 }),
                new RoundingHolidayEstimateAdjuster(5.0),
            },
            new List<string>(),
            2.6,
            17253.2);
    }

    public double Compute(HolidayEstimate item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
