namespace Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>One step of the legacy leave request template calculation.</summary>
public interface ILegacyLeaveRequestTemplateFactor
{
    string Describe();
    double Adjust(LegacyLeaveRequestTemplate item, double valueValue);
}

public sealed record CapLegacyLeaveRequestTemplateAdjuster(double Cap) : ILegacyLeaveRequestTemplateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyLeaveRequestTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyLeaveRequestTemplatePriorityOffsetAdjuster(double Weight) : ILegacyLeaveRequestTemplateFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(LegacyLeaveRequestTemplate item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record LabelLegacyLeaveRequestTemplateAdjuster(Dictionary<string, double> Factors) : ILegacyLeaveRequestTemplateFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyLeaveRequestTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy leave request template steps in order and keeps a trace of each.</summary>
public sealed class LegacyLeaveRequestTemplateEngine(List<ILegacyLeaveRequestTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyLeaveRequestTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyLeaveRequestTemplateEngine DefaultLegacyLeaveRequestTemplateEngine()
    {
        return new LegacyLeaveRequestTemplateEngine(
            new List<ILegacyLeaveRequestTemplateFactor>
            {
                new CapLegacyLeaveRequestTemplateAdjuster(9435.0),
                new LegacyLeaveRequestTemplatePriorityOffsetAdjuster(1.69),
                new LabelLegacyLeaveRequestTemplateAdjuster(new Dictionary<string, double> { ["south"] = 1.04 }),
            },
            new List<string>(),
            4.6,
            39078.9);
    }

    public double Run(LegacyLeaveRequestTemplate item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyLeaveRequestTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
