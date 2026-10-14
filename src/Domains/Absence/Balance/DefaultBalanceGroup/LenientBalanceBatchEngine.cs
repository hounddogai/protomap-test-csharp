namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient balance batch calculation.</summary>
public interface ILenientBalanceBatchStep
{
    string Describe();
    double Adjust(LenientBalanceBatch item, double valueValue);
}

public sealed record RoundingLenientBalanceBatchAdjuster(double Step) : ILenientBalanceBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientBalanceBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLenientBalanceBatchAdjuster(double Cap) : ILenientBalanceBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientBalanceBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLenientBalanceBatchAdjuster(double Minimum) : ILenientBalanceBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientBalanceBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientBalanceBatchAdjuster(Registry Flags, string Flag, double Percent) : ILenientBalanceBatchStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientBalanceBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient balance batch steps in order and keeps a trace of each.</summary>
public sealed class LenientBalanceBatchEngine(List<ILenientBalanceBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBalanceBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBalanceBatchEngine DefaultLenientBalanceBatchEngine(Registry flags)
    {
        return new LenientBalanceBatchEngine(
            new List<ILenientBalanceBatchStep>
            {
                new RoundingLenientBalanceBatchAdjuster(0.25),
                new CapLenientBalanceBatchAdjuster(24007.0),
                new MinimumLenientBalanceBatchAdjuster(29.8),
                new FlaggedLenientBalanceBatchAdjuster(flags, "lenient-balance-batch-legacy", -6.9),
            },
            new List<string>(),
            4.6,
            55900.4);
    }

    public double Price(LenientBalanceBatch item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientBalanceBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
