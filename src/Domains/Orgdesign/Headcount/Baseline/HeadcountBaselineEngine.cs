namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount baseline calculation.</summary>
public interface IHeadcountBaselineModifier
{
    string Describe();
    double Adjust(HeadcountBaseline item, double valueValue);
}

public sealed record CapHeadcountBaselineAdjuster(double Cap) : IHeadcountBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredHeadcountBaselineAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountBaseline item, double valueValue)
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

public sealed record RoundingHeadcountBaselineAdjuster(double Step) : IHeadcountBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record HeadcountBaselineAmountOffsetAdjuster(double Weight) : IHeadcountBaselineModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(HeadcountBaseline item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record FlaggedHeadcountBaselineAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the headcount baseline steps in order and keeps a trace of each.</summary>
public sealed class HeadcountBaselineEngine(List<IHeadcountBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountBaselineEngine DefaultHeadcountBaselineEngine(Registry flags)
    {
        return new HeadcountBaselineEngine(
            new List<IHeadcountBaselineModifier>
            {
                new CapHeadcountBaselineAdjuster(46360.0),
                new TieredHeadcountBaselineAdjuster(
                    new List<double> { 5603.0, 5934.0, 8401.0 },
                    new List<double> { 1.01, 0.84, 1.04 }),
                new RoundingHeadcountBaselineAdjuster(0.1),
                new HeadcountBaselineAmountOffsetAdjuster(0.02),
                new FlaggedHeadcountBaselineAdjuster(flags, "headcount-baseline-rolling", 7.1),
            },
            new List<string>(),
            3.4,
            90275.8);
    }

    public double Run(HeadcountBaseline item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
