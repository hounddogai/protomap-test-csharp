namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile record must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileRecordRule
{
    List<string> Check(SkillProfileRecord item);
}

public sealed record SkillProfileRecordLeaveDaysRangeRule(int Min, int Max) : ISkillProfileRecordRule
{
    public List<string> Check(SkillProfileRecord item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRecordCodePrefixRule(string Prefix) : ISkillProfileRecordRule
{
    public List<string> Check(SkillProfileRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRecordPinnedGuardRule(int Limit) : ISkillProfileRecordRule
{
    public List<string> Check(SkillProfileRecord item)
    {
        if (item.Pinned && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRecordNameLengthRule(int MaxLength) : ISkillProfileRecordRule
{
    public List<string> Check(SkillProfileRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileRecordAllowedLocationRule(List<string> Allowed) : ISkillProfileRecordRule
{
    public List<string> Check(SkillProfileRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile record.</summary>
public sealed record SkillProfileRecordValidator(List<ISkillProfileRecordRule> Rules)
{
    public List<string> Validate(SkillProfileRecord item)
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

    public void RequireValid(SkillProfileRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileRecord> items)
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

/// <summary>Functions of skill profile record rules.</summary>
public static class SkillProfileRecordRules
{
    public static SkillProfileRecordValidator DefaultSkillProfileRecordValidator()
    {
        return new SkillProfileRecordValidator(
            new List<ISkillProfileRecordRule>
            {
                new SkillProfileRecordLeaveDaysRangeRule(32, 120),
                new SkillProfileRecordCodePrefixRule("Q"),
                new SkillProfileRecordPinnedGuardRule(120),
                new SkillProfileRecordNameLengthRule(1000),
                new SkillProfileRecordAllowedLocationRule(new List<string>()),
            });
    }
}
