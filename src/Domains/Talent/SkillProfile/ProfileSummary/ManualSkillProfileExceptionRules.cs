namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

/// <summary>A rule a manual skill profile exception must pass. Check returns its problems, or none.</summary>
public interface IManualSkillProfileExceptionRule
{
    List<string> Check(ManualSkillProfileException item);
}

public sealed record ManualSkillProfileExceptionCodeLengthRule(int MaxLength) : IManualSkillProfileExceptionRule
{
    public List<string> Check(ManualSkillProfileException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSkillProfileExceptionRequiredEmployeeIdRule(string Message) : IManualSkillProfileExceptionRule
{
    public List<string> Check(ManualSkillProfileException item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualSkillProfileExceptionRequiredCategoryRule(string Message) : IManualSkillProfileExceptionRule
{
    public List<string> Check(ManualSkillProfileException item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualSkillProfileExceptionNameLengthRule(int MaxLength) : IManualSkillProfileExceptionRule
{
    public List<string> Check(ManualSkillProfileException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualSkillProfileExceptionRequiredNameRule(string Message) : IManualSkillProfileExceptionRule
{
    public List<string> Check(ManualSkillProfileException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualSkillProfileExceptionThresholdRangeRule(double Min, double Max) : IManualSkillProfileExceptionRule
{
    public List<string> Check(ManualSkillProfileException item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual skill profile exception.</summary>
public sealed record ManualSkillProfileExceptionValidator(List<IManualSkillProfileExceptionRule> Rules)
{
    public List<string> Validate(ManualSkillProfileException item)
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

    public void RequireValid(ManualSkillProfileException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualSkillProfileException> items)
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

/// <summary>Functions of manual skill profile exception rules.</summary>
public static class ManualSkillProfileExceptionRules
{
    public static ManualSkillProfileExceptionValidator DefaultManualSkillProfileExceptionValidator()
    {
        return new ManualSkillProfileExceptionValidator(
            new List<IManualSkillProfileExceptionRule>
            {
                new ManualSkillProfileExceptionCodeLengthRule(80),
                new ManualSkillProfileExceptionRequiredEmployeeIdRule(
                    "manual skill profile exception needs an entry"),
                new ManualSkillProfileExceptionRequiredCategoryRule(
                    "manual skill profile exception needs this field"),
                new ManualSkillProfileExceptionNameLengthRule(64),
                new ManualSkillProfileExceptionRequiredNameRule("manual skill profile exception needs an entry"),
                new ManualSkillProfileExceptionThresholdRangeRule(0.6, 1111.0),
            });
    }
}
