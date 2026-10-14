namespace Acme.Workforce.Talent.SkillProfile.Window;

/// <summary>A rule a capped skill profile cycle must pass. Check returns its problems, or none.</summary>
public interface ICappedSkillProfileCycleRule
{
    List<string> Check(CappedSkillProfileCycle item);
}

public sealed record CappedSkillProfileCycleDescriptionLengthRule(int MaxLength) : ICappedSkillProfileCycleRule
{
    public List<string> Check(CappedSkillProfileCycle item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileCycleRequiredDescriptionRule(string Message) : ICappedSkillProfileCycleRule
{
    public List<string> Check(CappedSkillProfileCycle item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedSkillProfileCycleCreatedAtRangeRule(int Min, int Max) : ICappedSkillProfileCycleRule
{
    public List<string> Check(CappedSkillProfileCycle item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedSkillProfileCycleRequiredCodeRule(string Message) : ICappedSkillProfileCycleRule
{
    public List<string> Check(CappedSkillProfileCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped skill profile cycle.</summary>
public sealed record CappedSkillProfileCycleValidator(List<ICappedSkillProfileCycleRule> Rules)
{
    public List<string> Validate(CappedSkillProfileCycle item)
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

    public void RequireValid(CappedSkillProfileCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedSkillProfileCycle> items)
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

/// <summary>Functions of capped skill profile cycle rules.</summary>
public static class CappedSkillProfileCycleRules
{
    public static CappedSkillProfileCycleValidator DefaultCappedSkillProfileCycleValidator()
    {
        return new CappedSkillProfileCycleValidator(
            new List<ICappedSkillProfileCycleRule>
            {
                new CappedSkillProfileCycleDescriptionLengthRule(1000),
                new CappedSkillProfileCycleRequiredDescriptionRule("capped skill profile cycle needs a value"),
                new CappedSkillProfileCycleCreatedAtRangeRule(120, 255),
                new CappedSkillProfileCycleRequiredCodeRule("capped skill profile cycle needs a value"),
            });
    }
}
