namespace Acme.Workforce.Assets.Laptop.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending laptop target calculation.</summary>
public interface IPendingLaptopTargetAdjuster
{
    string Describe();
    double Adjust(PendingLaptopTarget item, double valueValue);
}

public sealed record FlaggedPendingLaptopTargetAdjuster(Registry Flags, string Flag, double Percent) : IPendingLaptopTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingLaptopTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingLaptopTargetAdjuster(List<double> Bounds, List<double> Rates) : IPendingLaptopTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingLaptopTarget item, double valueValue)
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

public sealed record LabelPendingLaptopTargetAdjuster(Dictionary<string, double> Factors) : IPendingLaptopTargetAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PendingLaptopTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingLaptopTargetAdjuster(double Percent) : IPendingLaptopTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingLaptopTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending laptop target steps in order and keeps a trace of each.</summary>
public sealed class PendingLaptopTargetEngine(List<IPendingLaptopTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingLaptopTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingLaptopTargetEngine DefaultPendingLaptopTargetEngine(Registry flags)
    {
        return new PendingLaptopTargetEngine(
            new List<IPendingLaptopTargetAdjuster>
            {
                new FlaggedPendingLaptopTargetAdjuster(flags, "pending-laptop-target-pending", 8.2),
                new TieredPendingLaptopTargetAdjuster(
                    new List<double> { 3437.0, 5798.0, 7040.0 },
                    new List<double> { 0.84, 1.02, 0.83 }),
                new LabelPendingLaptopTargetAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.08, ["online"] = 1.13, ["west"] = 1.1 }),
                new PercentPendingLaptopTargetAdjuster(-21.4),
            },
            new List<string>(),
            2.4,
            68154.1);
    }

    public double Evaluate(PendingLaptopTarget item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingLaptopTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
