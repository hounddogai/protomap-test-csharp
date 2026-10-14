namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped access badge mapping calculation.</summary>
public interface ICappedAccessBadgeMappingStep
{
    string Describe();
    double Adjust(CappedAccessBadgeMapping item, double valueValue);
}

public sealed record NameCappedAccessBadgeMappingAdjuster(Dictionary<string, double> Factors) : ICappedAccessBadgeMappingStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CappedAccessBadgeMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedAccessBadgeMappingAdjuster(double Cap) : ICappedAccessBadgeMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAccessBadgeMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedAccessBadgeMappingAttemptsOffsetAdjuster(double Weight) : ICappedAccessBadgeMappingStep
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(CappedAccessBadgeMapping item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record RoundingCappedAccessBadgeMappingAdjuster(double Step) : ICappedAccessBadgeMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAccessBadgeMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedAccessBadgeMappingAdjuster(List<double> Bounds, List<double> Rates) : ICappedAccessBadgeMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedAccessBadgeMapping item, double valueValue)
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

/// <summary>Runs the capped access badge mapping steps in order and keeps a trace of each.</summary>
public sealed class CappedAccessBadgeMappingEngine(List<ICappedAccessBadgeMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAccessBadgeMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAccessBadgeMappingEngine DefaultCappedAccessBadgeMappingEngine()
    {
        return new CappedAccessBadgeMappingEngine(
            new List<ICappedAccessBadgeMappingStep>
            {
                new NameCappedAccessBadgeMappingAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.15, ["central"] = 0.92, ["east"] = 1.19 }),
                new CapCappedAccessBadgeMappingAdjuster(37929.0),
                new CappedAccessBadgeMappingAttemptsOffsetAdjuster(0.22),
                new RoundingCappedAccessBadgeMappingAdjuster(0.5),
                new TieredCappedAccessBadgeMappingAdjuster(
                    new List<double> { 1770.0, 4379.0, 5513.0 },
                    new List<double> { 1.07, 1.1, 1.17 }),
            },
            new List<string>(),
            1.2,
            51523.5);
    }

    public double Run(CappedAccessBadgeMapping item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAccessBadgeMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
