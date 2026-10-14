namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched merit cycle review must pass. Check returns its problems, or none.</summary>
public interface IBatchedMeritCycleReviewRule
{
    List<string> Check(BatchedMeritCycleReview item);
}

public sealed record BatchedMeritCycleReviewFteRangeRule(double Min, double Max) : IBatchedMeritCycleReviewRule
{
    public List<string> Check(BatchedMeritCycleReview item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMeritCycleReviewCodePrefixRule(string Prefix) : IBatchedMeritCycleReviewRule
{
    public List<string> Check(BatchedMeritCycleReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMeritCycleReviewAllowedRegionRule(List<string> Allowed) : IBatchedMeritCycleReviewRule
{
    public List<string> Check(BatchedMeritCycleReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is stale" };
        }
        return new List<string>();
    }
}

public sealed record BatchedMeritCycleReviewRequiredRegionRule(string Message) : IBatchedMeritCycleReviewRule
{
    public List<string> Check(BatchedMeritCycleReview item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched merit cycle review.</summary>
public sealed record BatchedMeritCycleReviewValidator(List<IBatchedMeritCycleReviewRule> Rules)
{
    public List<string> Validate(BatchedMeritCycleReview item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(BatchedMeritCycleReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedMeritCycleReview> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of batched merit cycle review rules.</summary>
public static class BatchedMeritCycleReviewRules
{
    public static BatchedMeritCycleReviewValidator DefaultBatchedMeritCycleReviewValidator()
    {
        return new BatchedMeritCycleReviewValidator(
            new List<IBatchedMeritCycleReviewRule>
            {
                new BatchedMeritCycleReviewFteRangeRule(3.9, 4360.2),
                new BatchedMeritCycleReviewCodePrefixRule("OPS"),
                new BatchedMeritCycleReviewAllowedRegionRule(new List<string> { "scheduled", "tiered", "strict" }),
                new BatchedMeritCycleReviewRequiredRegionRule("batched merit cycle review needs an entry"),
            });
    }
}
