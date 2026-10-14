namespace Acme.Workforce.Assets.Warranty.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty bundle calculation.</summary>
public interface IWarrantyBundleStep
{
    string Describe();
    double Adjust(WarrantyBundle item, double valueValue);
}

public sealed record MinimumWarrantyBundleAdjuster(double Minimum) : IWarrantyBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWarrantyBundleAdjuster(double Percent) : IWarrantyBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedWarrantyBundleAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the warranty bundle steps in order and keeps a trace of each.</summary>
public sealed class WarrantyBundleEngine(List<IWarrantyBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyBundleEngine DefaultWarrantyBundleEngine(Registry flags)
    {
        return new WarrantyBundleEngine(
            new List<IWarrantyBundleStep>
            {
                new MinimumWarrantyBundleAdjuster(30.9),
                new PercentWarrantyBundleAdjuster(-19.6),
                new FlaggedWarrantyBundleAdjuster(flags, "warranty-bundle-fallback", -5.6),
            },
            new List<string>(),
            4.4,
            16296.5);
    }

    public double Compute(WarrantyBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
