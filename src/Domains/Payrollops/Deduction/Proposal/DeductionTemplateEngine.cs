namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction template calculation.</summary>
public interface IDeductionTemplateFactor
{
    string Describe();
    double Adjust(DeductionTemplate item, double valueValue);
}

public sealed record RoundingDeductionTemplateAdjuster(double Step) : IDeductionTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDeductionTemplateAdjuster(double Percent) : IDeductionTemplateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DeductionTemplateThresholdOffsetAdjuster(double Weight) : IDeductionTemplateFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(DeductionTemplate item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the deduction template steps in order and keeps a trace of each.</summary>
public sealed class DeductionTemplateEngine(List<IDeductionTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionTemplateEngine DefaultDeductionTemplateEngine()
    {
        return new DeductionTemplateEngine(
            new List<IDeductionTemplateFactor>
            {
                new RoundingDeductionTemplateAdjuster(0.5),
                new PercentDeductionTemplateAdjuster(12.5),
                new DeductionTemplateThresholdOffsetAdjuster(0.04),
            },
            new List<string>(),
            3.4,
            6420.5);
    }

    public double Run(DeductionTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
