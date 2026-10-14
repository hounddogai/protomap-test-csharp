namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding plan calculation.</summary>
public interface IOffboardingPlanModifier
{
    string Describe();
    double Adjust(OffboardingPlan item, double valueValue);
}

public sealed record LabelOffboardingPlanAdjuster(Dictionary<string, double> Factors) : IOffboardingPlanModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(OffboardingPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record OffboardingPlanFloorValueOffsetAdjuster(double Weight) : IOffboardingPlanModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(OffboardingPlan item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record PercentOffboardingPlanAdjuster(double Percent) : IOffboardingPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOffboardingPlanAdjuster(double Minimum) : IOffboardingPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the offboarding plan steps in order and keeps a trace of each.</summary>
public sealed class OffboardingPlanEngine(List<IOffboardingPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingPlanEngine DefaultOffboardingPlanEngine()
    {
        return new OffboardingPlanEngine(
            new List<IOffboardingPlanModifier>
            {
                new LabelOffboardingPlanAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.93, ["central"] = 0.83 }),
                new OffboardingPlanFloorValueOffsetAdjuster(1.97),
                new PercentOffboardingPlanAdjuster(-11.0),
                new MinimumOffboardingPlanAdjuster(12.2),
            },
            new List<string>(),
            3.4,
            88723.0);
    }

    public double Compute(OffboardingPlan item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
