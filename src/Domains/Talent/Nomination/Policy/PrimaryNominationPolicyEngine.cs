namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary nomination policy calculation.</summary>
public interface IPrimaryNominationPolicyStep
{
    string Describe();
    double Adjust(PrimaryNominationPolicy item, double valueValue);
}

public sealed record MinimumPrimaryNominationPolicyAdjuster(double Minimum) : IPrimaryNominationPolicyStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryNominationPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PrimaryNominationPolicyBudgetOffsetAdjuster(double Weight) : IPrimaryNominationPolicyStep
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PrimaryNominationPolicy item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredPrimaryNominationPolicyAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryNominationPolicyStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryNominationPolicy item, double valueValue)
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

public sealed record RoundingPrimaryNominationPolicyAdjuster(double Step) : IPrimaryNominationPolicyStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryNominationPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryNominationPolicyAdjuster(double Percent) : IPrimaryNominationPolicyStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryNominationPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary nomination policy steps in order and keeps a trace of each.</summary>
public sealed class PrimaryNominationPolicyEngine(List<IPrimaryNominationPolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryNominationPolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryNominationPolicyEngine DefaultPrimaryNominationPolicyEngine()
    {
        return new PrimaryNominationPolicyEngine(
            new List<IPrimaryNominationPolicyStep>
            {
                new MinimumPrimaryNominationPolicyAdjuster(39.5),
                new PrimaryNominationPolicyBudgetOffsetAdjuster(1.23),
                new TieredPrimaryNominationPolicyAdjuster(
                    new List<double> { 5705.0, 7041.0, 9109.0, 9392.0 },
                    new List<double> { 1.13, 1.03, 1.12, 0.91 }),
                new RoundingPrimaryNominationPolicyAdjuster(5.0),
                new PercentPrimaryNominationPolicyAdjuster(5.8),
            },
            new List<string>(),
            4.6,
            39892.6);
    }

    public double Evaluate(PrimaryNominationPolicy item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryNominationPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
