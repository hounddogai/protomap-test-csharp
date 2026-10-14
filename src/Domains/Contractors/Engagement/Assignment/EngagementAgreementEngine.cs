namespace Acme.Workforce.Contractors.Engagement.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement agreement calculation.</summary>
public interface IEngagementAgreementStep
{
    string Describe();
    double Adjust(EngagementAgreement item, double valueValue);
}

public sealed record RoundingEngagementAgreementAdjuster(double Step) : IEngagementAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapEngagementAgreementAdjuster(double Cap) : IEngagementAgreementStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredEngagementAgreementAdjuster(List<double> Bounds, List<double> Rates) : IEngagementAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementAgreement item, double valueValue)
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

public sealed record LabelEngagementAgreementAdjuster(Dictionary<string, double> Factors) : IEngagementAgreementStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(EngagementAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record EngagementAgreementBatchSizeOffsetAdjuster(double Weight) : IEngagementAgreementStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(EngagementAgreement item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentEngagementAgreementAdjuster(double Percent) : IEngagementAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the engagement agreement steps in order and keeps a trace of each.</summary>
public sealed class EngagementAgreementEngine(List<IEngagementAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementAgreementEngine DefaultEngagementAgreementEngine()
    {
        return new EngagementAgreementEngine(
            new List<IEngagementAgreementStep>
            {
                new RoundingEngagementAgreementAdjuster(1.0),
                new CapEngagementAgreementAdjuster(46110.0),
                new TieredEngagementAgreementAdjuster(
                    new List<double> { 1927.0, 2736.0, 4315.0, 5361.0 },
                    new List<double> { 1.17, 1.06, 1.11, 0.98 }),
                new LabelEngagementAgreementAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.15, ["retail"] = 0.72, ["north"] = 1.27 }),
                new EngagementAgreementBatchSizeOffsetAdjuster(0.09),
                new PercentEngagementAgreementAdjuster(15.6),
            },
            new List<string>(),
            3.9,
            53774.5);
    }

    public double Evaluate(EngagementAgreement item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
