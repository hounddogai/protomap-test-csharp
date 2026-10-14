namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending skill profile group must pass. Check returns its problems, or none.</summary>
public interface IPendingSkillProfileGroupRule
{
    List<string> Check(PendingSkillProfileGroup item);
}

public sealed record PendingSkillProfileGroupTitleLengthRule(int MaxLength) : IPendingSkillProfileGroupRule
{
    public List<string> Check(PendingSkillProfileGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileGroupAllowedTitleRule(List<string> Allowed) : IPendingSkillProfileGroupRule
{
    public List<string> Check(PendingSkillProfileGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PendingSkillProfileGroupRequiredCodeRule(string Message) : IPendingSkillProfileGroupRule
{
    public List<string> Check(PendingSkillProfileGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending skill profile group.</summary>
public sealed record PendingSkillProfileGroupValidator(List<IPendingSkillProfileGroupRule> Rules)
{
    public List<string> Validate(PendingSkillProfileGroup item)
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

    public void RequireValid(PendingSkillProfileGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSkillProfileGroup> items)
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

/// <summary>Functions of pending skill profile group rules.</summary>
public static class PendingSkillProfileGroupRules
{
    public static PendingSkillProfileGroupValidator DefaultPendingSkillProfileGroupValidator()
    {
        return new PendingSkillProfileGroupValidator(
            new List<IPendingSkillProfileGroupRule>
            {
                new PendingSkillProfileGroupTitleLengthRule(255),
                new PendingSkillProfileGroupAllowedTitleRule(new List<string> { "pending" }),
                new PendingSkillProfileGroupRequiredCodeRule("pending skill profile group needs an entry"),
            });
    }
}
