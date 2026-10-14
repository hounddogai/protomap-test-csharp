namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan window calculation.</summary>
public interface ISuccessionPlanWindowAdjuster
{
    string Describe();
    double Adjust(SuccessionPlanWindow item, double valueValue);
}

public sealed record CapSuccessionPlanWindowAdjuster(double Cap) : ISuccessionPlanWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleSuccessionPlanWindowAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanWindowAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(SuccessionPlanWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSuccessionPlanWindowAdjuster(double Step) : ISuccessionPlanWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSuccessionPlanWindowAdjuster(double Minimum) : ISuccessionPlanWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the succession plan window steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanWindowEngine(List<ISuccessionPlanWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanWindowEngine DefaultSuccessionPlanWindowEngine()
    {
        return new SuccessionPlanWindowEngine(
            new List<ISuccessionPlanWindowAdjuster>
            {
                new CapSuccessionPlanWindowAdjuster(3312.0),
                new LocaleSuccessionPlanWindowAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.14, ["online"] = 1.04 }),
                new RoundingSuccessionPlanWindowAdjuster(1.0),
                new MinimumSuccessionPlanWindowAdjuster(16.2),
            },
            new List<string>(),
            1.4,
            88032.1);
    }

    public double Evaluate(SuccessionPlanWindow item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
