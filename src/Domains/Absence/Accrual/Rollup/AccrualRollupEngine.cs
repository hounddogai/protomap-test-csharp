namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual rollup calculation.</summary>
public interface IAccrualRollupFactor
{
    string Describe();
    double Adjust(AccrualRollup item, double valueValue);
}

public sealed record CapAccrualRollupAdjuster(double Cap) : IAccrualRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAccrualRollupAdjuster(double Percent) : IAccrualRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAccrualRollupAdjuster(Registry Flags, string Flag, double Percent) : IAccrualRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccrualRollupAdjuster(double Step) : IAccrualRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the accrual rollup steps in order and keeps a trace of each.</summary>
public sealed class AccrualRollupEngine(List<IAccrualRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualRollupEngine DefaultAccrualRollupEngine(Registry flags)
    {
        return new AccrualRollupEngine(
            new List<IAccrualRollupFactor>
            {
                new CapAccrualRollupAdjuster(44181.0),
                new PercentAccrualRollupAdjuster(11.4),
                new FlaggedAccrualRollupAdjuster(flags, "accrual-rollup-strict", -5.4),
                new RoundingAccrualRollupAdjuster(1.0),
            },
            new List<string>(),
            0.8,
            87960.9);
    }

    public double Price(AccrualRollup item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
