namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>A rule a legacy action plan digest must pass. Check returns its problems, or none.</summary>
public interface ILegacyActionPlanDigestRule
{
    List<string> Check(LegacyActionPlanDigest item);
}

public sealed record LegacyActionPlanDigestRequiredCodeRule(string Message) : ILegacyActionPlanDigestRule
{
    public List<string> Check(LegacyActionPlanDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyActionPlanDigestReviewerLengthRule(int MaxLength) : ILegacyActionPlanDigestRule
{
    public List<string> Check(LegacyActionPlanDigest item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyActionPlanDigestRequiredReviewerRule(string Message) : ILegacyActionPlanDigestRule
{
    public List<string> Check(LegacyActionPlanDigest item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyActionPlanDigestCodeLengthRule(int MaxLength) : ILegacyActionPlanDigestRule
{
    public List<string> Check(LegacyActionPlanDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyActionPlanDigestRequiredTitleRule(string Message) : ILegacyActionPlanDigestRule
{
    public List<string> Check(LegacyActionPlanDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyActionPlanDigestLimitRangeRule(int Min, int Max) : ILegacyActionPlanDigestRule
{
    public List<string> Check(LegacyActionPlanDigest item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy action plan digest.</summary>
public sealed record LegacyActionPlanDigestValidator(List<ILegacyActionPlanDigestRule> Rules)
{
    public List<string> Validate(LegacyActionPlanDigest item)
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

    public void RequireValid(LegacyActionPlanDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyActionPlanDigest> items)
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

/// <summary>Functions of legacy action plan digest rules.</summary>
public static class LegacyActionPlanDigestRules
{
    public static LegacyActionPlanDigestValidator DefaultLegacyActionPlanDigestValidator()
    {
        return new LegacyActionPlanDigestValidator(
            new List<ILegacyActionPlanDigestRule>
            {
                new LegacyActionPlanDigestRequiredCodeRule("legacy action plan digest needs this field"),
                new LegacyActionPlanDigestReviewerLengthRule(80),
                new LegacyActionPlanDigestRequiredReviewerRule("legacy action plan digest needs this field"),
                new LegacyActionPlanDigestCodeLengthRule(80),
                new LegacyActionPlanDigestRequiredTitleRule("legacy action plan digest needs this field"),
                new LegacyActionPlanDigestLimitRangeRule(32, 86400),
            });
    }
}
