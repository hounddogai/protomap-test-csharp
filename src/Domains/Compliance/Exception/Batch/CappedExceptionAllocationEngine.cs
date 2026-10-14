namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped exception allocation calculation.</summary>
public interface ICappedExceptionAllocationFactor
{
    string Describe();
    double Adjust(CappedExceptionAllocation item, double valueValue);
}

public sealed record LabelCappedExceptionAllocationAdjuster(Dictionary<string, double> Factors) : ICappedExceptionAllocationFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedExceptionAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCappedExceptionAllocationAdjuster(List<double> Bounds, List<double> Rates) : ICappedExceptionAllocationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedExceptionAllocation item, double valueValue)
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

public sealed record FlaggedCappedExceptionAllocationAdjuster(Registry Flags, string Flag, double Percent) : ICappedExceptionAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedExceptionAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedExceptionAllocationAdjuster(double Step) : ICappedExceptionAllocationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedExceptionAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped exception allocation steps in order and keeps a trace of each.</summary>
public sealed class CappedExceptionAllocationEngine(List<ICappedExceptionAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedExceptionAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedExceptionAllocationEngine DefaultCappedExceptionAllocationEngine(Registry flags)
    {
        return new CappedExceptionAllocationEngine(
            new List<ICappedExceptionAllocationFactor>
            {
                new LabelCappedExceptionAllocationAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.88, ["north"] = 1.14 }),
                new TieredCappedExceptionAllocationAdjuster(
                    new List<double> { 484.0, 4825.0, 7449.0 },
                    new List<double> { 1.03, 1.12, 1.06 }),
                new FlaggedCappedExceptionAllocationAdjuster(flags, "capped-exception-allocation-global", 7.5),
                new RoundingCappedExceptionAllocationAdjuster(0.1),
            },
            new List<string>(),
            0.2,
            24198.3);
    }

    public double Run(CappedExceptionAllocation item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedExceptionAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
