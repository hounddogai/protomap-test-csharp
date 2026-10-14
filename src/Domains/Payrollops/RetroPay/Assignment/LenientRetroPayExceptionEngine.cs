namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient retro pay exception calculation.</summary>
public interface ILenientRetroPayExceptionFactor
{
    string Describe();
    double Adjust(LenientRetroPayException item, double valueValue);
}

public sealed record LenientRetroPayExceptionBatchSizeOffsetAdjuster(double Weight) : ILenientRetroPayExceptionFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(LenientRetroPayException item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentLenientRetroPayExceptionAdjuster(double Percent) : ILenientRetroPayExceptionFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientRetroPayException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientRetroPayExceptionAdjuster(double Step) : ILenientRetroPayExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientRetroPayException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientRetroPayExceptionAdjuster(Registry Flags, string Flag, double Percent) : ILenientRetroPayExceptionFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientRetroPayException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientRetroPayExceptionAdjuster(List<double> Bounds, List<double> Rates) : ILenientRetroPayExceptionFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientRetroPayException item, double valueValue)
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

public sealed record MinimumLenientRetroPayExceptionAdjuster(double Minimum) : ILenientRetroPayExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientRetroPayException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient retro pay exception steps in order and keeps a trace of each.</summary>
public sealed class LenientRetroPayExceptionEngine(List<ILenientRetroPayExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRetroPayExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRetroPayExceptionEngine DefaultLenientRetroPayExceptionEngine(Registry flags)
    {
        return new LenientRetroPayExceptionEngine(
            new List<ILenientRetroPayExceptionFactor>
            {
                new LenientRetroPayExceptionBatchSizeOffsetAdjuster(0.66),
                new PercentLenientRetroPayExceptionAdjuster(-24.2),
                new RoundingLenientRetroPayExceptionAdjuster(5.0),
                new FlaggedLenientRetroPayExceptionAdjuster(flags, "lenient-retro-pay-exception-manual", -7.8),
                new TieredLenientRetroPayExceptionAdjuster(
                    new List<double> { 6124.0, 8293.0 },
                    new List<double> { 0.95, 0.9 }),
                new MinimumLenientRetroPayExceptionAdjuster(5.0),
            },
            new List<string>(),
            3.7,
            99048.0);
    }

    public double Compute(LenientRetroPayException item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientRetroPayException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
