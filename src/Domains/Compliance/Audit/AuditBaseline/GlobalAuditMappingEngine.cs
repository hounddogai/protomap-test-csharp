namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global audit mapping calculation.</summary>
public interface IGlobalAuditMappingFactor
{
    string Describe();
    double Adjust(GlobalAuditMapping item, double valueValue);
}

public sealed record DepartmentGlobalAuditMappingAdjuster(Dictionary<string, double> Factors) : IGlobalAuditMappingFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(GlobalAuditMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalAuditMappingAdjuster(double Minimum) : IGlobalAuditMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalAuditMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingGlobalAuditMappingAdjuster(double Step) : IGlobalAuditMappingFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAuditMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentGlobalAuditMappingAdjuster(double Percent) : IGlobalAuditMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAuditMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global audit mapping steps in order and keeps a trace of each.</summary>
public sealed class GlobalAuditMappingEngine(List<IGlobalAuditMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAuditMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAuditMappingEngine DefaultGlobalAuditMappingEngine()
    {
        return new GlobalAuditMappingEngine(
            new List<IGlobalAuditMappingFactor>
            {
                new DepartmentGlobalAuditMappingAdjuster(new Dictionary<string, double> { ["online"] = 1.36 }),
                new MinimumGlobalAuditMappingAdjuster(26.9),
                new RoundingGlobalAuditMappingAdjuster(5.0),
                new PercentGlobalAuditMappingAdjuster(23.5),
            },
            new List<string>(),
            1.1,
            39979.8);
    }

    public double Price(GlobalAuditMapping item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAuditMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
