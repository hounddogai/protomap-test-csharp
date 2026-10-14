namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict skill profile entry must pass. Check returns its problems, or none.</summary>
public interface IStrictSkillProfileEntryRule
{
    List<string> Check(StrictSkillProfileEntry item);
}

public sealed record StrictSkillProfileEntryAllowedTeamRule(List<string> Allowed) : IStrictSkillProfileEntryRule
{
    public List<string> Check(StrictSkillProfileEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is too long" };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileEntryFteRangeRule(double Min, double Max) : IStrictSkillProfileEntryRule
{
    public List<string> Check(StrictSkillProfileEntry item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileEntrySalaryBandLengthRule(int MaxLength) : IStrictSkillProfileEntryRule
{
    public List<string> Check(StrictSkillProfileEntry item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileEntryCodeLengthRule(int MaxLength) : IStrictSkillProfileEntryRule
{
    public List<string> Check(StrictSkillProfileEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileEntryRequiredTitleRule(string Message) : IStrictSkillProfileEntryRule
{
    public List<string> Check(StrictSkillProfileEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictSkillProfileEntryRequiredSalaryBandRule(string Message) : IStrictSkillProfileEntryRule
{
    public List<string> Check(StrictSkillProfileEntry item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict skill profile entry.</summary>
public sealed record StrictSkillProfileEntryValidator(List<IStrictSkillProfileEntryRule> Rules)
{
    public List<string> Validate(StrictSkillProfileEntry item)
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

    public void RequireValid(StrictSkillProfileEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictSkillProfileEntry> items)
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

/// <summary>Functions of strict skill profile entry rules.</summary>
public static class StrictSkillProfileEntryRules
{
    public static StrictSkillProfileEntryValidator DefaultStrictSkillProfileEntryValidator()
    {
        return new StrictSkillProfileEntryValidator(
            new List<IStrictSkillProfileEntryRule>
            {
                new StrictSkillProfileEntryAllowedTeamRule(new List<string> { "pending", "rolling" }),
                new StrictSkillProfileEntryFteRangeRule(8.5, 357.8),
                new StrictSkillProfileEntrySalaryBandLengthRule(1000),
                new StrictSkillProfileEntryCodeLengthRule(16),
                new StrictSkillProfileEntryRequiredTitleRule("strict skill profile entry needs this field"),
                new StrictSkillProfileEntryRequiredSalaryBandRule("strict skill profile entry needs this field"),
            });
    }
}
