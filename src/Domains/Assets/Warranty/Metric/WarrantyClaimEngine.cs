namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty claim calculation.</summary>
public interface IWarrantyClaimStep
{
    string Describe();
    double Adjust(WarrantyClaim item, double valueValue);
}

public sealed record RoundingWarrantyClaimAdjuster(double Step) : IWarrantyClaimStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NameWarrantyClaimAdjuster(Dictionary<string, double> Factors) : IWarrantyClaimStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WarrantyClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentWarrantyClaimAdjuster(double Percent) : IWarrantyClaimStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantyClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWarrantyClaimAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyClaim item, double valueValue)
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

/// <summary>Runs the warranty claim steps in order and keeps a trace of each.</summary>
public sealed class WarrantyClaimEngine(List<IWarrantyClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyClaimEngine DefaultWarrantyClaimEngine()
    {
        return new WarrantyClaimEngine(
            new List<IWarrantyClaimStep>
            {
                new RoundingWarrantyClaimAdjuster(0.05),
                new NameWarrantyClaimAdjuster(new Dictionary<string, double> { ["central"] = 1.02 }),
                new PercentWarrantyClaimAdjuster(12.7),
                new TieredWarrantyClaimAdjuster(
                    new List<double> { 1236.0, 7192.0, 8677.0 },
                    new List<double> { 1.09, 1.02, 0.93 }),
            },
            new List<string>(),
            0.6,
            32879.3);
    }

    public double Price(WarrantyClaim item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
