namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped adjustment agreement calculation.</summary>
public interface ICappedAdjustmentAgreementStep
{
    string Describe();
    double Adjust(CappedAdjustmentAgreement item, double valueValue);
}

public sealed record PercentCappedAdjustmentAgreementAdjuster(double Percent) : ICappedAdjustmentAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedAdjustmentAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCappedAdjustmentAgreementAdjuster(Registry Flags, string Flag, double Percent) : ICappedAdjustmentAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAdjustmentAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameCappedAdjustmentAgreementAdjuster(Dictionary<string, double> Factors) : ICappedAdjustmentAgreementStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(CappedAdjustmentAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedAdjustmentAgreementAdjuster(double Cap) : ICappedAdjustmentAgreementStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAdjustmentAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped adjustment agreement steps in order and keeps a trace of each.</summary>
public sealed class CappedAdjustmentAgreementEngine(List<ICappedAdjustmentAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAdjustmentAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAdjustmentAgreementEngine DefaultCappedAdjustmentAgreementEngine(Registry flags)
    {
        return new CappedAdjustmentAgreementEngine(
            new List<ICappedAdjustmentAgreementStep>
            {
                new PercentCappedAdjustmentAgreementAdjuster(17.1),
                new FlaggedCappedAdjustmentAgreementAdjuster(flags, "capped-adjustment-agreement-regional", 9.3),
                new NameCappedAdjustmentAgreementAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.32, ["east"] = 0.92 }),
                new CapCappedAdjustmentAgreementAdjuster(7544.0),
            },
            new List<string>(),
            1.0,
            18506.8);
    }

    public double Evaluate(CappedAdjustmentAgreement item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAdjustmentAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
