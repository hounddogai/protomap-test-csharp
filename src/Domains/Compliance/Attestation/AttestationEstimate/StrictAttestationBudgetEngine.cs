namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict attestation budget calculation.</summary>
public interface IStrictAttestationBudgetFactor
{
    string Describe();
    double Adjust(StrictAttestationBudget item, double valueValue);
}

public sealed record LabelStrictAttestationBudgetAdjuster(Dictionary<string, double> Factors) : IStrictAttestationBudgetFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictAttestationBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredStrictAttestationBudgetAdjuster(List<double> Bounds, List<double> Rates) : IStrictAttestationBudgetFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictAttestationBudget item, double valueValue)
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

public sealed record RoundingStrictAttestationBudgetAdjuster(double Step) : IStrictAttestationBudgetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictAttestationBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedStrictAttestationBudgetAdjuster(Registry Flags, string Flag, double Percent) : IStrictAttestationBudgetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictAttestationBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictAttestationBudgetAdjuster(double Cap) : IStrictAttestationBudgetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAttestationBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record StrictAttestationBudgetDurationMinutesOffsetAdjuster(double Weight) : IStrictAttestationBudgetFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(StrictAttestationBudget item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the strict attestation budget steps in order and keeps a trace of each.</summary>
public sealed class StrictAttestationBudgetEngine(List<IStrictAttestationBudgetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAttestationBudgetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAttestationBudgetEngine DefaultStrictAttestationBudgetEngine(Registry flags)
    {
        return new StrictAttestationBudgetEngine(
            new List<IStrictAttestationBudgetFactor>
            {
                new LabelStrictAttestationBudgetAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.15, ["east"] = 1.1 }),
                new TieredStrictAttestationBudgetAdjuster(
                    new List<double> { 6132.0, 6627.0, 7412.0, 7602.0 },
                    new List<double> { 1.02, 1.03, 0.99, 1.12 }),
                new RoundingStrictAttestationBudgetAdjuster(0.05),
                new FlaggedStrictAttestationBudgetAdjuster(flags, "strict-attestation-budget-regional", 8.4),
                new CapStrictAttestationBudgetAdjuster(40497.0),
                new StrictAttestationBudgetDurationMinutesOffsetAdjuster(0.74),
            },
            new List<string>(),
            4.0,
            2299.8);
    }

    public double Evaluate(StrictAttestationBudget item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAttestationBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
