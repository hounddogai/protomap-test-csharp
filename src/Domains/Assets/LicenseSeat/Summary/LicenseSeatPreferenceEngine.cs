namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat preference calculation.</summary>
public interface ILicenseSeatPreferenceStep
{
    string Describe();
    double Adjust(LicenseSeatPreference item, double valueValue);
}

public sealed record TieredLicenseSeatPreferenceAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatPreference item, double valueValue)
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

public sealed record PercentLicenseSeatPreferenceAdjuster(double Percent) : ILicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLicenseSeatPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleLicenseSeatPreferenceAdjuster(Dictionary<string, double> Factors) : ILicenseSeatPreferenceStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LicenseSeatPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLicenseSeatPreferenceAdjuster(double Minimum) : ILicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LicenseSeatPreferenceHoursOffsetAdjuster(double Weight) : ILicenseSeatPreferenceStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LicenseSeatPreference item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the license seat preference steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatPreferenceEngine(List<ILicenseSeatPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatPreferenceEngine DefaultLicenseSeatPreferenceEngine(Registry flags)
    {
        return new LicenseSeatPreferenceEngine(
            new List<ILicenseSeatPreferenceStep>
            {
                new TieredLicenseSeatPreferenceAdjuster(
                    new List<double> { 934.0, 2143.0, 9388.0 },
                    new List<double> { 0.91, 0.91, 1.05 }),
                new PercentLicenseSeatPreferenceAdjuster(-1.0),
                new FlaggedLicenseSeatPreferenceAdjuster(flags, "license-seat-preference-rolling", 6.9),
                new TitleLicenseSeatPreferenceAdjuster(new Dictionary<string, double> { ["east"] = 1.1 }),
                new MinimumLicenseSeatPreferenceAdjuster(14.9),
                new LicenseSeatPreferenceHoursOffsetAdjuster(1.45),
            },
            new List<string>(),
            3.0,
            7245.5);
    }

    public double Run(LicenseSeatPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
