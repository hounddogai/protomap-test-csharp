namespace Acme.Workforce.Payrollops.Garnishment.Review;

/// <summary>One step of the garnishment template calculation.</summary>
public interface IGarnishmentTemplateStep
{
    string Describe();
    double Adjust(GarnishmentTemplate item, double valueValue);
}

public sealed record TitleGarnishmentTemplateAdjuster(Dictionary<string, double> Factors) : IGarnishmentTemplateStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GarnishmentTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentGarnishmentTemplateAdjuster(double Percent) : IGarnishmentTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GarnishmentTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GarnishmentTemplateDiscountOffsetAdjuster(double Weight) : IGarnishmentTemplateStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(GarnishmentTemplate item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record TieredGarnishmentTemplateAdjuster(List<double> Bounds, List<double> Rates) : IGarnishmentTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GarnishmentTemplate item, double valueValue)
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

public sealed record MinimumGarnishmentTemplateAdjuster(double Minimum) : IGarnishmentTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the garnishment template steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentTemplateEngine(List<IGarnishmentTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentTemplateEngine DefaultGarnishmentTemplateEngine()
    {
        return new GarnishmentTemplateEngine(
            new List<IGarnishmentTemplateStep>
            {
                new TitleGarnishmentTemplateAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.98, ["west"] = 0.79 }),
                new PercentGarnishmentTemplateAdjuster(18.3),
                new GarnishmentTemplateDiscountOffsetAdjuster(0.96),
                new TieredGarnishmentTemplateAdjuster(
                    new List<double> { 4246.0, 7910.0 },
                    new List<double> { 0.89, 0.98 }),
                new MinimumGarnishmentTemplateAdjuster(36.9),
            },
            new List<string>(),
            3.1,
            65820.8);
    }

    public double Compute(GarnishmentTemplate item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
