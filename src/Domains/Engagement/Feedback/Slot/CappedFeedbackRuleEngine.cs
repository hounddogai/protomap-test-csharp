namespace Acme.Workforce.Engagement.Feedback.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped feedback rule calculation.</summary>
public interface ICappedFeedbackRuleStep
{
    string Describe();
    double Adjust(CappedFeedbackRule item, double valueValue);
}

public sealed record TeamCappedFeedbackRuleAdjuster(Dictionary<string, double> Factors) : ICappedFeedbackRuleStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(CappedFeedbackRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCappedFeedbackRuleAdjuster(List<double> Bounds, List<double> Rates) : ICappedFeedbackRuleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedFeedbackRule item, double valueValue)
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

public sealed record PercentCappedFeedbackRuleAdjuster(double Percent) : ICappedFeedbackRuleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedFeedbackRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedFeedbackRuleCeilingOffsetAdjuster(double Weight) : ICappedFeedbackRuleStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(CappedFeedbackRule item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record RoundingCappedFeedbackRuleAdjuster(double Step) : ICappedFeedbackRuleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedFeedbackRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped feedback rule steps in order and keeps a trace of each.</summary>
public sealed class CappedFeedbackRuleEngine(List<ICappedFeedbackRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedFeedbackRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedFeedbackRuleEngine DefaultCappedFeedbackRuleEngine()
    {
        return new CappedFeedbackRuleEngine(
            new List<ICappedFeedbackRuleStep>
            {
                new TeamCappedFeedbackRuleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.05, ["east"] = 1.13 }),
                new TieredCappedFeedbackRuleAdjuster(
                    new List<double> { 1837.0, 1992.0, 6151.0, 8354.0 },
                    new List<double> { 0.91, 0.82, 0.82, 0.91 }),
                new PercentCappedFeedbackRuleAdjuster(17.4),
                new CappedFeedbackRuleCeilingOffsetAdjuster(0.12),
                new RoundingCappedFeedbackRuleAdjuster(0.5),
            },
            new List<string>(),
            0.6,
            46700.0);
    }

    public double Run(CappedFeedbackRule item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedFeedbackRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
