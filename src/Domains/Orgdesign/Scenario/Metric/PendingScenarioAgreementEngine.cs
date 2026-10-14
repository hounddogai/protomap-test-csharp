namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending scenario agreement calculation.</summary>
public interface IPendingScenarioAgreementFactor
{
    string Describe();
    double Adjust(PendingScenarioAgreement item, double valueValue);
}

public sealed record FlaggedPendingScenarioAgreementAdjuster(Registry Flags, string Flag, double Percent) : IPendingScenarioAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingScenarioAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingScenarioAgreementAdjuster(List<double> Bounds, List<double> Rates) : IPendingScenarioAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingScenarioAgreement item, double valueValue)
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

public sealed record LocalePendingScenarioAgreementAdjuster(Dictionary<string, double> Factors) : IPendingScenarioAgreementFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PendingScenarioAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending scenario agreement steps in order and keeps a trace of each.</summary>
public sealed class PendingScenarioAgreementEngine(List<IPendingScenarioAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingScenarioAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingScenarioAgreementEngine DefaultPendingScenarioAgreementEngine(Registry flags)
    {
        return new PendingScenarioAgreementEngine(
            new List<IPendingScenarioAgreementFactor>
            {
                new FlaggedPendingScenarioAgreementAdjuster(flags, "pending-scenario-agreement-pending", 5.6),
                new TieredPendingScenarioAgreementAdjuster(
                    new List<double> { 2384.0, 5258.0, 7918.0 },
                    new List<double> { 1.0, 0.81, 1.17 }),
                new LocalePendingScenarioAgreementAdjuster(new Dictionary<string, double> { ["retail"] = 1.13 }),
            },
            new List<string>(),
            1.6,
            87052.7);
    }

    public double Compute(PendingScenarioAgreement item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingScenarioAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
