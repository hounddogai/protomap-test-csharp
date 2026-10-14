namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile exception must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileExceptionRule
{
    List<string> Check(SkillProfileException item);
}

public sealed record SkillProfileExceptionTitleLengthRule(int MaxLength) : ISkillProfileExceptionRule
{
    public List<string> Check(SkillProfileException item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileExceptionAllowedCodeRule(List<string> Allowed) : ISkillProfileExceptionRule
{
    public List<string> Check(SkillProfileException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileExceptionRequiredCodeRule(string Message) : ISkillProfileExceptionRule
{
    public List<string> Check(SkillProfileException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileExceptionThresholdRangeRule(double Min, double Max) : ISkillProfileExceptionRule
{
    public List<string> Check(SkillProfileException item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile exception.</summary>
public sealed record SkillProfileExceptionValidator(List<ISkillProfileExceptionRule> Rules)
{
    public List<string> Validate(SkillProfileException item)
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

    public void RequireValid(SkillProfileException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileException> items)
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

/// <summary>Functions of skill profile exception rules.</summary>
public static class SkillProfileExceptionRules
{
    public static SkillProfileExceptionValidator DefaultSkillProfileExceptionValidator()
    {
        return new SkillProfileExceptionValidator(
            new List<ISkillProfileExceptionRule>
            {
                new SkillProfileExceptionTitleLengthRule(16),
                new SkillProfileExceptionAllowedCodeRule(new List<string>()),
                new SkillProfileExceptionRequiredCodeRule("skill profile exception needs this field"),
                new SkillProfileExceptionThresholdRangeRule(6.5, 1212.9),
            });
    }
}
