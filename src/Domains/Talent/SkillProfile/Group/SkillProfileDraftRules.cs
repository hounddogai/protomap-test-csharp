namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a skill profile draft must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileDraftRule
{
    List<string> Check(SkillProfileDraft item);
}

public sealed record SkillProfileDraftSequenceRangeRule(int Min, int Max) : ISkillProfileDraftRule
{
    public List<string> Check(SkillProfileDraft item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileDraftSourceLengthRule(int MaxLength) : ISkillProfileDraftRule
{
    public List<string> Check(SkillProfileDraft item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileDraftAllowedSourceRule(List<string> Allowed) : ISkillProfileDraftRule
{
    public List<string> Check(SkillProfileDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileDraftVisibleGuardRule(int Limit) : ISkillProfileDraftRule
{
    public List<string> Check(SkillProfileDraft item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileDraftVersionRangeRule(int Min, int Max) : ISkillProfileDraftRule
{
    public List<string> Check(SkillProfileDraft item)
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

public sealed record SkillProfileDraftRetriesRangeRule(int Min, int Max) : ISkillProfileDraftRule
{
    public List<string> Check(SkillProfileDraft item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a skill profile draft.</summary>
public sealed record SkillProfileDraftValidator(List<ISkillProfileDraftRule> Rules)
{
    public List<string> Validate(SkillProfileDraft item)
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

    public void RequireValid(SkillProfileDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileDraft> items)
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

/// <summary>Functions of skill profile draft rules.</summary>
public static class SkillProfileDraftRules
{
    public static SkillProfileDraftValidator DefaultSkillProfileDraftValidator()
    {
        return new SkillProfileDraftValidator(
            new List<ISkillProfileDraftRule>
            {
                new SkillProfileDraftSequenceRangeRule(64, 3600),
                new SkillProfileDraftSourceLengthRule(1000),
                new SkillProfileDraftAllowedSourceRule(new List<string> { "batched", "primary" }),
                new SkillProfileDraftVisibleGuardRule(120),
                new SkillProfileDraftVersionRangeRule(80, 120),
                new SkillProfileDraftRetriesRangeRule(255, 1000),
            });
    }
}
