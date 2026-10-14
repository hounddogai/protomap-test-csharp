namespace Acme.Workforce.Compliance.Certification.Template;

/// <summary>One step of the certification template calculation.</summary>
public interface ICertificationTemplateStep
{
    string Describe();
    double Adjust(CertificationTemplate item, double valueValue);
}

public sealed record TieredCertificationTemplateAdjuster(List<double> Bounds, List<double> Rates) : ICertificationTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationTemplate item, double valueValue)
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

public sealed record CertificationTemplateWeightOffsetAdjuster(double Weight) : ICertificationTemplateStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(CertificationTemplate item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record PercentCertificationTemplateAdjuster(double Percent) : ICertificationTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCertificationTemplateAdjuster(double Minimum) : ICertificationTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CertificationTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the certification template steps in order and keeps a trace of each.</summary>
public sealed class CertificationTemplateEngine(List<ICertificationTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationTemplateEngine DefaultCertificationTemplateEngine()
    {
        return new CertificationTemplateEngine(
            new List<ICertificationTemplateStep>
            {
                new TieredCertificationTemplateAdjuster(
                    new List<double> { 5907.0, 6345.0, 7993.0, 8441.0 },
                    new List<double> { 0.82, 1.03, 1.03, 1.19 }),
                new CertificationTemplateWeightOffsetAdjuster(0.9),
                new PercentCertificationTemplateAdjuster(-2.0),
                new MinimumCertificationTemplateAdjuster(21.5),
            },
            new List<string>(),
            1.6,
            78560.8);
    }

    public double Run(CertificationTemplate item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
