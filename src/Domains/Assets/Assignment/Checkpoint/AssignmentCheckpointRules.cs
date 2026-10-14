namespace Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>A rule a assignment checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAssignmentCheckpointRule
{
    List<string> Check(AssignmentCheckpoint item);
}

public sealed record AssignmentCheckpointRequiredLabelRule(string Message) : IAssignmentCheckpointRule
{
    public List<string> Check(AssignmentCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentCheckpointCodePrefixRule(string Prefix) : IAssignmentCheckpointRule
{
    public List<string> Check(AssignmentCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentCheckpointStartsAtRangeRule(int Min, int Max) : IAssignmentCheckpointRule
{
    public List<string> Check(AssignmentCheckpoint item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentCheckpointRequiredCodeRule(string Message) : IAssignmentCheckpointRule
{
    public List<string> Check(AssignmentCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentCheckpointSkillLengthRule(int MaxLength) : IAssignmentCheckpointRule
{
    public List<string> Check(AssignmentCheckpoint item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment checkpoint.</summary>
public sealed record AssignmentCheckpointValidator(List<IAssignmentCheckpointRule> Rules)
{
    public List<string> Validate(AssignmentCheckpoint item)
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

    public void RequireValid(AssignmentCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentCheckpoint> items)
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

/// <summary>Functions of assignment checkpoint rules.</summary>
public static class AssignmentCheckpointRules
{
    public static AssignmentCheckpointValidator DefaultAssignmentCheckpointValidator()
    {
        return new AssignmentCheckpointValidator(
            new List<IAssignmentCheckpointRule>
            {
                new AssignmentCheckpointRequiredLabelRule("assignment checkpoint needs an entry"),
                new AssignmentCheckpointCodePrefixRule("ACM"),
                new AssignmentCheckpointStartsAtRangeRule(64, 120),
                new AssignmentCheckpointRequiredCodeRule("assignment checkpoint needs a value"),
                new AssignmentCheckpointSkillLengthRule(86400),
            });
    }
}
