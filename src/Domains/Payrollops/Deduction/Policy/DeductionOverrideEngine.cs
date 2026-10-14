namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction override calculation.</summary>
public interface IDeductionOverrideStep
{
    string Describe();
    double Adjust(DeductionOverride item, double valueValue);
}

public sealed record FlaggedDeductionOverrideAdjuster(Registry Flags, string Flag, double Percent) : IDeductionOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDeductionOverrideAdjuster(double Cap) : IDeductionOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DeductionOverrideRatioOffsetAdjuster(double Weight) : IDeductionOverrideStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(DeductionOverride item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the deduction override steps in order and keeps a trace of each.</summary>
public sealed class DeductionOverrideEngine(List<IDeductionOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionOverrideEngine DefaultDeductionOverrideEngine(Registry flags)
    {
        return new DeductionOverrideEngine(
            new List<IDeductionOverrideStep>
            {
                new FlaggedDeductionOverrideAdjuster(flags, "deduction-override-global", -9.2),
                new CapDeductionOverrideAdjuster(1164.0),
                new DeductionOverrideRatioOffsetAdjuster(0.86),
            },
            new List<string>(),
            0.2,
            73278.5);
    }

    public double Evaluate(DeductionOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
