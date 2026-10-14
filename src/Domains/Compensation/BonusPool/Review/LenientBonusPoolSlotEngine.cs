namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient bonus pool slot calculation.</summary>
public interface ILenientBonusPoolSlotAdjuster
{
    string Describe();
    double Adjust(LenientBonusPoolSlot item, double valueValue);
}

public sealed record PercentLenientBonusPoolSlotAdjuster(double Percent) : ILenientBonusPoolSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientBonusPoolSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientBonusPoolSlotAdjuster(double Cap) : ILenientBonusPoolSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientBonusPoolSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLenientBonusPoolSlotAdjuster(double Minimum) : ILenientBonusPoolSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientBonusPoolSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentLenientBonusPoolSlotAdjuster(Dictionary<string, double> Factors) : ILenientBonusPoolSlotAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LenientBonusPoolSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient bonus pool slot steps in order and keeps a trace of each.</summary>
public sealed class LenientBonusPoolSlotEngine(List<ILenientBonusPoolSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBonusPoolSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBonusPoolSlotEngine DefaultLenientBonusPoolSlotEngine()
    {
        return new LenientBonusPoolSlotEngine(
            new List<ILenientBonusPoolSlotAdjuster>
            {
                new PercentLenientBonusPoolSlotAdjuster(23.9),
                new CapLenientBonusPoolSlotAdjuster(45403.0),
                new MinimumLenientBonusPoolSlotAdjuster(24.1),
                new DepartmentLenientBonusPoolSlotAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.28, ["west"] = 1.37 }),
            },
            new List<string>(),
            2.9,
            41945.6);
    }

    public double Compute(LenientBonusPoolSlot item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientBonusPoolSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
