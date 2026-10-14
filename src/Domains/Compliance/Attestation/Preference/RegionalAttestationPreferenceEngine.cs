namespace Acme.Workforce.Compliance.Attestation.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional attestation preference calculation.</summary>
public interface IRegionalAttestationPreferenceFactor
{
    string Describe();
    double Adjust(RegionalAttestationPreference item, double valueValue);
}

public sealed record RoundingRegionalAttestationPreferenceAdjuster(double Step) : IRegionalAttestationPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalAttestationPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRegionalAttestationPreferenceAdjuster(double Minimum) : IRegionalAttestationPreferenceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalAttestationPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRegionalAttestationPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IRegionalAttestationPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalAttestationPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRegionalAttestationPreferenceAdjuster(double Percent) : IRegionalAttestationPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalAttestationPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional attestation preference steps in order and keeps a trace of each.</summary>
public sealed class RegionalAttestationPreferenceEngine(List<IRegionalAttestationPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAttestationPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAttestationPreferenceEngine DefaultRegionalAttestationPreferenceEngine(Registry flags)
    {
        return new RegionalAttestationPreferenceEngine(
            new List<IRegionalAttestationPreferenceFactor>
            {
                new RoundingRegionalAttestationPreferenceAdjuster(0.05),
                new MinimumRegionalAttestationPreferenceAdjuster(6.4),
                new FlaggedRegionalAttestationPreferenceAdjuster(
                    flags,
                    "regional-attestation-preference-global",
                    -8.7),
                new PercentRegionalAttestationPreferenceAdjuster(14.6),
            },
            new List<string>(),
            1.6,
            25354.0);
    }

    public double Price(RegionalAttestationPreference item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalAttestationPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
