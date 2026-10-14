namespace Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>One step of the access badge summary calculation.</summary>
public interface IAccessBadgeSummaryStep
{
    string Describe();
    double Adjust(AccessBadgeSummary item, double valueValue);
}

public sealed record NameAccessBadgeSummaryAdjuster(Dictionary<string, double> Factors) : IAccessBadgeSummaryStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AccessBadgeSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAccessBadgeSummaryAdjuster(double Minimum) : IAccessBadgeSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAccessBadgeSummaryAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeSummary item, double valueValue)
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

public sealed record CapAccessBadgeSummaryAdjuster(double Cap) : IAccessBadgeSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the access badge summary steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeSummaryEngine(List<IAccessBadgeSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeSummaryEngine DefaultAccessBadgeSummaryEngine()
    {
        return new AccessBadgeSummaryEngine(
            new List<IAccessBadgeSummaryStep>
            {
                new NameAccessBadgeSummaryAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.8, ["south"] = 0.74 }),
                new MinimumAccessBadgeSummaryAdjuster(40.1),
                new TieredAccessBadgeSummaryAdjuster(
                    new List<double> { 903.0, 1234.0, 8304.0, 9876.0 },
                    new List<double> { 1.04, 0.86, 1.08, 0.98 }),
                new CapAccessBadgeSummaryAdjuster(31683.0),
            },
            new List<string>(),
            4.6,
            77672.9);
    }

    public double Compute(AccessBadgeSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
