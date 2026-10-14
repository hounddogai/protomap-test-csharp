namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction score calculation.</summary>
public interface IDeductionScoreModifier
{
    string Describe();
    double Adjust(DeductionScore item, double valueValue);
}

public sealed record UnitDeductionScoreAdjuster(Dictionary<string, double> Factors) : IDeductionScoreModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(DeductionScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDeductionScoreAdjuster(double Step) : IDeductionScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapDeductionScoreAdjuster(double Cap) : IDeductionScoreModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDeductionScoreAdjuster(double Percent) : IDeductionScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedDeductionScoreAdjuster(Registry Flags, string Flag, double Percent) : IDeductionScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the deduction score steps in order and keeps a trace of each.</summary>
public sealed class DeductionScoreEngine(List<IDeductionScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionScoreEngine DefaultDeductionScoreEngine(Registry flags)
    {
        return new DeductionScoreEngine(
            new List<IDeductionScoreModifier>
            {
                new UnitDeductionScoreAdjuster(new Dictionary<string, double> { ["retail"] = 0.72, ["east"] = 0.96 }),
                new RoundingDeductionScoreAdjuster(0.1),
                new CapDeductionScoreAdjuster(35975.0),
                new PercentDeductionScoreAdjuster(4.4),
                new FlaggedDeductionScoreAdjuster(flags, "deduction-score-primary", -8.0),
            },
            new List<string>(),
            2.0,
            40499.8);
    }

    public double Evaluate(DeductionScore item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
