namespace Acme.Workforce.Engagement.ActionPlan.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan manifest calculation.</summary>
public interface IActionPlanManifestStep
{
    string Describe();
    double Adjust(ActionPlanManifest item, double valueValue);
}

public sealed record RoundingActionPlanManifestAdjuster(double Step) : IActionPlanManifestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumActionPlanManifestAdjuster(double Minimum) : IActionPlanManifestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ActionPlanManifestVersionOffsetAdjuster(double Weight) : IActionPlanManifestStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(ActionPlanManifest item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the action plan manifest steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanManifestEngine(List<IActionPlanManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanManifestEngine DefaultActionPlanManifestEngine()
    {
        return new ActionPlanManifestEngine(
            new List<IActionPlanManifestStep>
            {
                new RoundingActionPlanManifestAdjuster(0.1),
                new MinimumActionPlanManifestAdjuster(29.8),
                new ActionPlanManifestVersionOffsetAdjuster(1.94),
            },
            new List<string>(),
            2.0,
            13122.1);
    }

    public double Price(ActionPlanManifest item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
