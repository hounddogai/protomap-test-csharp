namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual scenario audit calculation.</summary>
public interface IManualScenarioAuditAdjuster
{
    string Describe();
    double Adjust(ManualScenarioAudit item, double valueValue);
}

public sealed record TieredManualScenarioAuditAdjuster(List<double> Bounds, List<double> Rates) : IManualScenarioAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualScenarioAudit item, double valueValue)
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

public sealed record MinimumManualScenarioAuditAdjuster(double Minimum) : IManualScenarioAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualScenarioAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentManualScenarioAuditAdjuster(double Percent) : IManualScenarioAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualScenarioAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualScenarioAuditAdjuster(double Cap) : IManualScenarioAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualScenarioAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingManualScenarioAuditAdjuster(double Step) : IManualScenarioAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualScenarioAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualScenarioAuditAdjuster(Registry Flags, string Flag, double Percent) : IManualScenarioAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualScenarioAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual scenario audit steps in order and keeps a trace of each.</summary>
public sealed class ManualScenarioAuditEngine(List<IManualScenarioAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualScenarioAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualScenarioAuditEngine DefaultManualScenarioAuditEngine(Registry flags)
    {
        return new ManualScenarioAuditEngine(
            new List<IManualScenarioAuditAdjuster>
            {
                new TieredManualScenarioAuditAdjuster(
                    new List<double> { 2551.0, 4229.0, 9859.0 },
                    new List<double> { 0.97, 0.89, 0.85 }),
                new MinimumManualScenarioAuditAdjuster(20.4),
                new PercentManualScenarioAuditAdjuster(-14.3),
                new CapManualScenarioAuditAdjuster(31244.0),
                new RoundingManualScenarioAuditAdjuster(5.0),
                new FlaggedManualScenarioAuditAdjuster(flags, "manual-scenario-audit-strict", -1.9),
            },
            new List<string>(),
            4.4,
            12927.8);
    }

    public double Run(ManualScenarioAudit item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualScenarioAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
