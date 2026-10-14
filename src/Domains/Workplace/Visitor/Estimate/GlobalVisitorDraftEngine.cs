namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global visitor draft calculation.</summary>
public interface IGlobalVisitorDraftFactor
{
    string Describe();
    double Adjust(GlobalVisitorDraft item, double valueValue);
}

public sealed record CapGlobalVisitorDraftAdjuster(double Cap) : IGlobalVisitorDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalVisitorDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record GlobalVisitorDraftCeilingOffsetAdjuster(double Weight) : IGlobalVisitorDraftFactor
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(GlobalVisitorDraft item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record PercentGlobalVisitorDraftAdjuster(double Percent) : IGlobalVisitorDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalVisitorDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalVisitorDraftAdjuster(double Step) : IGlobalVisitorDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalVisitorDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global visitor draft steps in order and keeps a trace of each.</summary>
public sealed class GlobalVisitorDraftEngine(List<IGlobalVisitorDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalVisitorDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalVisitorDraftEngine DefaultGlobalVisitorDraftEngine()
    {
        return new GlobalVisitorDraftEngine(
            new List<IGlobalVisitorDraftFactor>
            {
                new CapGlobalVisitorDraftAdjuster(29230.0),
                new GlobalVisitorDraftCeilingOffsetAdjuster(1.36),
                new PercentGlobalVisitorDraftAdjuster(-7.3),
                new RoundingGlobalVisitorDraftAdjuster(5.0),
            },
            new List<string>(),
            4.3,
            18002.2);
    }

    public double Compute(GlobalVisitorDraft item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalVisitorDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
