namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount rule must pass. Check returns its problems, or none.</summary>
public interface IHeadcountRuleRule
{
    List<string> Check(HeadcountRule item);
}

public sealed record HeadcountRuleAllowedPhoneRule(List<string> Allowed) : IHeadcountRuleRule
{
    public List<string> Check(HeadcountRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is missing" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRuleTagsLimitRule(int MaxCount, List<string> Banned) : IHeadcountRuleRule
{
    public List<string> Check(HeadcountRule item)
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

public sealed record HeadcountRuleRequiredChannelRule(string Message) : IHeadcountRuleRule
{
    public List<string> Check(HeadcountRule item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRuleChannelLengthRule(int MaxLength) : IHeadcountRuleRule
{
    public List<string> Check(HeadcountRule item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountRuleRequiredTeamRule(string Message) : IHeadcountRuleRule
{
    public List<string> Check(HeadcountRule item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount rule.</summary>
public sealed record HeadcountRuleValidator(List<IHeadcountRuleRule> Rules)
{
    public List<string> Validate(HeadcountRule item)
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

    public void RequireValid(HeadcountRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountRule> items)
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

/// <summary>Functions of headcount rule rules.</summary>
public static class HeadcountRuleRules
{
    public static HeadcountRuleValidator DefaultHeadcountRuleValidator()
    {
        return new HeadcountRuleValidator(
            new List<IHeadcountRuleRule>
            {
                new HeadcountRuleAllowedPhoneRule(new List<string> { "tiered", "pending" }),
                new HeadcountRuleTagsLimitRule(86400, new List<string> { "lenient", "rolling", "weighted" }),
                new HeadcountRuleRequiredChannelRule("headcount rule needs a value"),
                new HeadcountRuleChannelLengthRule(1000),
                new HeadcountRuleRequiredTeamRule("headcount rule needs an entry"),
            });
    }
}
