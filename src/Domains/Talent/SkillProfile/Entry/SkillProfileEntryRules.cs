namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile entry must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileEntryRule
{
    List<string> Check(SkillProfileEntry item);
}

public sealed record SkillProfileEntryAllowedLabelRule(List<string> Allowed) : ISkillProfileEntryRule
{
    public List<string> Check(SkillProfileEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileEntryRequiredCodeRule(string Message) : ISkillProfileEntryRule
{
    public List<string> Check(SkillProfileEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileEntryCodeLengthRule(int MaxLength) : ISkillProfileEntryRule
{
    public List<string> Check(SkillProfileEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileEntryLabelLengthRule(int MaxLength) : ISkillProfileEntryRule
{
    public List<string> Check(SkillProfileEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileEntryLimitRangeRule(int Min, int Max) : ISkillProfileEntryRule
{
    public List<string> Check(SkillProfileEntry item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileEntryArchivedGuardRule(int Limit) : ISkillProfileEntryRule
{
    public List<string> Check(SkillProfileEntry item)
    {
        if (item.Archived && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile entry.</summary>
public sealed record SkillProfileEntryValidator(List<ISkillProfileEntryRule> Rules)
{
    public List<string> Validate(SkillProfileEntry item)
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

    public void RequireValid(SkillProfileEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileEntry> items)
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

/// <summary>Functions of skill profile entry rules.</summary>
public static class SkillProfileEntryRules
{
    public static SkillProfileEntryValidator DefaultSkillProfileEntryValidator()
    {
        return new SkillProfileEntryValidator(
            new List<ISkillProfileEntryRule>
            {
                new SkillProfileEntryAllowedLabelRule(new List<string> { "weighted", "legacy" }),
                new SkillProfileEntryRequiredCodeRule("skill profile entry needs an entry"),
                new SkillProfileEntryCodeLengthRule(120),
                new SkillProfileEntryLabelLengthRule(3600),
                new SkillProfileEntryLimitRangeRule(120, 1000),
                new SkillProfileEntryArchivedGuardRule(32),
            });
    }
}
