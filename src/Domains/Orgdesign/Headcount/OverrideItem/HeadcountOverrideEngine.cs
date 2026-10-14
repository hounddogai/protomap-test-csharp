namespace Acme.Workforce.Orgdesign.Headcount.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount override calculation.</summary>
public interface IHeadcountOverrideFactor
{
    string Describe();
    double Adjust(HeadcountOverride item, double valueValue);
}

public sealed record FlaggedHeadcountOverrideAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumHeadcountOverrideAdjuster(double Minimum) : IHeadcountOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentHeadcountOverrideAdjuster(double Percent) : IHeadcountOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryHeadcountOverrideAdjuster(Dictionary<string, double> Factors) : IHeadcountOverrideFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(HeadcountOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapHeadcountOverrideAdjuster(double Cap) : IHeadcountOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingHeadcountOverrideAdjuster(double Step) : IHeadcountOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the headcount override steps in order and keeps a trace of each.</summary>
public sealed class HeadcountOverrideEngine(List<IHeadcountOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountOverrideEngine DefaultHeadcountOverrideEngine(Registry flags)
    {
        return new HeadcountOverrideEngine(
            new List<IHeadcountOverrideFactor>
            {
                new FlaggedHeadcountOverrideAdjuster(flags, "headcount-override-manual", -8.2),
                new MinimumHeadcountOverrideAdjuster(9.3),
                new PercentHeadcountOverrideAdjuster(3.2),
                new CountryHeadcountOverrideAdjuster(new Dictionary<string, double> { ["central"] = 0.94 }),
                new CapHeadcountOverrideAdjuster(47330.0),
                new RoundingHeadcountOverrideAdjuster(0.5),
            },
            new List<string>(),
            2.5,
            36484.8);
    }

    public double Evaluate(HeadcountOverride item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
