namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

/// <summary>A rule a default skill profile group must pass. Check returns its problems, or none.</summary>
public interface IDefaultSkillProfileGroupRule
{
    List<string> Check(DefaultSkillProfileGroup item);
}

public sealed record DefaultSkillProfileGroupTitleLengthRule(int MaxLength) : IDefaultSkillProfileGroupRule
{
    public List<string> Check(DefaultSkillProfileGroup item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileGroupBatchSizeRangeRule(int Min, int Max) : IDefaultSkillProfileGroupRule
{
    public List<string> Check(DefaultSkillProfileGroup item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultSkillProfileGroupLabelLengthRule(int MaxLength) : IDefaultSkillProfileGroupRule
{
    public List<string> Check(DefaultSkillProfileGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileGroupRequiredTitleRule(string Message) : IDefaultSkillProfileGroupRule
{
    public List<string> Check(DefaultSkillProfileGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileGroupRequiredLabelRule(string Message) : IDefaultSkillProfileGroupRule
{
    public List<string> Check(DefaultSkillProfileGroup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default skill profile group.</summary>
public sealed record DefaultSkillProfileGroupValidator(List<IDefaultSkillProfileGroupRule> Rules)
{
    public List<string> Validate(DefaultSkillProfileGroup item)
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

    public void RequireValid(DefaultSkillProfileGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSkillProfileGroup> items)
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

/// <summary>Functions of default skill profile group rules.</summary>
public static class DefaultSkillProfileGroupRules
{
    public static DefaultSkillProfileGroupValidator DefaultDefaultSkillProfileGroupValidator()
    {
        return new DefaultSkillProfileGroupValidator(
            new List<IDefaultSkillProfileGroupRule>
            {
                new DefaultSkillProfileGroupTitleLengthRule(16),
                new DefaultSkillProfileGroupBatchSizeRangeRule(1000, 3600),
                new DefaultSkillProfileGroupLabelLengthRule(16),
                new DefaultSkillProfileGroupRequiredTitleRule("default skill profile group needs an entry"),
                new DefaultSkillProfileGroupRequiredLabelRule("default skill profile group needs this field"),
            });
    }
}
