namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge plan calculation.</summary>
public interface IAccessBadgePlanStep
{
    string Describe();
    double Adjust(AccessBadgePlan item, double valueValue);
}

public sealed record LocaleAccessBadgePlanAdjuster(Dictionary<string, double> Factors) : IAccessBadgePlanStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(AccessBadgePlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAccessBadgePlanAdjuster(double Percent) : IAccessBadgePlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgePlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AccessBadgePlanFteOffsetAdjuster(double Weight) : IAccessBadgePlanStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(AccessBadgePlan item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record TieredAccessBadgePlanAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgePlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgePlan item, double valueValue)
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

public sealed record RoundingAccessBadgePlanAdjuster(double Step) : IAccessBadgePlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgePlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAccessBadgePlanAdjuster(double Minimum) : IAccessBadgePlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgePlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the access badge plan steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgePlanEngine(List<IAccessBadgePlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgePlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgePlanEngine DefaultAccessBadgePlanEngine()
    {
        return new AccessBadgePlanEngine(
            new List<IAccessBadgePlanStep>
            {
                new LocaleAccessBadgePlanAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.97, ["central"] = 1.04, ["north"] = 0.87 }),
                new PercentAccessBadgePlanAdjuster(-0.8),
                new AccessBadgePlanFteOffsetAdjuster(1.21),
                new TieredAccessBadgePlanAdjuster(
                    new List<double> { 823.0, 7032.0, 7051.0 },
                    new List<double> { 1.0, 1.17, 1.0 }),
                new RoundingAccessBadgePlanAdjuster(0.1),
                new MinimumAccessBadgePlanAdjuster(24.1),
            },
            new List<string>(),
            1.9,
            65700.9);
    }

    public double Price(AccessBadgePlan item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgePlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
