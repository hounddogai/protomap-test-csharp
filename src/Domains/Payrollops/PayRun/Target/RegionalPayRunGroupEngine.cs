namespace Acme.Workforce.Payrollops.PayRun.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional pay run group calculation.</summary>
public interface IRegionalPayRunGroupStep
{
    string Describe();
    double Adjust(RegionalPayRunGroup item, double valueValue);
}

public sealed record TieredRegionalPayRunGroupAdjuster(List<double> Bounds, List<double> Rates) : IRegionalPayRunGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalPayRunGroup item, double valueValue)
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

public sealed record CapRegionalPayRunGroupAdjuster(double Cap) : IRegionalPayRunGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalPayRunGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalPayRunGroupQuantityOffsetAdjuster(double Weight) : IRegionalPayRunGroupStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(RegionalPayRunGroup item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record RoundingRegionalPayRunGroupAdjuster(double Step) : IRegionalPayRunGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPayRunGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalPayRunGroupAdjuster(double Percent) : IRegionalPayRunGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalPayRunGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalPayRunGroupAdjuster(Registry Flags, string Flag, double Percent) : IRegionalPayRunGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalPayRunGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional pay run group steps in order and keeps a trace of each.</summary>
public sealed class RegionalPayRunGroupEngine(List<IRegionalPayRunGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPayRunGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPayRunGroupEngine DefaultRegionalPayRunGroupEngine(Registry flags)
    {
        return new RegionalPayRunGroupEngine(
            new List<IRegionalPayRunGroupStep>
            {
                new TieredRegionalPayRunGroupAdjuster(
                    new List<double> { 5385.0, 6841.0, 7749.0 },
                    new List<double> { 0.9, 0.96, 0.93 }),
                new CapRegionalPayRunGroupAdjuster(24594.0),
                new RegionalPayRunGroupQuantityOffsetAdjuster(1.99),
                new RoundingRegionalPayRunGroupAdjuster(0.1),
                new PercentRegionalPayRunGroupAdjuster(18.1),
                new FlaggedRegionalPayRunGroupAdjuster(flags, "regional-pay-run-group-batched", 8.6),
            },
            new List<string>(),
            0.3,
            62809.1);
    }

    public double Evaluate(RegionalPayRunGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPayRunGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
