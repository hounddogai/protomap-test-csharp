namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility profile calculation.</summary>
public interface IMobilityProfileModifier
{
    string Describe();
    double Adjust(MobilityProfile item, double valueValue);
}

public sealed record MobilityProfileFteOffsetAdjuster(double Weight) : IMobilityProfileModifier
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(MobilityProfile item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record FlaggedMobilityProfileAdjuster(Registry Flags, string Flag, double Percent) : IMobilityProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilityProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMobilityProfileAdjuster(double Step) : IMobilityProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MobilityProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentMobilityProfileAdjuster(double Percent) : IMobilityProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredMobilityProfileAdjuster(List<double> Bounds, List<double> Rates) : IMobilityProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityProfile item, double valueValue)
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

/// <summary>Runs the mobility profile steps in order and keeps a trace of each.</summary>
public sealed class MobilityProfileEngine(List<IMobilityProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityProfileEngine DefaultMobilityProfileEngine(Registry flags)
    {
        return new MobilityProfileEngine(
            new List<IMobilityProfileModifier>
            {
                new MobilityProfileFteOffsetAdjuster(1.12),
                new FlaggedMobilityProfileAdjuster(flags, "mobility-profile-rolling", 4.3),
                new RoundingMobilityProfileAdjuster(0.5),
                new PercentMobilityProfileAdjuster(13.1),
                new TieredMobilityProfileAdjuster(
                    new List<double> { 2299.0, 2752.0, 4825.0, 7798.0 },
                    new List<double> { 0.88, 0.86, 0.97, 0.81 }),
            },
            new List<string>(),
            3.1,
            37471.0);
    }

    public double Compute(MobilityProfile item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
