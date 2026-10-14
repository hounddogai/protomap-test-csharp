namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the attestation budget calculation.</summary>
public interface IAttestationBudgetFactor
{
    string Describe();
    double Adjust(AttestationBudget item, double valueValue);
}

public sealed record MinimumAttestationBudgetAdjuster(double Minimum) : IAttestationBudgetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAttestationBudgetAdjuster(double Percent) : IAttestationBudgetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationBudgetAdjuster(List<double> Bounds, List<double> Rates) : IAttestationBudgetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationBudget item, double valueValue)
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

public sealed record AttestationBudgetLeaveDaysOffsetAdjuster(double Weight) : IAttestationBudgetFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(AttestationBudget item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record FlaggedAttestationBudgetAdjuster(Registry Flags, string Flag, double Percent) : IAttestationBudgetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAttestationBudgetAdjuster(double Cap) : IAttestationBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the attestation budget steps in order and keeps a trace of each.</summary>
public sealed class AttestationBudgetEngine(List<IAttestationBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationBudgetEngine DefaultAttestationBudgetEngine(Registry flags)
    {
        return new AttestationBudgetEngine(
            new List<IAttestationBudgetFactor>
            {
                new MinimumAttestationBudgetAdjuster(20.5),
                new PercentAttestationBudgetAdjuster(11.2),
                new TieredAttestationBudgetAdjuster(
                    new List<double> { 2447.0, 5157.0, 8955.0 },
                    new List<double> { 0.84, 1.08, 1.17 }),
                new AttestationBudgetLeaveDaysOffsetAdjuster(0.6),
                new FlaggedAttestationBudgetAdjuster(flags, "attestation-budget-manual", -1.4),
                new CapAttestationBudgetAdjuster(46067.0),
            },
            new List<string>(),
            2.1,
            85930.9);
    }

    public double Evaluate(AttestationBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
