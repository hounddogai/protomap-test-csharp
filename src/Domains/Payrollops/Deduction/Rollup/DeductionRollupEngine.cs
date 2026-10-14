namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction rollup calculation.</summary>
public interface IDeductionRollupFactor
{
    string Describe();
    double Adjust(DeductionRollup item, double valueValue);
}

public sealed record RoundingDeductionRollupAdjuster(double Step) : IDeductionRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumDeductionRollupAdjuster(double Minimum) : IDeductionRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDeductionRollupAdjuster(Registry Flags, string Flag, double Percent) : IDeductionRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DeductionRollupRetriesOffsetAdjuster(double Weight) : IDeductionRollupFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(DeductionRollup item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record CapDeductionRollupAdjuster(double Cap) : IDeductionRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDeductionRollupAdjuster(List<double> Bounds, List<double> Rates) : IDeductionRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionRollup item, double valueValue)
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

/// <summary>Runs the deduction rollup steps in order and keeps a trace of each.</summary>
public sealed class DeductionRollupEngine(List<IDeductionRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionRollupEngine DefaultDeductionRollupEngine(Registry flags)
    {
        return new DeductionRollupEngine(
            new List<IDeductionRollupFactor>
            {
                new RoundingDeductionRollupAdjuster(5.0),
                new MinimumDeductionRollupAdjuster(19.2),
                new FlaggedDeductionRollupAdjuster(flags, "deduction-rollup-capped", -1.3),
                new DeductionRollupRetriesOffsetAdjuster(1.44),
                new CapDeductionRollupAdjuster(34228.0),
                new TieredDeductionRollupAdjuster(
                    new List<double> { 1968.0, 2188.0, 6646.0 },
                    new List<double> { 1.15, 1.1, 1.13 }),
            },
            new List<string>(),
            0.5,
            46785.1);
    }

    public double Compute(DeductionRollup item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
