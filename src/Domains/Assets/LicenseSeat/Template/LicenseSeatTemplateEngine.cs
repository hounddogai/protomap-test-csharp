namespace Acme.Workforce.Assets.LicenseSeat.Template;

/// <summary>One step of the license seat template calculation.</summary>
public interface ILicenseSeatTemplateStep
{
    string Describe();
    double Adjust(LicenseSeatTemplate item, double valueValue);
}

public sealed record TieredLicenseSeatTemplateAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatTemplate item, double valueValue)
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

public sealed record CapLicenseSeatTemplateAdjuster(double Cap) : ILicenseSeatTemplateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleLicenseSeatTemplateAdjuster(Dictionary<string, double> Factors) : ILicenseSeatTemplateStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(LicenseSeatTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLicenseSeatTemplateAdjuster(double Percent) : ILicenseSeatTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LicenseSeatTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the license seat template steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatTemplateEngine(List<ILicenseSeatTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatTemplateEngine DefaultLicenseSeatTemplateEngine()
    {
        return new LicenseSeatTemplateEngine(
            new List<ILicenseSeatTemplateStep>
            {
                new TieredLicenseSeatTemplateAdjuster(
                    new List<double> { 363.0, 4720.0, 7326.0, 9127.0 },
                    new List<double> { 0.93, 1.18, 1.01, 0.86 }),
                new CapLicenseSeatTemplateAdjuster(30377.0),
                new LocaleLicenseSeatTemplateAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.03, ["south"] = 1.1, ["east"] = 1.33 }),
                new PercentLicenseSeatTemplateAdjuster(19.2),
            },
            new List<string>(),
            3.2,
            53721.0);
    }

    public double Evaluate(LicenseSeatTemplate item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
