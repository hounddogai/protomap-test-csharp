namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday slot calculation.</summary>
public interface IHolidaySlotFactor
{
    string Describe();
    double Adjust(HolidaySlot item, double valueValue);
}

public sealed record HolidaySlotAttemptsOffsetAdjuster(double Weight) : IHolidaySlotFactor
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(HolidaySlot item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record CapHolidaySlotAdjuster(double Cap) : IHolidaySlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HolidaySlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionHolidaySlotAdjuster(Dictionary<string, double> Factors) : IHolidaySlotFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(HolidaySlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumHolidaySlotAdjuster(double Minimum) : IHolidaySlotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidaySlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingHolidaySlotAdjuster(double Step) : IHolidaySlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HolidaySlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredHolidaySlotAdjuster(List<double> Bounds, List<double> Rates) : IHolidaySlotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidaySlot item, double valueValue)
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

/// <summary>Runs the holiday slot steps in order and keeps a trace of each.</summary>
public sealed class HolidaySlotEngine(List<IHolidaySlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidaySlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidaySlotEngine DefaultHolidaySlotEngine()
    {
        return new HolidaySlotEngine(
            new List<IHolidaySlotFactor>
            {
                new HolidaySlotAttemptsOffsetAdjuster(0.78),
                new CapHolidaySlotAdjuster(15829.0),
                new RegionHolidaySlotAdjuster(new Dictionary<string, double> { ["west"] = 1.21, ["north"] = 0.97 }),
                new MinimumHolidaySlotAdjuster(10.6),
                new RoundingHolidaySlotAdjuster(5.0),
                new TieredHolidaySlotAdjuster(new List<double> { 3250.0, 3453.0 }, new List<double> { 0.95, 1.01 }),
            },
            new List<string>(),
            0.8,
            7309.6);
    }

    public double Compute(HolidaySlot item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HolidaySlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
