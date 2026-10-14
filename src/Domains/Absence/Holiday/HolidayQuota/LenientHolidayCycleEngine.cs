namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient holiday cycle calculation.</summary>
public interface ILenientHolidayCycleStep
{
    string Describe();
    double Adjust(LenientHolidayCycle item, double valueValue);
}

public sealed record CapLenientHolidayCycleAdjuster(double Cap) : ILenientHolidayCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientHolidayCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientHolidayCycleAdjuster(double Percent) : ILenientHolidayCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientHolidayCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientHolidayCycleAmountOffsetAdjuster(double Weight) : ILenientHolidayCycleStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LenientHolidayCycle item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredLenientHolidayCycleAdjuster(List<double> Bounds, List<double> Rates) : ILenientHolidayCycleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientHolidayCycle item, double valueValue)
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

public sealed record RoundingLenientHolidayCycleAdjuster(double Step) : ILenientHolidayCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientHolidayCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientHolidayCycleAdjuster(Registry Flags, string Flag, double Percent) : ILenientHolidayCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientHolidayCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient holiday cycle steps in order and keeps a trace of each.</summary>
public sealed class LenientHolidayCycleEngine(List<ILenientHolidayCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientHolidayCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientHolidayCycleEngine DefaultLenientHolidayCycleEngine(Registry flags)
    {
        return new LenientHolidayCycleEngine(
            new List<ILenientHolidayCycleStep>
            {
                new CapLenientHolidayCycleAdjuster(18780.0),
                new PercentLenientHolidayCycleAdjuster(5.9),
                new LenientHolidayCycleAmountOffsetAdjuster(1.09),
                new TieredLenientHolidayCycleAdjuster(
                    new List<double> { 92.0, 5907.0, 6871.0, 9089.0 },
                    new List<double> { 0.8, 1.2, 1.2, 1.09 }),
                new RoundingLenientHolidayCycleAdjuster(0.05),
                new FlaggedLenientHolidayCycleAdjuster(flags, "lenient-holiday-cycle-manual", -2.5),
            },
            new List<string>(),
            3.1,
            42533.6);
    }

    public double Price(LenientHolidayCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientHolidayCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
