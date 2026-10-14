namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient laptop manifest calculation.</summary>
public interface ILenientLaptopManifestStep
{
    string Describe();
    double Adjust(LenientLaptopManifest item, double valueValue);
}

public sealed record PercentLenientLaptopManifestAdjuster(double Percent) : ILenientLaptopManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientLaptopManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientLaptopManifestAdjuster(double Minimum) : ILenientLaptopManifestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientLaptopManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamLenientLaptopManifestAdjuster(Dictionary<string, double> Factors) : ILenientLaptopManifestStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(LenientLaptopManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientLaptopManifestAdjuster(double Cap) : ILenientLaptopManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientLaptopManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientLaptopManifestAdjuster(double Step) : ILenientLaptopManifestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientLaptopManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient laptop manifest steps in order and keeps a trace of each.</summary>
public sealed class LenientLaptopManifestEngine(List<ILenientLaptopManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientLaptopManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientLaptopManifestEngine DefaultLenientLaptopManifestEngine()
    {
        return new LenientLaptopManifestEngine(
            new List<ILenientLaptopManifestStep>
            {
                new PercentLenientLaptopManifestAdjuster(12.2),
                new MinimumLenientLaptopManifestAdjuster(46.9),
                new TeamLenientLaptopManifestAdjuster(new Dictionary<string, double> { ["north"] = 1.31 }),
                new CapLenientLaptopManifestAdjuster(30851.0),
                new RoundingLenientLaptopManifestAdjuster(5.0),
            },
            new List<string>(),
            1.9,
            30562.8);
    }

    public double Run(LenientLaptopManifest item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientLaptopManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
