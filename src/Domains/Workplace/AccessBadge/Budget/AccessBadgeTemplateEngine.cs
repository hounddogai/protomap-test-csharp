namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge template calculation.</summary>
public interface IAccessBadgeTemplateStep
{
    string Describe();
    double Adjust(AccessBadgeTemplate item, double valueValue);
}

public sealed record RoundingAccessBadgeTemplateAdjuster(double Step) : IAccessBadgeTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAccessBadgeTemplateAdjuster(double Minimum) : IAccessBadgeTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAccessBadgeTemplateAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeTemplate item, double valueValue)
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

/// <summary>Runs the access badge template steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeTemplateEngine(List<IAccessBadgeTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeTemplateEngine DefaultAccessBadgeTemplateEngine()
    {
        return new AccessBadgeTemplateEngine(
            new List<IAccessBadgeTemplateStep>
            {
                new RoundingAccessBadgeTemplateAdjuster(0.5),
                new MinimumAccessBadgeTemplateAdjuster(15.1),
                new TieredAccessBadgeTemplateAdjuster(
                    new List<double> { 798.0, 3004.0, 8439.0 },
                    new List<double> { 0.86, 0.9, 0.82 }),
            },
            new List<string>(),
            2.6,
            35232.9);
    }

    public double Price(AccessBadgeTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
