namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a merit cycle review must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleReviewRule
{
    List<string> Check(MeritCycleReview item);
}

public sealed record MeritCycleReviewAllowedTitleRule(List<string> Allowed) : IMeritCycleReviewRule
{
    public List<string> Check(MeritCycleReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleReviewRequiredTitleRule(string Message) : IMeritCycleReviewRule
{
    public List<string> Check(MeritCycleReview item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleReviewCodePrefixRule(string Prefix) : IMeritCycleReviewRule
{
    public List<string> Check(MeritCycleReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle review.</summary>
public sealed record MeritCycleReviewValidator(List<IMeritCycleReviewRule> Rules)
{
    public List<string> Validate(MeritCycleReview item)
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

    public void RequireValid(MeritCycleReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleReview> items)
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

/// <summary>Functions of merit cycle review rules.</summary>
public static class MeritCycleReviewRules
{
    public static MeritCycleReviewValidator DefaultMeritCycleReviewValidator()
    {
        return new MeritCycleReviewValidator(
            new List<IMeritCycleReviewRule>
            {
                new MeritCycleReviewAllowedTitleRule(new List<string> { "regional", "pending" }),
                new MeritCycleReviewRequiredTitleRule("merit cycle review needs this field"),
                new MeritCycleReviewCodePrefixRule("OPS"),
            });
    }
}
