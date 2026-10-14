namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive certification budget calculation.</summary>
public interface IAdaptiveCertificationBudgetStep
{
    string Describe();
    double Adjust(AdaptiveCertificationBudget item, double valueValue);
}

public sealed record CapAdaptiveCertificationBudgetAdjuster(double Cap) : IAdaptiveCertificationBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveCertificationBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameAdaptiveCertificationBudgetAdjuster(Dictionary<string, double> Factors) : IAdaptiveCertificationBudgetStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveCertificationBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdaptiveCertificationBudgetAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveCertificationBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveCertificationBudget item, double valueValue)
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

public sealed record AdaptiveCertificationBudgetMarginOffsetAdjuster(double Weight) : IAdaptiveCertificationBudgetStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(AdaptiveCertificationBudget item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the adaptive certification budget steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveCertificationBudgetEngine(List<IAdaptiveCertificationBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveCertificationBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveCertificationBudgetEngine DefaultAdaptiveCertificationBudgetEngine()
    {
        return new AdaptiveCertificationBudgetEngine(
            new List<IAdaptiveCertificationBudgetStep>
            {
                new CapAdaptiveCertificationBudgetAdjuster(5128.0),
                new NameAdaptiveCertificationBudgetAdjuster(new Dictionary<string, double> { ["east"] = 0.97 }),
                new TieredAdaptiveCertificationBudgetAdjuster(
                    new List<double> { 1647.0, 5018.0, 6105.0, 6652.0 },
                    new List<double> { 0.99, 0.95, 1.19, 1.19 }),
                new AdaptiveCertificationBudgetMarginOffsetAdjuster(1.95),
            },
            new List<string>(),
            2.2,
            86476.9);
    }

    public double Price(AdaptiveCertificationBudget item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveCertificationBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
