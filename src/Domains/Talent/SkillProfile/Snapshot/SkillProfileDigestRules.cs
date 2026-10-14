namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>A rule a skill profile digest must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileDigestRule
{
    List<string> Check(SkillProfileDigest item);
}

public sealed record SkillProfileDigestManagerIdLengthRule(int MaxLength) : ISkillProfileDigestRule
{
    public List<string> Check(SkillProfileDigest item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileDigestRequiredTitleRule(string Message) : ISkillProfileDigestRule
{
    public List<string> Check(SkillProfileDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileDigestCodeLengthRule(int MaxLength) : ISkillProfileDigestRule
{
    public List<string> Check(SkillProfileDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileDigestChannelLengthRule(int MaxLength) : ISkillProfileDigestRule
{
    public List<string> Check(SkillProfileDigest item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile digest.</summary>
public sealed record SkillProfileDigestValidator(List<ISkillProfileDigestRule> Rules)
{
    public List<string> Validate(SkillProfileDigest item)
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

    public void RequireValid(SkillProfileDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileDigest> items)
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

/// <summary>Functions of skill profile digest rules.</summary>
public static class SkillProfileDigestRules
{
    public static SkillProfileDigestValidator DefaultSkillProfileDigestValidator()
    {
        return new SkillProfileDigestValidator(
            new List<ISkillProfileDigestRule>
            {
                new SkillProfileDigestManagerIdLengthRule(1000),
                new SkillProfileDigestRequiredTitleRule("skill profile digest needs an entry"),
                new SkillProfileDigestCodeLengthRule(86400),
                new SkillProfileDigestChannelLengthRule(64),
            });
    }
}
