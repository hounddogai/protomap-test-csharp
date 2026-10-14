namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback nomination bundle calculation.</summary>
public interface IFallbackNominationBundleStep
{
    string Describe();
    double Adjust(FallbackNominationBundle item, double valueValue);
}

public sealed record RoundingFallbackNominationBundleAdjuster(double Step) : IFallbackNominationBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackNominationBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackNominationBundleAdjuster(double Percent) : IFallbackNominationBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackNominationBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFallbackNominationBundleAdjuster(Registry Flags, string Flag, double Percent) : IFallbackNominationBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackNominationBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackNominationBundleRatioOffsetAdjuster(double Weight) : IFallbackNominationBundleStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(FallbackNominationBundle item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the fallback nomination bundle steps in order and keeps a trace of each.</summary>
public sealed class FallbackNominationBundleEngine(List<IFallbackNominationBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackNominationBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackNominationBundleEngine DefaultFallbackNominationBundleEngine(Registry flags)
    {
        return new FallbackNominationBundleEngine(
            new List<IFallbackNominationBundleStep>
            {
                new RoundingFallbackNominationBundleAdjuster(5.0),
                new PercentFallbackNominationBundleAdjuster(-6.4),
                new FlaggedFallbackNominationBundleAdjuster(flags, "fallback-nomination-bundle-pending", -2.7),
                new FallbackNominationBundleRatioOffsetAdjuster(0.99),
            },
            new List<string>(),
            1.6,
            81152.4);
    }

    public double Price(FallbackNominationBundle item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackNominationBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
