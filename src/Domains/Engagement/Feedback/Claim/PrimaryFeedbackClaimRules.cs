namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary feedback claim must pass. Check returns its problems, or none.</summary>
public interface IPrimaryFeedbackClaimRule
{
    List<string> Check(PrimaryFeedbackClaim item);
}

public sealed record PrimaryFeedbackClaimPriorityRangeRule(int Min, int Max) : IPrimaryFeedbackClaimRule
{
    public List<string> Check(PrimaryFeedbackClaim item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryFeedbackClaimTitleLengthRule(int MaxLength) : IPrimaryFeedbackClaimRule
{
    public List<string> Check(PrimaryFeedbackClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFeedbackClaimAllowedCodeRule(List<string> Allowed) : IPrimaryFeedbackClaimRule
{
    public List<string> Check(PrimaryFeedbackClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFeedbackClaimCodePrefixRule(string Prefix) : IPrimaryFeedbackClaimRule
{
    public List<string> Check(PrimaryFeedbackClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFeedbackClaimEndsAtRangeRule(int Min, int Max) : IPrimaryFeedbackClaimRule
{
    public List<string> Check(PrimaryFeedbackClaim item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary feedback claim.</summary>
public sealed record PrimaryFeedbackClaimValidator(List<IPrimaryFeedbackClaimRule> Rules)
{
    public List<string> Validate(PrimaryFeedbackClaim item)
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

    public void RequireValid(PrimaryFeedbackClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryFeedbackClaim> items)
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

/// <summary>Functions of primary feedback claim rules.</summary>
public static class PrimaryFeedbackClaimRules
{
    public static PrimaryFeedbackClaimValidator DefaultPrimaryFeedbackClaimValidator()
    {
        return new PrimaryFeedbackClaimValidator(
            new List<IPrimaryFeedbackClaimRule>
            {
                new PrimaryFeedbackClaimPriorityRangeRule(32, 86400),
                new PrimaryFeedbackClaimTitleLengthRule(86400),
                new PrimaryFeedbackClaimAllowedCodeRule(new List<string> { "default", "rolling", "primary" }),
                new PrimaryFeedbackClaimCodePrefixRule("ACM"),
                new PrimaryFeedbackClaimEndsAtRangeRule(255, 3600),
            });
    }
}
