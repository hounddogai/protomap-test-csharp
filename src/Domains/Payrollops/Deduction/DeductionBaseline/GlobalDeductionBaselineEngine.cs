namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global deduction baseline calculation.</summary>
public interface IGlobalDeductionBaselineAdjuster
{
    string Describe();
    double Adjust(GlobalDeductionBaseline item, double valueValue);
}

public sealed record FlaggedGlobalDeductionBaselineAdjuster(Registry Flags, string Flag, double Percent) : IGlobalDeductionBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalDeductionBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalDeductionBaselineEndsAtOffsetAdjuster(double Weight) : IGlobalDeductionBaselineAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(GlobalDeductionBaseline item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record TieredGlobalDeductionBaselineAdjuster(List<double> Bounds, List<double> Rates) : IGlobalDeductionBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalDeductionBaseline item, double valueValue)
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

public sealed record MinimumGlobalDeductionBaselineAdjuster(double Minimum) : IGlobalDeductionBaselineAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalDeductionBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGlobalDeductionBaselineAdjuster(double Cap) : IGlobalDeductionBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalDeductionBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global deduction baseline steps in order and keeps a trace of each.</summary>
public sealed class GlobalDeductionBaselineEngine(List<IGlobalDeductionBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalDeductionBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalDeductionBaselineEngine DefaultGlobalDeductionBaselineEngine(Registry flags)
    {
        return new GlobalDeductionBaselineEngine(
            new List<IGlobalDeductionBaselineAdjuster>
            {
                new FlaggedGlobalDeductionBaselineAdjuster(flags, "global-deduction-baseline-pending", -0.3),
                new GlobalDeductionBaselineEndsAtOffsetAdjuster(1.31),
                new TieredGlobalDeductionBaselineAdjuster(
                    new List<double> { 7261.0, 9932.0 },
                    new List<double> { 0.86, 0.82 }),
                new MinimumGlobalDeductionBaselineAdjuster(2.0),
                new CapGlobalDeductionBaselineAdjuster(3503.0),
            },
            new List<string>(),
            2.6,
            60329.4);
    }

    public double Price(GlobalDeductionBaseline item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalDeductionBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
