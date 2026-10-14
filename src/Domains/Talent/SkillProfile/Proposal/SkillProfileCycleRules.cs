namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile cycle must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileCycleRule
{
    List<string> Check(SkillProfileCycle item);
}

public sealed record SkillProfileCycleCodeLengthRule(int MaxLength) : ISkillProfileCycleRule
{
    public List<string> Check(SkillProfileCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileCycleRequiredNameRule(string Message) : ISkillProfileCycleRule
{
    public List<string> Check(SkillProfileCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileCycleAllowedPhoneRule(List<string> Allowed) : ISkillProfileCycleRule
{
    public List<string> Check(SkillProfileCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileCycleNameLengthRule(int MaxLength) : ISkillProfileCycleRule
{
    public List<string> Check(SkillProfileCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileCycleRequiredPhoneRule(string Message) : ISkillProfileCycleRule
{
    public List<string> Check(SkillProfileCycle item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile cycle.</summary>
public sealed record SkillProfileCycleValidator(List<ISkillProfileCycleRule> Rules)
{
    public List<string> Validate(SkillProfileCycle item)
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

    public void RequireValid(SkillProfileCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileCycle> items)
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

/// <summary>Functions of skill profile cycle rules.</summary>
public static class SkillProfileCycleRules
{
    public static SkillProfileCycleValidator DefaultSkillProfileCycleValidator()
    {
        return new SkillProfileCycleValidator(
            new List<ISkillProfileCycleRule>
            {
                new SkillProfileCycleCodeLengthRule(64),
                new SkillProfileCycleRequiredNameRule("skill profile cycle needs a value"),
                new SkillProfileCycleAllowedPhoneRule(new List<string> { "primary", "scheduled", "fallback" }),
                new SkillProfileCycleNameLengthRule(120),
                new SkillProfileCycleRequiredPhoneRule("skill profile cycle needs this field"),
            });
    }
}
