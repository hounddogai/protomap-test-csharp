namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the seasonal license seat adjustment calculation.</summary>
public interface ISeasonalLicenseSeatAdjustmentStep
{
    string Describe();
    double Adjust(SeasonalLicenseSeatAdjustment item, double valueValue);
}

public sealed record FlaggedSeasonalLicenseSeatAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalLicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalLicenseSeatAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalLicenseSeatAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalLicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalLicenseSeatAdjustment item, double valueValue)
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

public sealed record SourceSeasonalLicenseSeatAdjustmentAdjuster(Dictionary<string, double> Factors) : ISeasonalLicenseSeatAdjustmentStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(SeasonalLicenseSeatAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal license seat adjustment steps in order and keeps a trace of each.</summary>
public sealed class SeasonalLicenseSeatAdjustmentEngine(List<ISeasonalLicenseSeatAdjustmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalLicenseSeatAdjustmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalLicenseSeatAdjustmentEngine DefaultSeasonalLicenseSeatAdjustmentEngine(Registry flags)
    {
        return new SeasonalLicenseSeatAdjustmentEngine(
            new List<ISeasonalLicenseSeatAdjustmentStep>
            {
                new FlaggedSeasonalLicenseSeatAdjustmentAdjuster(
                    flags,
                    "seasonal-license-seat-adjustment-pending",
                    -1.1),
                new TieredSeasonalLicenseSeatAdjustmentAdjuster(
                    new List<double> { 408.0, 2870.0, 3494.0, 6510.0 },
                    new List<double> { 1.01, 1.14, 1.0, 1.19 }),
                new SourceSeasonalLicenseSeatAdjustmentAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.81, ["east"] = 1.05, ["north"] = 0.81 }),
            },
            new List<string>(),
            3.0,
            59383.4);
    }

    public double Price(SeasonalLicenseSeatAdjustment item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalLicenseSeatAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
