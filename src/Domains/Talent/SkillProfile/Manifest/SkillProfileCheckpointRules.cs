namespace Acme.Workforce.Talent.SkillProfile.Manifest;

/// <summary>A rule a skill profile checkpoint must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileCheckpointRule
{
    List<string> Check(SkillProfileCheckpoint item);
}

public sealed record SkillProfileCheckpointCostCenterLengthRule(int MaxLength) : ISkillProfileCheckpointRule
{
    public List<string> Check(SkillProfileCheckpoint item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileCheckpointRequiredCodeRule(string Message) : ISkillProfileCheckpointRule
{
    public List<string> Check(SkillProfileCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileCheckpointRequiredTitleRule(string Message) : ISkillProfileCheckpointRule
{
    public List<string> Check(SkillProfileCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileCheckpointCodePrefixRule(string Prefix) : ISkillProfileCheckpointRule
{
    public List<string> Check(SkillProfileCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileCheckpointTitleLengthRule(int MaxLength) : ISkillProfileCheckpointRule
{
    public List<string> Check(SkillProfileCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile checkpoint.</summary>
public sealed record SkillProfileCheckpointValidator(List<ISkillProfileCheckpointRule> Rules)
{
    public List<string> Validate(SkillProfileCheckpoint item)
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

    public void RequireValid(SkillProfileCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileCheckpoint> items)
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

/// <summary>Functions of skill profile checkpoint rules.</summary>
public static class SkillProfileCheckpointRules
{
    public static SkillProfileCheckpointValidator DefaultSkillProfileCheckpointValidator()
    {
        return new SkillProfileCheckpointValidator(
            new List<ISkillProfileCheckpointRule>
            {
                new SkillProfileCheckpointCostCenterLengthRule(64),
                new SkillProfileCheckpointRequiredCodeRule("skill profile checkpoint needs a value"),
                new SkillProfileCheckpointRequiredTitleRule("skill profile checkpoint needs this field"),
                new SkillProfileCheckpointCodePrefixRule("REF"),
                new SkillProfileCheckpointTitleLengthRule(1000),
            });
    }
}
