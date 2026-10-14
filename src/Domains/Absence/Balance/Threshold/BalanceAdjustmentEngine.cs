namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the balance adjustment calculation.</summary>
public interface IBalanceAdjustmentModifier
{
    string Describe();
    double Adjust(BalanceAdjustment item, double valueValue);
}

public sealed record MinimumBalanceAdjustmentAdjuster(double Minimum) : IBalanceAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBalanceAdjustmentAdjuster(double Cap) : IBalanceAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBalanceAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IBalanceAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the balance adjustment steps in order and keeps a trace of each.</summary>
public sealed class BalanceAdjustmentEngine(List<IBalanceAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceAdjustmentEngine DefaultBalanceAdjustmentEngine(Registry flags)
    {
        return new BalanceAdjustmentEngine(
            new List<IBalanceAdjustmentModifier>
            {
                new MinimumBalanceAdjustmentAdjuster(38.1),
                new CapBalanceAdjustmentAdjuster(22721.0),
                new FlaggedBalanceAdjustmentAdjuster(flags, "balance-adjustment-adaptive", 8.9),
            },
            new List<string>(),
            3.0,
            78622.2);
    }

    public double Evaluate(BalanceAdjustment item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
