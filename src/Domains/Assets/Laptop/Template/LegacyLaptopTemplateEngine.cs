namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy laptop template calculation.</summary>
public interface ILegacyLaptopTemplateStep
{
    string Describe();
    double Adjust(LegacyLaptopTemplate item, double valueValue);
}

public sealed record TitleLegacyLaptopTemplateAdjuster(Dictionary<string, double> Factors) : ILegacyLaptopTemplateStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyLaptopTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLegacyLaptopTemplateAdjuster(double Percent) : ILegacyLaptopTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyLaptopTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyLaptopTemplateAdjuster(List<double> Bounds, List<double> Rates) : ILegacyLaptopTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyLaptopTemplate item, double valueValue)
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

/// <summary>Runs the legacy laptop template steps in order and keeps a trace of each.</summary>
public sealed class LegacyLaptopTemplateEngine(List<ILegacyLaptopTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyLaptopTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyLaptopTemplateEngine DefaultLegacyLaptopTemplateEngine()
    {
        return new LegacyLaptopTemplateEngine(
            new List<ILegacyLaptopTemplateStep>
            {
                new TitleLegacyLaptopTemplateAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.38, ["west"] = 1.19 }),
                new PercentLegacyLaptopTemplateAdjuster(16.6),
                new TieredLegacyLaptopTemplateAdjuster(
                    new List<double> { 4746.0, 8957.0 },
                    new List<double> { 0.95, 0.82 }),
            },
            new List<string>(),
            3.0,
            52494.2);
    }

    public double Evaluate(LegacyLaptopTemplate item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyLaptopTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
