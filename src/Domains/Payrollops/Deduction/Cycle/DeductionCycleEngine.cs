namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the deduction cycle calculation.</summary>
public interface IDeductionCycleModifier
{
    string Describe();
    double Adjust(DeductionCycle item, double valueValue);
}

public sealed record MinimumDeductionCycleAdjuster(double Minimum) : IDeductionCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDeductionCycleAdjuster(Registry Flags, string Flag, double Percent) : IDeductionCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDeductionCycleAdjuster(double Percent) : IDeductionCycleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDeductionCycleAdjuster(double Cap) : IDeductionCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDeductionCycleAdjuster(List<double> Bounds, List<double> Rates) : IDeductionCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionCycle item, double valueValue)
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

/// <summary>Runs the deduction cycle steps in order and keeps a trace of each.</summary>
public sealed class DeductionCycleEngine(List<IDeductionCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionCycleEngine DefaultDeductionCycleEngine(Registry flags)
    {
        return new DeductionCycleEngine(
            new List<IDeductionCycleModifier>
            {
                new MinimumDeductionCycleAdjuster(20.5),
                new FlaggedDeductionCycleAdjuster(flags, "deduction-cycle-primary", -9.9),
                new PercentDeductionCycleAdjuster(14.9),
                new CapDeductionCycleAdjuster(17175.0),
                new TieredDeductionCycleAdjuster(
                    new List<double> { 3787.0, 6660.0, 7501.0 },
                    new List<double> { 1.15, 1.06, 0.83 }),
            },
            new List<string>(),
            1.9,
            70101.2);
    }

    public double Price(DeductionCycle item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
