namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification plan calculation.</summary>
public interface ICertificationPlanStep
{
    string Describe();
    double Adjust(CertificationPlan item, double valueValue);
}

public sealed record PercentCertificationPlanAdjuster(double Percent) : ICertificationPlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CertificationPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCertificationPlanAdjuster(double Step) : ICertificationPlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CertificationPlanFloorValueOffsetAdjuster(double Weight) : ICertificationPlanStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(CertificationPlan item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record ChannelCertificationPlanAdjuster(Dictionary<string, double> Factors) : ICertificationPlanStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CertificationPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the certification plan steps in order and keeps a trace of each.</summary>
public sealed class CertificationPlanEngine(List<ICertificationPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationPlanEngine DefaultCertificationPlanEngine()
    {
        return new CertificationPlanEngine(
            new List<ICertificationPlanStep>
            {
                new PercentCertificationPlanAdjuster(7.9),
                new RoundingCertificationPlanAdjuster(0.1),
                new CertificationPlanFloorValueOffsetAdjuster(1.63),
                new ChannelCertificationPlanAdjuster(new Dictionary<string, double> { ["north"] = 0.76 }),
            },
            new List<string>(),
            1.5,
            54664.2);
    }

    public double Evaluate(CertificationPlan item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
