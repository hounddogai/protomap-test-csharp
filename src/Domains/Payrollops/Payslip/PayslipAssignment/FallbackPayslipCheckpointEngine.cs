namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback payslip checkpoint calculation.</summary>
public interface IFallbackPayslipCheckpointModifier
{
    string Describe();
    double Adjust(FallbackPayslipCheckpoint item, double valueValue);
}

public sealed record CapFallbackPayslipCheckpointAdjuster(double Cap) : IFallbackPayslipCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPayslipCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackPayslipCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IFallbackPayslipCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackPayslipCheckpoint item, double valueValue)
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

public sealed record DepartmentFallbackPayslipCheckpointAdjuster(Dictionary<string, double> Factors) : IFallbackPayslipCheckpointModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(FallbackPayslipCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFallbackPayslipCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPayslipCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPayslipCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackPayslipCheckpointAdjuster(double Step) : IFallbackPayslipCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackPayslipCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback payslip checkpoint steps in order and keeps a trace of each.</summary>
public sealed class FallbackPayslipCheckpointEngine(List<IFallbackPayslipCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPayslipCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPayslipCheckpointEngine DefaultFallbackPayslipCheckpointEngine(Registry flags)
    {
        return new FallbackPayslipCheckpointEngine(
            new List<IFallbackPayslipCheckpointModifier>
            {
                new CapFallbackPayslipCheckpointAdjuster(7981.0),
                new TieredFallbackPayslipCheckpointAdjuster(
                    new List<double> { 2859.0, 4569.0, 9469.0 },
                    new List<double> { 0.96, 1.01, 1.2 }),
                new DepartmentFallbackPayslipCheckpointAdjuster(new Dictionary<string, double> { ["east"] = 1.23 }),
                new FlaggedFallbackPayslipCheckpointAdjuster(flags, "fallback-payslip-checkpoint-lenient", 8.2),
                new RoundingFallbackPayslipCheckpointAdjuster(1.0),
            },
            new List<string>(),
            1.2,
            14878.0);
    }

    public double Evaluate(FallbackPayslipCheckpoint item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPayslipCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
