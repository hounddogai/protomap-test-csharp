namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary asset return profile calculation.</summary>
public interface IPrimaryAssetReturnProfileStep
{
    string Describe();
    double Adjust(PrimaryAssetReturnProfile item, double valueValue);
}

public sealed record CategoryPrimaryAssetReturnProfileAdjuster(Dictionary<string, double> Factors) : IPrimaryAssetReturnProfileStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PrimaryAssetReturnProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPrimaryAssetReturnProfileAdjuster(double Percent) : IPrimaryAssetReturnProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAssetReturnProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryAssetReturnProfileAdjuster(double Step) : IPrimaryAssetReturnProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryAssetReturnProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary asset return profile steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAssetReturnProfileEngine(List<IPrimaryAssetReturnProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAssetReturnProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAssetReturnProfileEngine DefaultPrimaryAssetReturnProfileEngine()
    {
        return new PrimaryAssetReturnProfileEngine(
            new List<IPrimaryAssetReturnProfileStep>
            {
                new CategoryPrimaryAssetReturnProfileAdjuster(new Dictionary<string, double> { ["west"] = 1.3 }),
                new PercentPrimaryAssetReturnProfileAdjuster(24.9),
                new RoundingPrimaryAssetReturnProfileAdjuster(5.0),
            },
            new List<string>(),
            3.7,
            6339.5);
    }

    public double Price(PrimaryAssetReturnProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAssetReturnProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
