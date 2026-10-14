namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback warranty summary calculation.</summary>
public interface IFallbackWarrantySummaryModifier
{
    string Describe();
    double Adjust(FallbackWarrantySummary item, double valueValue);
}

public sealed record PercentFallbackWarrantySummaryAdjuster(double Percent) : IFallbackWarrantySummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackWarrantySummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackWarrantySummaryAdjuster(double Cap) : IFallbackWarrantySummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackWarrantySummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedFallbackWarrantySummaryAdjuster(Registry Flags, string Flag, double Percent) : IFallbackWarrantySummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackWarrantySummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackWarrantySummaryAdjuster(double Step) : IFallbackWarrantySummaryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackWarrantySummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback warranty summary steps in order and keeps a trace of each.</summary>
public sealed class FallbackWarrantySummaryEngine(List<IFallbackWarrantySummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackWarrantySummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackWarrantySummaryEngine DefaultFallbackWarrantySummaryEngine(Registry flags)
    {
        return new FallbackWarrantySummaryEngine(
            new List<IFallbackWarrantySummaryModifier>
            {
                new PercentFallbackWarrantySummaryAdjuster(9.2),
                new CapFallbackWarrantySummaryAdjuster(48283.0),
                new FlaggedFallbackWarrantySummaryAdjuster(flags, "fallback-warranty-summary-rolling", -0.5),
                new RoundingFallbackWarrantySummaryAdjuster(5.0),
            },
            new List<string>(),
            3.5,
            55682.3);
    }

    public double Compute(FallbackWarrantySummary item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackWarrantySummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
