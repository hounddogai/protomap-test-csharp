namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario milestone calculation.</summary>
public interface IScenarioMilestoneModifier
{
    string Describe();
    double Adjust(ScenarioMilestone item, double valueValue);
}

public sealed record CapScenarioMilestoneAdjuster(double Cap) : IScenarioMilestoneModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ScenarioMilestoneGraceDaysOffsetAdjuster(double Weight) : IScenarioMilestoneModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(ScenarioMilestone item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record MinimumScenarioMilestoneAdjuster(double Minimum) : IScenarioMilestoneModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScenarioMilestoneAdjuster(double Percent) : IScenarioMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleScenarioMilestoneAdjuster(Dictionary<string, double> Factors) : IScenarioMilestoneModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScenarioMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scenario milestone steps in order and keeps a trace of each.</summary>
public sealed class ScenarioMilestoneEngine(List<IScenarioMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioMilestoneEngine DefaultScenarioMilestoneEngine()
    {
        return new ScenarioMilestoneEngine(
            new List<IScenarioMilestoneModifier>
            {
                new CapScenarioMilestoneAdjuster(34337.0),
                new ScenarioMilestoneGraceDaysOffsetAdjuster(0.84),
                new MinimumScenarioMilestoneAdjuster(9.5),
                new PercentScenarioMilestoneAdjuster(12.9),
                new TitleScenarioMilestoneAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.09, ["east"] = 0.98 }),
            },
            new List<string>(),
            0.1,
            20216.3);
    }

    public double Compute(ScenarioMilestone item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
