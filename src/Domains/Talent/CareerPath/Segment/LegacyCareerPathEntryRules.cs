namespace Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>A rule a legacy career path entry must pass. Check returns its problems, or none.</summary>
public interface ILegacyCareerPathEntryRule
{
    List<string> Check(LegacyCareerPathEntry item);
}

public sealed record LegacyCareerPathEntryCodePrefixRule(string Prefix) : ILegacyCareerPathEntryRule
{
    public List<string> Check(LegacyCareerPathEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathEntryFteRangeRule(double Min, double Max) : ILegacyCareerPathEntryRule
{
    public List<string> Check(LegacyCareerPathEntry item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyCareerPathEntryRequiredCodeRule(string Message) : ILegacyCareerPathEntryRule
{
    public List<string> Check(LegacyCareerPathEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathEntrySkillLengthRule(int MaxLength) : ILegacyCareerPathEntryRule
{
    public List<string> Check(LegacyCareerPathEntry item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy career path entry.</summary>
public sealed record LegacyCareerPathEntryValidator(List<ILegacyCareerPathEntryRule> Rules)
{
    public List<string> Validate(LegacyCareerPathEntry item)
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

    public void RequireValid(LegacyCareerPathEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyCareerPathEntry> items)
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

/// <summary>Functions of legacy career path entry rules.</summary>
public static class LegacyCareerPathEntryRules
{
    public static LegacyCareerPathEntryValidator DefaultLegacyCareerPathEntryValidator()
    {
        return new LegacyCareerPathEntryValidator(
            new List<ILegacyCareerPathEntryRule>
            {
                new LegacyCareerPathEntryCodePrefixRule("Q"),
                new LegacyCareerPathEntryFteRangeRule(1.1, 3784.4),
                new LegacyCareerPathEntryRequiredCodeRule("legacy career path entry needs a value"),
                new LegacyCareerPathEntrySkillLengthRule(120),
            });
    }
}
