namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default rate draft calculation.</summary>
public interface IDefaultRateDraftFactor
{
    string Describe();
    double Adjust(DefaultRateDraft item, double valueValue);
}

public sealed record FlaggedDefaultRateDraftAdjuster(Registry Flags, string Flag, double Percent) : IDefaultRateDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultRateDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultRateDraftAdjuster(double Cap) : IDefaultRateDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultRateDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDefaultRateDraftAdjuster(double Step) : IDefaultRateDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultRateDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default rate draft steps in order and keeps a trace of each.</summary>
public sealed class DefaultRateDraftEngine(List<IDefaultRateDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultRateDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultRateDraftEngine DefaultDefaultRateDraftEngine(Registry flags)
    {
        return new DefaultRateDraftEngine(
            new List<IDefaultRateDraftFactor>
            {
                new FlaggedDefaultRateDraftAdjuster(flags, "default-rate-draft-adaptive", 5.6),
                new CapDefaultRateDraftAdjuster(15615.0),
                new RoundingDefaultRateDraftAdjuster(1.0),
            },
            new List<string>(),
            0.3,
            97005.8);
    }

    public double Compute(DefaultRateDraft item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultRateDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
