namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor template calculation.</summary>
public interface IVisitorTemplateFactor
{
    string Describe();
    double Adjust(VisitorTemplate item, double valueValue);
}

public sealed record PercentVisitorTemplateAdjuster(double Percent) : IVisitorTemplateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentVisitorTemplateAdjuster(Dictionary<string, double> Factors) : IVisitorTemplateFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(VisitorTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredVisitorTemplateAdjuster(List<double> Bounds, List<double> Rates) : IVisitorTemplateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorTemplate item, double valueValue)
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

/// <summary>Runs the visitor template steps in order and keeps a trace of each.</summary>
public sealed class VisitorTemplateEngine(List<IVisitorTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorTemplateEngine DefaultVisitorTemplateEngine()
    {
        return new VisitorTemplateEngine(
            new List<IVisitorTemplateFactor>
            {
                new PercentVisitorTemplateAdjuster(9.7),
                new DepartmentVisitorTemplateAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.23, ["north"] = 1.23 }),
                new TieredVisitorTemplateAdjuster(
                    new List<double> { 2970.0, 4601.0, 6641.0, 8147.0 },
                    new List<double> { 0.98, 0.96, 0.98, 0.98 }),
            },
            new List<string>(),
            0.6,
            45133.5);
    }

    public double Price(VisitorTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<VisitorTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
