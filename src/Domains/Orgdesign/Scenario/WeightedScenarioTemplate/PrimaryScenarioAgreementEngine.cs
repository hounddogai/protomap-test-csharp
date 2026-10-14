namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary scenario agreement calculation.</summary>
public interface IPrimaryScenarioAgreementFactor
{
    string Describe();
    double Adjust(PrimaryScenarioAgreement item, double valueValue);
}

public sealed record PrimaryScenarioAgreementMinimumOffsetAdjuster(double Weight) : IPrimaryScenarioAgreementFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(PrimaryScenarioAgreement item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingPrimaryScenarioAgreementAdjuster(double Step) : IPrimaryScenarioAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryScenarioAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryScenarioAgreementAdjuster(double Percent) : IPrimaryScenarioAgreementFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryScenarioAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPrimaryScenarioAgreementAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryScenarioAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryScenarioAgreement item, double valueValue)
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

public sealed record FlaggedPrimaryScenarioAgreementAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryScenarioAgreementFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryScenarioAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryScenarioAgreementAdjuster(double Cap) : IPrimaryScenarioAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryScenarioAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary scenario agreement steps in order and keeps a trace of each.</summary>
public sealed class PrimaryScenarioAgreementEngine(List<IPrimaryScenarioAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryScenarioAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryScenarioAgreementEngine DefaultPrimaryScenarioAgreementEngine(Registry flags)
    {
        return new PrimaryScenarioAgreementEngine(
            new List<IPrimaryScenarioAgreementFactor>
            {
                new PrimaryScenarioAgreementMinimumOffsetAdjuster(0.61),
                new RoundingPrimaryScenarioAgreementAdjuster(1.0),
                new PercentPrimaryScenarioAgreementAdjuster(7.6),
                new TieredPrimaryScenarioAgreementAdjuster(
                    new List<double> { 3598.0, 4030.0, 6490.0 },
                    new List<double> { 0.93, 1.08, 0.95 }),
                new FlaggedPrimaryScenarioAgreementAdjuster(flags, "primary-scenario-agreement-weighted", 4.0),
                new CapPrimaryScenarioAgreementAdjuster(28243.0),
            },
            new List<string>(),
            0.5,
            20363.7);
    }

    public double Compute(PrimaryScenarioAgreement item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryScenarioAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
