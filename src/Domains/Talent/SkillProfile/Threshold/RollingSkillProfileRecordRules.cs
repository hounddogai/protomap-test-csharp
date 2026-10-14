namespace Acme.Workforce.Talent.SkillProfile.Threshold;

/// <summary>A rule a rolling skill profile record must pass. Check returns its problems, or none.</summary>
public interface IRollingSkillProfileRecordRule
{
    List<string> Check(RollingSkillProfileRecord item);
}

public sealed record RollingSkillProfileRecordRequiredNameRule(string Message) : IRollingSkillProfileRecordRule
{
    public List<string> Check(RollingSkillProfileRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSkillProfileRecordEmployeeIdLengthRule(int MaxLength) : IRollingSkillProfileRecordRule
{
    public List<string> Check(RollingSkillProfileRecord item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSkillProfileRecordRequiredEmployeeIdRule(string Message) : IRollingSkillProfileRecordRule
{
    public List<string> Check(RollingSkillProfileRecord item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSkillProfileRecordNameLengthRule(int MaxLength) : IRollingSkillProfileRecordRule
{
    public List<string> Check(RollingSkillProfileRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling skill profile record.</summary>
public sealed record RollingSkillProfileRecordValidator(List<IRollingSkillProfileRecordRule> Rules)
{
    public List<string> Validate(RollingSkillProfileRecord item)
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

    public void RequireValid(RollingSkillProfileRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSkillProfileRecord> items)
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

/// <summary>Functions of rolling skill profile record rules.</summary>
public static class RollingSkillProfileRecordRules
{
    public static RollingSkillProfileRecordValidator DefaultRollingSkillProfileRecordValidator()
    {
        return new RollingSkillProfileRecordValidator(
            new List<IRollingSkillProfileRecordRule>
            {
                new RollingSkillProfileRecordRequiredNameRule("rolling skill profile record needs a value"),
                new RollingSkillProfileRecordEmployeeIdLengthRule(80),
                new RollingSkillProfileRecordRequiredEmployeeIdRule("rolling skill profile record needs this field"),
                new RollingSkillProfileRecordNameLengthRule(16),
            });
    }
}
