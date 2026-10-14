namespace Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>One step of the access badge audit calculation.</summary>
public interface IAccessBadgeAuditAdjuster
{
    string Describe();
    double Adjust(AccessBadgeAudit item, double valueValue);
}

public sealed record AccessBadgeAuditWeightOffsetAdjuster(double Weight) : IAccessBadgeAuditAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(AccessBadgeAudit item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record PercentAccessBadgeAuditAdjuster(double Percent) : IAccessBadgeAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitAccessBadgeAuditAdjuster(Dictionary<string, double> Factors) : IAccessBadgeAuditAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AccessBadgeAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the access badge audit steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeAuditEngine(List<IAccessBadgeAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeAuditEngine DefaultAccessBadgeAuditEngine()
    {
        return new AccessBadgeAuditEngine(
            new List<IAccessBadgeAuditAdjuster>
            {
                new AccessBadgeAuditWeightOffsetAdjuster(1.47),
                new PercentAccessBadgeAuditAdjuster(-8.0),
                new UnitAccessBadgeAuditAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.1, ["retail"] = 1.25 }),
            },
            new List<string>(),
            0.1,
            95619.0);
    }

    public double Price(AccessBadgeAudit item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
