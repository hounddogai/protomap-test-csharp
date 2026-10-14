namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation plan calculation.</summary>
public interface IAttestationPlanStep
{
    string Describe();
    double Adjust(AttestationPlan item, double valueValue);
}

public sealed record AttestationPlanLeaveDaysOffsetAdjuster(double Weight) : IAttestationPlanStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(AttestationPlan item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record PercentAttestationPlanAdjuster(double Percent) : IAttestationPlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationPlanAdjuster(List<double> Bounds, List<double> Rates) : IAttestationPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationPlan item, double valueValue)
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

public sealed record TeamAttestationPlanAdjuster(Dictionary<string, double> Factors) : IAttestationPlanStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(AttestationPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAttestationPlanAdjuster(double Step) : IAttestationPlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the attestation plan steps in order and keeps a trace of each.</summary>
public sealed class AttestationPlanEngine(List<IAttestationPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationPlanEngine DefaultAttestationPlanEngine()
    {
        return new AttestationPlanEngine(
            new List<IAttestationPlanStep>
            {
                new AttestationPlanLeaveDaysOffsetAdjuster(0.35),
                new PercentAttestationPlanAdjuster(16.5),
                new TieredAttestationPlanAdjuster(
                    new List<double> { 2084.0, 8026.0 },
                    new List<double> { 1.13, 1.18 }),
                new TeamAttestationPlanAdjuster(new Dictionary<string, double> { ["central"] = 0.98 }),
                new RoundingAttestationPlanAdjuster(5.0),
            },
            new List<string>(),
            4.7,
            11875.4);
    }

    public double Evaluate(AttestationPlan item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
