namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary refresh variance calculation.</summary>
public interface IPrimaryRefreshVarianceFactor
{
    string Describe();
    double Adjust(PrimaryRefreshVariance item, double valueValue);
}

public sealed record RoundingPrimaryRefreshVarianceAdjuster(double Step) : IPrimaryRefreshVarianceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryRefreshVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PrimaryRefreshVarianceRevisionOffsetAdjuster(double Weight) : IPrimaryRefreshVarianceFactor
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(PrimaryRefreshVariance item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record LabelPrimaryRefreshVarianceAdjuster(Dictionary<string, double> Factors) : IPrimaryRefreshVarianceFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PrimaryRefreshVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPrimaryRefreshVarianceAdjuster(double Cap) : IPrimaryRefreshVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryRefreshVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPrimaryRefreshVarianceAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryRefreshVarianceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryRefreshVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary refresh variance steps in order and keeps a trace of each.</summary>
public sealed class PrimaryRefreshVarianceEngine(List<IPrimaryRefreshVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryRefreshVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryRefreshVarianceEngine DefaultPrimaryRefreshVarianceEngine(Registry flags)
    {
        return new PrimaryRefreshVarianceEngine(
            new List<IPrimaryRefreshVarianceFactor>
            {
                new RoundingPrimaryRefreshVarianceAdjuster(0.25),
                new PrimaryRefreshVarianceRevisionOffsetAdjuster(1.93),
                new LabelPrimaryRefreshVarianceAdjuster(new Dictionary<string, double> { ["central"] = 0.89 }),
                new CapPrimaryRefreshVarianceAdjuster(3243.0),
                new FlaggedPrimaryRefreshVarianceAdjuster(flags, "primary-refresh-variance-weighted", 6.7),
            },
            new List<string>(),
            0.3,
            26070.5);
    }

    public double Run(PrimaryRefreshVariance item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryRefreshVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
