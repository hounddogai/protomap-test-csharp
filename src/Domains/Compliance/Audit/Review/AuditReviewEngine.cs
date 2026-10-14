namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit review calculation.</summary>
public interface IAuditReviewStep
{
    string Describe();
    double Adjust(AuditReview item, double valueValue);
}

public sealed record RoundingAuditReviewAdjuster(double Step) : IAuditReviewStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAuditReviewAdjuster(Registry Flags, string Flag, double Percent) : IAuditReviewStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAuditReviewAdjuster(double Percent) : IAuditReviewStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AuditReviewCostOffsetAdjuster(double Weight) : IAuditReviewStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(AuditReview item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the audit review steps in order and keeps a trace of each.</summary>
public sealed class AuditReviewEngine(List<IAuditReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditReviewEngine DefaultAuditReviewEngine(Registry flags)
    {
        return new AuditReviewEngine(
            new List<IAuditReviewStep>
            {
                new RoundingAuditReviewAdjuster(0.05),
                new FlaggedAuditReviewAdjuster(flags, "audit-review-fallback", -7.3),
                new PercentAuditReviewAdjuster(-1.9),
                new AuditReviewCostOffsetAdjuster(0.15),
            },
            new List<string>(),
            4.1,
            16997.6);
    }

    public double Evaluate(AuditReview item)
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

    public Dictionary<string, double> EvaluateAll(List<AuditReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
