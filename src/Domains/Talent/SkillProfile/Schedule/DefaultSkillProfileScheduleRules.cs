namespace Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>A rule a default skill profile schedule must pass. Check returns its problems, or none.</summary>
public interface IDefaultSkillProfileScheduleRule
{
    List<string> Check(DefaultSkillProfileSchedule item);
}

public sealed record DefaultSkillProfileScheduleRequiredTitleRule(string Message) : IDefaultSkillProfileScheduleRule
{
    public List<string> Check(DefaultSkillProfileSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileScheduleExternalRefLengthRule(int MaxLength) : IDefaultSkillProfileScheduleRule
{
    public List<string> Check(DefaultSkillProfileSchedule item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileScheduleRequiredCodeRule(string Message) : IDefaultSkillProfileScheduleRule
{
    public List<string> Check(DefaultSkillProfileSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileScheduleCodePrefixRule(string Prefix) : IDefaultSkillProfileScheduleRule
{
    public List<string> Check(DefaultSkillProfileSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultSkillProfileScheduleCountryLengthRule(int MaxLength) : IDefaultSkillProfileScheduleRule
{
    public List<string> Check(DefaultSkillProfileSchedule item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default skill profile schedule.</summary>
public sealed record DefaultSkillProfileScheduleValidator(List<IDefaultSkillProfileScheduleRule> Rules)
{
    public List<string> Validate(DefaultSkillProfileSchedule item)
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

    public void RequireValid(DefaultSkillProfileSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultSkillProfileSchedule> items)
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

/// <summary>Functions of default skill profile schedule rules.</summary>
public static class DefaultSkillProfileScheduleRules
{
    public static DefaultSkillProfileScheduleValidator DefaultDefaultSkillProfileScheduleValidator()
    {
        return new DefaultSkillProfileScheduleValidator(
            new List<IDefaultSkillProfileScheduleRule>
            {
                new DefaultSkillProfileScheduleRequiredTitleRule("default skill profile schedule needs a value"),
                new DefaultSkillProfileScheduleExternalRefLengthRule(120),
                new DefaultSkillProfileScheduleRequiredCodeRule("default skill profile schedule needs an entry"),
                new DefaultSkillProfileScheduleCodePrefixRule("ACM"),
                new DefaultSkillProfileScheduleCountryLengthRule(255),
            });
    }
}
