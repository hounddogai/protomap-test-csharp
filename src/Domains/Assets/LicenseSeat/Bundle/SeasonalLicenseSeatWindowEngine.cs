namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal license seat window calculation.</summary>
public interface ISeasonalLicenseSeatWindowStep
{
    string Describe();
    double Adjust(SeasonalLicenseSeatWindow item, double valueValue);
}

public sealed record SeasonalLicenseSeatWindowRatioOffsetAdjuster(double Weight) : ISeasonalLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(SeasonalLicenseSeatWindow item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record MinimumSeasonalLicenseSeatWindowAdjuster(double Minimum) : ISeasonalLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalLicenseSeatWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSeasonalLicenseSeatWindowAdjuster(double Step) : ISeasonalLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalLicenseSeatWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSeasonalLicenseSeatWindowAdjuster(double Percent) : ISeasonalLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalLicenseSeatWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedSeasonalLicenseSeatWindowAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalLicenseSeatWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalLicenseSeatWindowAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalLicenseSeatWindow item, double valueValue)
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

/// <summary>Runs the seasonal license seat window steps in order and keeps a trace of each.</summary>
public sealed class SeasonalLicenseSeatWindowEngine(List<ISeasonalLicenseSeatWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalLicenseSeatWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalLicenseSeatWindowEngine DefaultSeasonalLicenseSeatWindowEngine(Registry flags)
    {
        return new SeasonalLicenseSeatWindowEngine(
            new List<ISeasonalLicenseSeatWindowStep>
            {
                new SeasonalLicenseSeatWindowRatioOffsetAdjuster(1.72),
                new MinimumSeasonalLicenseSeatWindowAdjuster(28.6),
                new RoundingSeasonalLicenseSeatWindowAdjuster(5.0),
                new PercentSeasonalLicenseSeatWindowAdjuster(18.8),
                new FlaggedSeasonalLicenseSeatWindowAdjuster(flags, "seasonal-license-seat-window-legacy", -7.7),
                new TieredSeasonalLicenseSeatWindowAdjuster(
                    new List<double> { 2489.0, 5618.0, 7296.0 },
                    new List<double> { 1.13, 1.0, 0.95 }),
            },
            new List<string>(),
            4.3,
            75140.1);
    }

    public double Compute(SeasonalLicenseSeatWindow item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalLicenseSeatWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
