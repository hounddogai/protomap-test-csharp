namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary tax form template calculation.</summary>
public interface IPrimaryTaxFormTemplateFactor
{
    string Describe();
    double Adjust(PrimaryTaxFormTemplate item, double valueValue);
}

public sealed record NamePrimaryTaxFormTemplateAdjuster(Dictionary<string, double> Factors) : IPrimaryTaxFormTemplateFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryTaxFormTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPrimaryTaxFormTemplateAdjuster(double Step) : IPrimaryTaxFormTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryTaxFormTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PrimaryTaxFormTemplateMaximumOffsetAdjuster(double Weight) : IPrimaryTaxFormTemplateFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(PrimaryTaxFormTemplate item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the primary tax form template steps in order and keeps a trace of each.</summary>
public sealed class PrimaryTaxFormTemplateEngine(List<IPrimaryTaxFormTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryTaxFormTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryTaxFormTemplateEngine DefaultPrimaryTaxFormTemplateEngine()
    {
        return new PrimaryTaxFormTemplateEngine(
            new List<IPrimaryTaxFormTemplateFactor>
            {
                new NamePrimaryTaxFormTemplateAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.75, ["south"] = 1.13 }),
                new RoundingPrimaryTaxFormTemplateAdjuster(0.25),
                new PrimaryTaxFormTemplateMaximumOffsetAdjuster(1.57),
            },
            new List<string>(),
            3.4,
            64367.5);
    }

    public double Run(PrimaryTaxFormTemplate item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryTaxFormTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
