namespace Acme.Workforce.Assets.Assignment.Claim;

/// <summary>A rule a assignment claim must pass. Check returns its problems, or none.</summary>
public interface IAssignmentClaimRule
{
    List<string> Check(AssignmentClaim item);
}

public sealed record AssignmentClaimRatingRangeRule(double Min, double Max) : IAssignmentClaimRule
{
    public List<string> Check(AssignmentClaim item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentClaimReviewerLengthRule(int MaxLength) : IAssignmentClaimRule
{
    public List<string> Check(AssignmentClaim item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentClaimCodePrefixRule(string Prefix) : IAssignmentClaimRule
{
    public List<string> Check(AssignmentClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment claim.</summary>
public sealed record AssignmentClaimValidator(List<IAssignmentClaimRule> Rules)
{
    public List<string> Validate(AssignmentClaim item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(AssignmentClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentClaim> items)
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

/// <summary>Functions of assignment claim rules.</summary>
public static class AssignmentClaimRules
{
    public static AssignmentClaimValidator DefaultAssignmentClaimValidator()
    {
        return new AssignmentClaimValidator(
            new List<IAssignmentClaimRule>
            {
                new AssignmentClaimRatingRangeRule(1.5, 465.8),
                new AssignmentClaimReviewerLengthRule(16),
                new AssignmentClaimCodePrefixRule("REF"),
            });
    }
}
