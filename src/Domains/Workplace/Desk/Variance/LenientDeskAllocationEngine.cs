namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient desk allocation calculation.</summary>
public interface ILenientDeskAllocationStep
{
    string Describe();
    double Adjust(LenientDeskAllocation item, double valueValue);
}

public sealed record CapLenientDeskAllocationAdjuster(double Cap) : ILenientDeskAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientDeskAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientDeskAllocationAdjuster(double Step) : ILenientDeskAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientDeskAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLenientDeskAllocationAdjuster(List<double> Bounds, List<double> Rates) : ILenientDeskAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientDeskAllocation item, double valueValue)
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

public sealed record MinimumLenientDeskAllocationAdjuster(double Minimum) : ILenientDeskAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientDeskAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleLenientDeskAllocationAdjuster(Dictionary<string, double> Factors) : ILenientDeskAllocationStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientDeskAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientDeskAllocationAdjuster(Registry Flags, string Flag, double Percent) : ILenientDeskAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientDeskAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient desk allocation steps in order and keeps a trace of each.</summary>
public sealed class LenientDeskAllocationEngine(List<ILenientDeskAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientDeskAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientDeskAllocationEngine DefaultLenientDeskAllocationEngine(Registry flags)
    {
        return new LenientDeskAllocationEngine(
            new List<ILenientDeskAllocationStep>
            {
                new CapLenientDeskAllocationAdjuster(33087.0),
                new RoundingLenientDeskAllocationAdjuster(0.1),
                new TieredLenientDeskAllocationAdjuster(
                    new List<double> { 1241.0, 5698.0, 6423.0 },
                    new List<double> { 0.88, 1.15, 1.12 }),
                new MinimumLenientDeskAllocationAdjuster(4.2),
                new TitleLenientDeskAllocationAdjuster(new Dictionary<string, double> { ["retail"] = 1.35 }),
                new FlaggedLenientDeskAllocationAdjuster(flags, "lenient-desk-allocation-primary", 5.9),
            },
            new List<string>(),
            4.3,
            75123.1);
    }

    public double Evaluate(LenientDeskAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientDeskAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
