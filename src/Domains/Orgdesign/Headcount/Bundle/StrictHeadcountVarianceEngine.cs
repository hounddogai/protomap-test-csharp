namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the strict headcount variance calculation.</summary>
public interface IStrictHeadcountVarianceFactor
{
    string Describe();
    double Adjust(StrictHeadcountVariance item, double valueValue);
}

public sealed record FlaggedStrictHeadcountVarianceAdjuster(Registry Flags, string Flag, double Percent) : IStrictHeadcountVarianceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictHeadcountVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentStrictHeadcountVarianceAdjuster(double Percent) : IStrictHeadcountVarianceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictHeadcountVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictHeadcountVarianceAdjuster(double Cap) : IStrictHeadcountVarianceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictHeadcountVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumStrictHeadcountVarianceAdjuster(double Minimum) : IStrictHeadcountVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictHeadcountVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict headcount variance steps in order and keeps a trace of each.</summary>
public sealed class StrictHeadcountVarianceEngine(List<IStrictHeadcountVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictHeadcountVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictHeadcountVarianceEngine DefaultStrictHeadcountVarianceEngine(Registry flags)
    {
        return new StrictHeadcountVarianceEngine(
            new List<IStrictHeadcountVarianceFactor>
            {
                new FlaggedStrictHeadcountVarianceAdjuster(flags, "strict-headcount-variance-weighted", -5.0),
                new PercentStrictHeadcountVarianceAdjuster(-23.1),
                new CapStrictHeadcountVarianceAdjuster(5833.0),
                new MinimumStrictHeadcountVarianceAdjuster(41.0),
            },
            new List<string>(),
            0.5,
            41521.0);
    }

    public double Compute(StrictHeadcountVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictHeadcountVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
