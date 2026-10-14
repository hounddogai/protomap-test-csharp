namespace Acme.Workforce.Assets.Laptop.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop manifest calculation.</summary>
public interface ILaptopManifestAdjuster
{
    string Describe();
    double Adjust(LaptopManifest item, double valueValue);
}

public sealed record CapLaptopManifestAdjuster(double Cap) : ILaptopManifestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLaptopManifestAdjuster(double Minimum) : ILaptopManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLaptopManifestAdjuster(Registry Flags, string Flag, double Percent) : ILaptopManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the laptop manifest steps in order and keeps a trace of each.</summary>
public sealed class LaptopManifestEngine(List<ILaptopManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopManifestEngine DefaultLaptopManifestEngine(Registry flags)
    {
        return new LaptopManifestEngine(
            new List<ILaptopManifestAdjuster>
            {
                new CapLaptopManifestAdjuster(28757.0),
                new MinimumLaptopManifestAdjuster(3.9),
                new FlaggedLaptopManifestAdjuster(flags, "laptop-manifest-scheduled", -5.2),
            },
            new List<string>(),
            4.2,
            98220.2);
    }

    public double Compute(LaptopManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
