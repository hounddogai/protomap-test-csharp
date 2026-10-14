namespace Acme.Workforce.Compensation.BonusPool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling bonus pool proposal must pass. Check returns its problems, or none.</summary>
public interface IRollingBonusPoolProposalRule
{
    List<string> Check(RollingBonusPoolProposal item);
}

public sealed record RollingBonusPoolProposalRequiredReviewerRule(string Message) : IRollingBonusPoolProposalRule
{
    public List<string> Check(RollingBonusPoolProposal item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolProposalAllowedTimezoneRule(List<string> Allowed) : IRollingBonusPoolProposalRule
{
    public List<string> Check(RollingBonusPoolProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolProposalRequiredTitleRule(string Message) : IRollingBonusPoolProposalRule
{
    public List<string> Check(RollingBonusPoolProposal item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolProposalTitleLengthRule(int MaxLength) : IRollingBonusPoolProposalRule
{
    public List<string> Check(RollingBonusPoolProposal item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBonusPoolProposalTagsLimitRule(int MaxCount, List<string> Banned) : IRollingBonusPoolProposalRule
{
    public List<string> Check(RollingBonusPoolProposal item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record RollingBonusPoolProposalCodeLengthRule(int MaxLength) : IRollingBonusPoolProposalRule
{
    public List<string> Check(RollingBonusPoolProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling bonus pool proposal.</summary>
public sealed record RollingBonusPoolProposalValidator(List<IRollingBonusPoolProposalRule> Rules)
{
    public List<string> Validate(RollingBonusPoolProposal item)
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

    public void RequireValid(RollingBonusPoolProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBonusPoolProposal> items)
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

/// <summary>Functions of rolling bonus pool proposal rules.</summary>
public static class RollingBonusPoolProposalRules
{
    public static RollingBonusPoolProposalValidator DefaultRollingBonusPoolProposalValidator()
    {
        return new RollingBonusPoolProposalValidator(
            new List<IRollingBonusPoolProposalRule>
            {
                new RollingBonusPoolProposalRequiredReviewerRule("rolling bonus pool proposal needs this field"),
                new RollingBonusPoolProposalAllowedTimezoneRule(new List<string> { "regional", "legacy" }),
                new RollingBonusPoolProposalRequiredTitleRule("rolling bonus pool proposal needs this field"),
                new RollingBonusPoolProposalTitleLengthRule(1000),
                new RollingBonusPoolProposalTagsLimitRule(
                    3600,
                    new List<string> { "seasonal", "seasonal", "primary" }),
                new RollingBonusPoolProposalCodeLengthRule(86400),
            });
    }
}
