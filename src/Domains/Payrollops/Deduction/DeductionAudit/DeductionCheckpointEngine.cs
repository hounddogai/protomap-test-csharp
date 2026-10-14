namespace Acme.Workforce.Payrollops.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction checkpoint calculation.</summary>
public interface IDeductionCheckpointModifier
{
    string Describe();
    double Adjust(DeductionCheckpoint item, double valueValue);
}

public sealed record MinimumDeductionCheckpointAdjuster(double Minimum) : IDeductionCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDeductionCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IDeductionCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDeductionCheckpointAdjuster(double Percent) : IDeductionCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDeductionCheckpointAdjuster(double Step) : IDeductionCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryDeductionCheckpointAdjuster(Dictionary<string, double> Factors) : IDeductionCheckpointModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DeductionCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the deduction checkpoint steps in order and keeps a trace of each.</summary>
public sealed class DeductionCheckpointEngine(List<IDeductionCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionCheckpointEngine DefaultDeductionCheckpointEngine(Registry flags)
    {
        return new DeductionCheckpointEngine(
            new List<IDeductionCheckpointModifier>
            {
                new MinimumDeductionCheckpointAdjuster(19.6),
                new FlaggedDeductionCheckpointAdjuster(flags, "deduction-checkpoint-strict", -9.0),
                new PercentDeductionCheckpointAdjuster(-7.9),
                new RoundingDeductionCheckpointAdjuster(0.1),
                new CountryDeductionCheckpointAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.27, ["online"] = 0.95, ["west"] = 1.12 }),
            },
            new List<string>(),
            2.6,
            81973.8);
    }

    public double Price(DeductionCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
