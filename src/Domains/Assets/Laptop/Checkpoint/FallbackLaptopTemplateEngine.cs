namespace Acme.Workforce.Assets.Laptop.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback laptop template calculation.</summary>
public interface IFallbackLaptopTemplateStep
{
    string Describe();
    double Adjust(FallbackLaptopTemplate item, double valueValue);
}

public sealed record FallbackLaptopTemplateStartsAtOffsetAdjuster(double Weight) : IFallbackLaptopTemplateStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(FallbackLaptopTemplate item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record CapFallbackLaptopTemplateAdjuster(double Cap) : IFallbackLaptopTemplateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackLaptopTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackLaptopTemplateAdjuster(List<double> Bounds, List<double> Rates) : IFallbackLaptopTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackLaptopTemplate item, double valueValue)
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

public sealed record PercentFallbackLaptopTemplateAdjuster(double Percent) : IFallbackLaptopTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackLaptopTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelFallbackLaptopTemplateAdjuster(Dictionary<string, double> Factors) : IFallbackLaptopTemplateStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FallbackLaptopTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFallbackLaptopTemplateAdjuster(double Step) : IFallbackLaptopTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackLaptopTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback laptop template steps in order and keeps a trace of each.</summary>
public sealed class FallbackLaptopTemplateEngine(List<IFallbackLaptopTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackLaptopTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackLaptopTemplateEngine DefaultFallbackLaptopTemplateEngine()
    {
        return new FallbackLaptopTemplateEngine(
            new List<IFallbackLaptopTemplateStep>
            {
                new FallbackLaptopTemplateStartsAtOffsetAdjuster(0.11),
                new CapFallbackLaptopTemplateAdjuster(21660.0),
                new TieredFallbackLaptopTemplateAdjuster(
                    new List<double> { 274.0, 5151.0, 7142.0 },
                    new List<double> { 0.86, 0.82, 0.91 }),
                new PercentFallbackLaptopTemplateAdjuster(-9.4),
                new LabelFallbackLaptopTemplateAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.8, ["central"] = 0.89 }),
                new RoundingFallbackLaptopTemplateAdjuster(5.0),
            },
            new List<string>(),
            3.1,
            28701.0);
    }

    public double Compute(FallbackLaptopTemplate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackLaptopTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
