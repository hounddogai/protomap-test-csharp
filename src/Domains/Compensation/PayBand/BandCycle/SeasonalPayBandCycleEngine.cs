namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal pay band cycle calculation.</summary>
public interface ISeasonalPayBandCycleModifier
{
    string Describe();
    double Adjust(SeasonalPayBandCycle item, double valueValue);
}

public sealed record TieredSeasonalPayBandCycleAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalPayBandCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalPayBandCycle item, double valueValue)
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

public sealed record RoundingSeasonalPayBandCycleAdjuster(double Step) : ISeasonalPayBandCycleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalPayBandCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSeasonalPayBandCycleAdjuster(double Cap) : ISeasonalPayBandCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalPayBandCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SeasonalPayBandCycleGraceDaysOffsetAdjuster(double Weight) : ISeasonalPayBandCycleModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(SeasonalPayBandCycle item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record MinimumSeasonalPayBandCycleAdjuster(double Minimum) : ISeasonalPayBandCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalPayBandCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal pay band cycle steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPayBandCycleEngine(List<ISeasonalPayBandCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPayBandCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPayBandCycleEngine DefaultSeasonalPayBandCycleEngine()
    {
        return new SeasonalPayBandCycleEngine(
            new List<ISeasonalPayBandCycleModifier>
            {
                new TieredSeasonalPayBandCycleAdjuster(
                    new List<double> { 1595.0, 5322.0, 8425.0 },
                    new List<double> { 1.04, 1.13, 0.97 }),
                new RoundingSeasonalPayBandCycleAdjuster(0.05),
                new CapSeasonalPayBandCycleAdjuster(23076.0),
                new SeasonalPayBandCycleGraceDaysOffsetAdjuster(0.95),
                new MinimumSeasonalPayBandCycleAdjuster(13.6),
            },
            new List<string>(),
            4.5,
            87927.0);
    }

    public double Price(SeasonalPayBandCycle item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPayBandCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
