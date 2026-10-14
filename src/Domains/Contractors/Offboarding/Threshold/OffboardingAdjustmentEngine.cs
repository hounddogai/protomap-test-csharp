namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding adjustment calculation.</summary>
public interface IOffboardingAdjustmentAdjuster
{
    string Describe();
    double Adjust(OffboardingAdjustment item, double valueValue);
}

public sealed record OffboardingAdjustmentRateOffsetAdjuster(double Weight) : IOffboardingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(OffboardingAdjustment item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record RoundingOffboardingAdjustmentAdjuster(double Step) : IOffboardingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedOffboardingAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentOffboardingAdjustmentAdjuster(Dictionary<string, double> Factors) : IOffboardingAdjustmentAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(OffboardingAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredOffboardingAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingAdjustment item, double valueValue)
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

/// <summary>Runs the offboarding adjustment steps in order and keeps a trace of each.</summary>
public sealed class OffboardingAdjustmentEngine(List<IOffboardingAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingAdjustmentEngine DefaultOffboardingAdjustmentEngine(Registry flags)
    {
        return new OffboardingAdjustmentEngine(
            new List<IOffboardingAdjustmentAdjuster>
            {
                new OffboardingAdjustmentRateOffsetAdjuster(0.12),
                new RoundingOffboardingAdjustmentAdjuster(0.05),
                new FlaggedOffboardingAdjustmentAdjuster(flags, "offboarding-adjustment-default", 4.4),
                new DepartmentOffboardingAdjustmentAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.86, ["south"] = 1.35, ["online"] = 1.0 }),
                new TieredOffboardingAdjustmentAdjuster(
                    new List<double> { 1114.0, 2679.0, 4723.0, 8995.0 },
                    new List<double> { 1.05, 0.98, 1.2, 0.81 }),
            },
            new List<string>(),
            0.7,
            33052.7);
    }

    public double Compute(OffboardingAdjustment item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
