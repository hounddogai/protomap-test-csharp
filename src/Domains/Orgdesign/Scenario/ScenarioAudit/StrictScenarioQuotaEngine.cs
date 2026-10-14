namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict scenario quota calculation.</summary>
public interface IStrictScenarioQuotaStep
{
    string Describe();
    double Adjust(StrictScenarioQuota item, double valueValue);
}

public sealed record FlaggedStrictScenarioQuotaAdjuster(Registry Flags, string Flag, double Percent) : IStrictScenarioQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictScenarioQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictScenarioQuotaAdjuster(double Cap) : IStrictScenarioQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictScenarioQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStrictScenarioQuotaAdjuster(double Step) : IStrictScenarioQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictScenarioQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStrictScenarioQuotaAdjuster(double Percent) : IStrictScenarioQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictScenarioQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StrictScenarioQuotaDurationMinutesOffsetAdjuster(double Weight) : IStrictScenarioQuotaStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(StrictScenarioQuota item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the strict scenario quota steps in order and keeps a trace of each.</summary>
public sealed class StrictScenarioQuotaEngine(List<IStrictScenarioQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictScenarioQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictScenarioQuotaEngine DefaultStrictScenarioQuotaEngine(Registry flags)
    {
        return new StrictScenarioQuotaEngine(
            new List<IStrictScenarioQuotaStep>
            {
                new FlaggedStrictScenarioQuotaAdjuster(flags, "strict-scenario-quota-rolling", 8.2),
                new CapStrictScenarioQuotaAdjuster(4447.0),
                new RoundingStrictScenarioQuotaAdjuster(1.0),
                new PercentStrictScenarioQuotaAdjuster(-14.1),
                new StrictScenarioQuotaDurationMinutesOffsetAdjuster(0.13),
            },
            new List<string>(),
            0.3,
            76271.1);
    }

    public double Run(StrictScenarioQuota item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictScenarioQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
