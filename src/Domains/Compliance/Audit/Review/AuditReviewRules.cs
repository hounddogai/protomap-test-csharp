namespace Acme.Workforce.Compliance.Audit.Review;

/// <summary>A rule a audit review must pass. Check returns its problems, or none.</summary>
public interface IAuditReviewRule
{
    List<string> Check(AuditReview item);
}

public sealed record AuditReviewCostCenterLengthRule(int MaxLength) : IAuditReviewRule
{
    public List<string> Check(AuditReview item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditReviewCodePrefixRule(string Prefix) : IAuditReviewRule
{
    public List<string> Check(AuditReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AuditReviewRequiredCostCenterRule(string Message) : IAuditReviewRule
{
    public List<string> Check(AuditReview item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditReviewVisibleGuardRule(double Limit) : IAuditReviewRule
{
    public List<string> Check(AuditReview item)
    {
        if (item.Visible && item.Rating > Limit)
        {
            return new List<string> { $"rating {item.Rating} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit review.</summary>
public sealed record AuditReviewValidator(List<IAuditReviewRule> Rules)
{
    public List<string> Validate(AuditReview item)
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

    public void RequireValid(AuditReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditReview> items)
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

/// <summary>Functions of audit review rules.</summary>
public static class AuditReviewRules
{
    public static AuditReviewValidator DefaultAuditReviewValidator()
    {
        return new AuditReviewValidator(
            new List<IAuditReviewRule>
            {
                new AuditReviewCostCenterLengthRule(16),
                new AuditReviewCodePrefixRule("OPS"),
                new AuditReviewRequiredCostCenterRule("audit review needs this field"),
                new AuditReviewVisibleGuardRule(2.4),
            });
    }
}
