namespace Acme.Workforce.Workplace.AccessBadge.Policy;

/// <summary>One step of the access badge manifest calculation.</summary>
public interface IAccessBadgeManifestAdjuster
{
    string Describe();
    double Adjust(AccessBadgeManifest item, double valueValue);
}

public sealed record AccessBadgeManifestScoreOffsetAdjuster(double Weight) : IAccessBadgeManifestAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(AccessBadgeManifest item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record UnitAccessBadgeManifestAdjuster(Dictionary<string, double> Factors) : IAccessBadgeManifestAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AccessBadgeManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAccessBadgeManifestAdjuster(double Percent) : IAccessBadgeManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccessBadgeManifestAdjuster(double Minimum) : IAccessBadgeManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the access badge manifest steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeManifestEngine(List<IAccessBadgeManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeManifestEngine DefaultAccessBadgeManifestEngine()
    {
        return new AccessBadgeManifestEngine(
            new List<IAccessBadgeManifestAdjuster>
            {
                new AccessBadgeManifestScoreOffsetAdjuster(0.33),
                new UnitAccessBadgeManifestAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.97, ["central"] = 1.2 }),
                new PercentAccessBadgeManifestAdjuster(14.1),
                new MinimumAccessBadgeManifestAdjuster(11.2),
            },
            new List<string>(),
            2.3,
            37155.7);
    }

    public double Evaluate(AccessBadgeManifest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
