namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled warranty manifest calculation.</summary>
public interface IScheduledWarrantyManifestFactor
{
    string Describe();
    double Adjust(ScheduledWarrantyManifest item, double valueValue);
}

public sealed record PercentScheduledWarrantyManifestAdjuster(double Percent) : IScheduledWarrantyManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledWarrantyManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledWarrantyManifestAdjuster(double Step) : IScheduledWarrantyManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledWarrantyManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelScheduledWarrantyManifestAdjuster(Dictionary<string, double> Factors) : IScheduledWarrantyManifestFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScheduledWarrantyManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledWarrantyManifestAdjuster(double Cap) : IScheduledWarrantyManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledWarrantyManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled warranty manifest steps in order and keeps a trace of each.</summary>
public sealed class ScheduledWarrantyManifestEngine(List<IScheduledWarrantyManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledWarrantyManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledWarrantyManifestEngine DefaultScheduledWarrantyManifestEngine()
    {
        return new ScheduledWarrantyManifestEngine(
            new List<IScheduledWarrantyManifestFactor>
            {
                new PercentScheduledWarrantyManifestAdjuster(-19.2),
                new RoundingScheduledWarrantyManifestAdjuster(0.5),
                new LabelScheduledWarrantyManifestAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.36, ["south"] = 0.79, ["online"] = 1.39 }),
                new CapScheduledWarrantyManifestAdjuster(19635.0),
            },
            new List<string>(),
            0.3,
            73788.8);
    }

    public double Evaluate(ScheduledWarrantyManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledWarrantyManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
