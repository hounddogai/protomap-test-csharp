namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive tax form threshold calculation.</summary>
public interface IAdaptiveTaxFormThresholdAdjuster
{
    string Describe();
    double Adjust(AdaptiveTaxFormThreshold item, double valueValue);
}

public sealed record RoundingAdaptiveTaxFormThresholdAdjuster(double Step) : IAdaptiveTaxFormThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveTaxFormThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveTaxFormThresholdAdjuster(double Percent) : IAdaptiveTaxFormThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveTaxFormThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAdaptiveTaxFormThresholdAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTaxFormThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTaxFormThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive tax form threshold steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTaxFormThresholdEngine(List<IAdaptiveTaxFormThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTaxFormThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTaxFormThresholdEngine DefaultAdaptiveTaxFormThresholdEngine(Registry flags)
    {
        return new AdaptiveTaxFormThresholdEngine(
            new List<IAdaptiveTaxFormThresholdAdjuster>
            {
                new RoundingAdaptiveTaxFormThresholdAdjuster(0.1),
                new PercentAdaptiveTaxFormThresholdAdjuster(-0.8),
                new FlaggedAdaptiveTaxFormThresholdAdjuster(flags, "adaptive-tax-form-threshold-manual", 6.0),
            },
            new List<string>(),
            1.1,
            58712.0);
    }

    public double Evaluate(AdaptiveTaxFormThreshold item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTaxFormThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
