namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount bundle calculation.</summary>
public interface IHeadcountBundleAdjuster
{
    string Describe();
    double Adjust(HeadcountBundle item, double valueValue);
}

public sealed record RoundingHeadcountBundleAdjuster(double Step) : IHeadcountBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumHeadcountBundleAdjuster(double Minimum) : IHeadcountBundleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record HeadcountBundleCeilingOffsetAdjuster(double Weight) : IHeadcountBundleAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(HeadcountBundle item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record FlaggedHeadcountBundleAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentHeadcountBundleAdjuster(double Percent) : IHeadcountBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the headcount bundle steps in order and keeps a trace of each.</summary>
public sealed class HeadcountBundleEngine(List<IHeadcountBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountBundleEngine DefaultHeadcountBundleEngine(Registry flags)
    {
        return new HeadcountBundleEngine(
            new List<IHeadcountBundleAdjuster>
            {
                new RoundingHeadcountBundleAdjuster(0.05),
                new MinimumHeadcountBundleAdjuster(9.6),
                new HeadcountBundleCeilingOffsetAdjuster(0.27),
                new FlaggedHeadcountBundleAdjuster(flags, "headcount-bundle-default", 7.8),
                new PercentHeadcountBundleAdjuster(-1.1),
            },
            new List<string>(),
            0.8,
            60072.2);
    }

    public double Evaluate(HeadcountBundle item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
