namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile summary must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileSummaryRule
{
    List<string> Check(SkillProfileSummary item);
}

public sealed record SkillProfileSummaryVersionRangeRule(int Min, int Max) : ISkillProfileSummaryRule
{
    public List<string> Check(SkillProfileSummary item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileSummaryNameLengthRule(int MaxLength) : ISkillProfileSummaryRule
{
    public List<string> Check(SkillProfileSummary item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSummaryCodePrefixRule(string Prefix) : ISkillProfileSummaryRule
{
    public List<string> Check(SkillProfileSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSummaryAllowedNameRule(List<string> Allowed) : ISkillProfileSummaryRule
{
    public List<string> Check(SkillProfileSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileSummaryRateRangeRule(double Min, double Max) : ISkillProfileSummaryRule
{
    public List<string> Check(SkillProfileSummary item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileSummaryRequiredSkillRule(string Message) : ISkillProfileSummaryRule
{
    public List<string> Check(SkillProfileSummary item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile summary.</summary>
public sealed record SkillProfileSummaryValidator(List<ISkillProfileSummaryRule> Rules)
{
    public List<string> Validate(SkillProfileSummary item)
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

    public void RequireValid(SkillProfileSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileSummary> items)
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

/// <summary>Functions of skill profile summary rules.</summary>
public static class SkillProfileSummaryRules
{
    public static SkillProfileSummaryValidator DefaultSkillProfileSummaryValidator()
    {
        return new SkillProfileSummaryValidator(
            new List<ISkillProfileSummaryRule>
            {
                new SkillProfileSummaryVersionRangeRule(32, 32),
                new SkillProfileSummaryNameLengthRule(64),
                new SkillProfileSummaryCodePrefixRule("OPS"),
                new SkillProfileSummaryAllowedNameRule(new List<string>()),
                new SkillProfileSummaryRateRangeRule(8.5, 3525.9),
                new SkillProfileSummaryRequiredSkillRule("skill profile summary needs an entry"),
            });
    }
}
