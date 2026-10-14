namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual laptop bundle calculation.</summary>
public interface IManualLaptopBundleStep
{
    string Describe();
    double Adjust(ManualLaptopBundle item, double valueValue);
}

public sealed record TieredManualLaptopBundleAdjuster(List<double> Bounds, List<double> Rates) : IManualLaptopBundleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualLaptopBundle item, double valueValue)
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

public sealed record PercentManualLaptopBundleAdjuster(double Percent) : IManualLaptopBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualLaptopBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualLaptopBundleAdjuster(double Step) : IManualLaptopBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualLaptopBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapManualLaptopBundleAdjuster(double Cap) : IManualLaptopBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualLaptopBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedManualLaptopBundleAdjuster(Registry Flags, string Flag, double Percent) : IManualLaptopBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualLaptopBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual laptop bundle steps in order and keeps a trace of each.</summary>
public sealed class ManualLaptopBundleEngine(List<IManualLaptopBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualLaptopBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualLaptopBundleEngine DefaultManualLaptopBundleEngine(Registry flags)
    {
        return new ManualLaptopBundleEngine(
            new List<IManualLaptopBundleStep>
            {
                new TieredManualLaptopBundleAdjuster(
                    new List<double> { 4894.0, 8793.0, 8974.0 },
                    new List<double> { 1.17, 0.84, 1.05 }),
                new PercentManualLaptopBundleAdjuster(-17.6),
                new RoundingManualLaptopBundleAdjuster(0.05),
                new CapManualLaptopBundleAdjuster(2644.0),
                new FlaggedManualLaptopBundleAdjuster(flags, "manual-laptop-bundle-strict", 3.6),
            },
            new List<string>(),
            2.7,
            53677.1);
    }

    public double Price(ManualLaptopBundle item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualLaptopBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
