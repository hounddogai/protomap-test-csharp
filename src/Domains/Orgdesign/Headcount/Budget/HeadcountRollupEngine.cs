namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the headcount rollup calculation.</summary>
public interface IHeadcountRollupFactor
{
    string Describe();
    double Adjust(HeadcountRollup item, double valueValue);
}

public sealed record HeadcountRollupTenureMonthsOffsetAdjuster(double Weight) : IHeadcountRollupFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(HeadcountRollup item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record FlaggedHeadcountRollupAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentHeadcountRollupAdjuster(double Percent) : IHeadcountRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHeadcountRollupAdjuster(double Cap) : IHeadcountRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the headcount rollup steps in order and keeps a trace of each.</summary>
public sealed class HeadcountRollupEngine(List<IHeadcountRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountRollupEngine DefaultHeadcountRollupEngine(Registry flags)
    {
        return new HeadcountRollupEngine(
            new List<IHeadcountRollupFactor>
            {
                new HeadcountRollupTenureMonthsOffsetAdjuster(0.2),
                new FlaggedHeadcountRollupAdjuster(flags, "headcount-rollup-regional", 2.6),
                new PercentHeadcountRollupAdjuster(-15.5),
                new CapHeadcountRollupAdjuster(18097.0),
            },
            new List<string>(),
            0.0,
            24783.7);
    }

    public double Compute(HeadcountRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
