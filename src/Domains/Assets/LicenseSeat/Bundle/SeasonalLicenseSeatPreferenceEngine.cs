namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal license seat preference calculation.</summary>
public interface ISeasonalLicenseSeatPreferenceStep
{
    string Describe();
    double Adjust(SeasonalLicenseSeatPreference item, double valueValue);
}

public sealed record RoundingSeasonalLicenseSeatPreferenceAdjuster(double Step) : ISeasonalLicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalLicenseSeatPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSeasonalLicenseSeatPreferenceAdjuster(double Minimum) : ISeasonalLicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalLicenseSeatPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSeasonalLicenseSeatPreferenceAdjuster(double Cap) : ISeasonalLicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalLicenseSeatPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSeasonalLicenseSeatPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalLicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalLicenseSeatPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal license seat preference steps in order and keeps a trace of each.</summary>
public sealed class SeasonalLicenseSeatPreferenceEngine(List<ISeasonalLicenseSeatPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalLicenseSeatPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalLicenseSeatPreferenceEngine DefaultSeasonalLicenseSeatPreferenceEngine(Registry flags)
    {
        return new SeasonalLicenseSeatPreferenceEngine(
            new List<ISeasonalLicenseSeatPreferenceStep>
            {
                new RoundingSeasonalLicenseSeatPreferenceAdjuster(0.05),
                new MinimumSeasonalLicenseSeatPreferenceAdjuster(35.2),
                new CapSeasonalLicenseSeatPreferenceAdjuster(40821.0),
                new FlaggedSeasonalLicenseSeatPreferenceAdjuster(
                    flags,
                    "seasonal-license-seat-preference-scheduled",
                    -9.5),
            },
            new List<string>(),
            2.4,
            54950.3);
    }

    public double Run(SeasonalLicenseSeatPreference item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalLicenseSeatPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
