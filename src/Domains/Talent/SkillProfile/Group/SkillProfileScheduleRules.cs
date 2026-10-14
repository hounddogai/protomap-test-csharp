namespace Acme.Workforce.Talent.SkillProfile.Group;

/// <summary>A rule a skill profile schedule must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileScheduleRule
{
    List<string> Check(SkillProfileSchedule item);
}

public sealed record SkillProfileScheduleRequiredChecksumRule(string Message) : ISkillProfileScheduleRule
{
    public List<string> Check(SkillProfileSchedule item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileScheduleRequiredCodeRule(string Message) : ISkillProfileScheduleRule
{
    public List<string> Check(SkillProfileSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileScheduleChecksumLengthRule(int MaxLength) : ISkillProfileScheduleRule
{
    public List<string> Check(SkillProfileSchedule item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile schedule.</summary>
public sealed record SkillProfileScheduleValidator(List<ISkillProfileScheduleRule> Rules)
{
    public List<string> Validate(SkillProfileSchedule item)
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

    public void RequireValid(SkillProfileSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileSchedule> items)
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

/// <summary>Functions of skill profile schedule rules.</summary>
public static class SkillProfileScheduleRules
{
    public static SkillProfileScheduleValidator DefaultSkillProfileScheduleValidator()
    {
        return new SkillProfileScheduleValidator(
            new List<ISkillProfileScheduleRule>
            {
                new SkillProfileScheduleRequiredChecksumRule("skill profile schedule needs an entry"),
                new SkillProfileScheduleRequiredCodeRule("skill profile schedule needs an entry"),
                new SkillProfileScheduleChecksumLengthRule(16),
            });
    }
}
