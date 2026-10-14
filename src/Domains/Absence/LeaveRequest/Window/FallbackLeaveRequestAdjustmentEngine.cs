namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback leave request adjustment calculation.</summary>
public interface IFallbackLeaveRequestAdjustmentModifier
{
    string Describe();
    double Adjust(FallbackLeaveRequestAdjustment item, double valueValue);
}

public sealed record TieredFallbackLeaveRequestAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IFallbackLeaveRequestAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackLeaveRequestAdjustment item, double valueValue)
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

public sealed record RoundingFallbackLeaveRequestAdjustmentAdjuster(double Step) : IFallbackLeaveRequestAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackLeaveRequestAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentFallbackLeaveRequestAdjustmentAdjuster(double Percent) : IFallbackLeaveRequestAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackLeaveRequestAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackLeaveRequestAdjustmentAdjuster(double Minimum) : IFallbackLeaveRequestAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackLeaveRequestAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFallbackLeaveRequestAdjustmentAdjuster(double Cap) : IFallbackLeaveRequestAdjustmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackLeaveRequestAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback leave request adjustment steps in order and keeps a trace of each.</summary>
public sealed class FallbackLeaveRequestAdjustmentEngine(List<IFallbackLeaveRequestAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackLeaveRequestAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackLeaveRequestAdjustmentEngine DefaultFallbackLeaveRequestAdjustmentEngine()
    {
        return new FallbackLeaveRequestAdjustmentEngine(
            new List<IFallbackLeaveRequestAdjustmentModifier>
            {
                new TieredFallbackLeaveRequestAdjustmentAdjuster(
                    new List<double> { 4435.0, 6292.0 },
                    new List<double> { 1.05, 0.91 }),
                new RoundingFallbackLeaveRequestAdjustmentAdjuster(1.0),
                new PercentFallbackLeaveRequestAdjustmentAdjuster(-17.0),
                new MinimumFallbackLeaveRequestAdjustmentAdjuster(10.5),
                new CapFallbackLeaveRequestAdjustmentAdjuster(27299.0),
            },
            new List<string>(),
            3.1,
            74993.7);
    }

    public double Run(FallbackLeaveRequestAdjustment item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackLeaveRequestAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
