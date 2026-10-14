namespace Acme.Workforce.Contractors.Engagement.Digest;

/// <summary>A rule a rolling engagement digest must pass. Check returns its problems, or none.</summary>
public interface IRollingEngagementDigestRule
{
    List<string> Check(RollingEngagementDigest item);
}

public sealed record RollingEngagementDigestRequiredCostCenterRule(string Message) : IRollingEngagementDigestRule
{
    public List<string> Check(RollingEngagementDigest item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingEngagementDigestCodeLengthRule(int MaxLength) : IRollingEngagementDigestRule
{
    public List<string> Check(RollingEngagementDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingEngagementDigestRequiredBadgeRule(string Message) : IRollingEngagementDigestRule
{
    public List<string> Check(RollingEngagementDigest item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingEngagementDigestRequiredLabelRule(string Message) : IRollingEngagementDigestRule
{
    public List<string> Check(RollingEngagementDigest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling engagement digest.</summary>
public sealed record RollingEngagementDigestValidator(List<IRollingEngagementDigestRule> Rules)
{
    public List<string> Validate(RollingEngagementDigest item)
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

    public void RequireValid(RollingEngagementDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingEngagementDigest> items)
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

/// <summary>Functions of rolling engagement digest rules.</summary>
public static class RollingEngagementDigestRules
{
    public static RollingEngagementDigestValidator DefaultRollingEngagementDigestValidator()
    {
        return new RollingEngagementDigestValidator(
            new List<IRollingEngagementDigestRule>
            {
                new RollingEngagementDigestRequiredCostCenterRule("rolling engagement digest needs an entry"),
                new RollingEngagementDigestCodeLengthRule(255),
                new RollingEngagementDigestRequiredBadgeRule("rolling engagement digest needs a value"),
                new RollingEngagementDigestRequiredLabelRule("rolling engagement digest needs a value"),
            });
    }
}
