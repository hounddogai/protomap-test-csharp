namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan claim calculation.</summary>
public interface ISuccessionPlanClaimStep
{
    string Describe();
    double Adjust(SuccessionPlanClaim item, double valueValue);
}

public sealed record PercentSuccessionPlanClaimAdjuster(double Percent) : ISuccessionPlanClaimStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleSuccessionPlanClaimAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanClaimStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SuccessionPlanClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record SuccessionPlanClaimGraceDaysOffsetAdjuster(double Weight) : ISuccessionPlanClaimStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(SuccessionPlanClaim item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the succession plan claim steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanClaimEngine(List<ISuccessionPlanClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanClaimEngine DefaultSuccessionPlanClaimEngine()
    {
        return new SuccessionPlanClaimEngine(
            new List<ISuccessionPlanClaimStep>
            {
                new PercentSuccessionPlanClaimAdjuster(5.9),
                new TitleSuccessionPlanClaimAdjuster(new Dictionary<string, double> { ["east"] = 0.88 }),
                new SuccessionPlanClaimGraceDaysOffsetAdjuster(0.49),
            },
            new List<string>(),
            0.3,
            5190.0);
    }

    public double Price(SuccessionPlanClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
