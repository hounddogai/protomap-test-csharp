namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the headcount exception calculation.</summary>
public interface IHeadcountExceptionAdjuster
{
    string Describe();
    double Adjust(HeadcountException item, double valueValue);
}

public sealed record PercentHeadcountExceptionAdjuster(double Percent) : IHeadcountExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedHeadcountExceptionAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredHeadcountExceptionAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountExceptionAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountException item, double valueValue)
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

public sealed record TeamHeadcountExceptionAdjuster(Dictionary<string, double> Factors) : IHeadcountExceptionAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(HeadcountException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapHeadcountExceptionAdjuster(double Cap) : IHeadcountExceptionAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the headcount exception steps in order and keeps a trace of each.</summary>
public sealed class HeadcountExceptionEngine(List<IHeadcountExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountExceptionEngine DefaultHeadcountExceptionEngine(Registry flags)
    {
        return new HeadcountExceptionEngine(
            new List<IHeadcountExceptionAdjuster>
            {
                new PercentHeadcountExceptionAdjuster(-21.6),
                new FlaggedHeadcountExceptionAdjuster(flags, "headcount-exception-adaptive", 2.8),
                new TieredHeadcountExceptionAdjuster(
                    new List<double> { 460.0, 8218.0 },
                    new List<double> { 0.91, 1.16 }),
                new TeamHeadcountExceptionAdjuster(new Dictionary<string, double> { ["north"] = 0.92 }),
                new CapHeadcountExceptionAdjuster(32216.0),
            },
            new List<string>(),
            4.4,
            17333.1);
    }

    public double Compute(HeadcountException item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
