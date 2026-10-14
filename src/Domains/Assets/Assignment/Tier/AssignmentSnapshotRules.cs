namespace Acme.Workforce.Assets.Assignment.Tier;

/// <summary>A rule a assignment snapshot must pass. Check returns its problems, or none.</summary>
public interface IAssignmentSnapshotRule
{
    List<string> Check(AssignmentSnapshot item);
}

public sealed record AssignmentSnapshotRequiredSkillRule(string Message) : IAssignmentSnapshotRule
{
    public List<string> Check(AssignmentSnapshot item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentSnapshotCodePrefixRule(string Prefix) : IAssignmentSnapshotRule
{
    public List<string> Check(AssignmentSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentSnapshotNameLengthRule(int MaxLength) : IAssignmentSnapshotRule
{
    public List<string> Check(AssignmentSnapshot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentSnapshotRequiredNameRule(string Message) : IAssignmentSnapshotRule
{
    public List<string> Check(AssignmentSnapshot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment snapshot.</summary>
public sealed record AssignmentSnapshotValidator(List<IAssignmentSnapshotRule> Rules)
{
    public List<string> Validate(AssignmentSnapshot item)
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

    public void RequireValid(AssignmentSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentSnapshot> items)
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

/// <summary>Functions of assignment snapshot rules.</summary>
public static class AssignmentSnapshotRules
{
    public static AssignmentSnapshotValidator DefaultAssignmentSnapshotValidator()
    {
        return new AssignmentSnapshotValidator(
            new List<IAssignmentSnapshotRule>
            {
                new AssignmentSnapshotRequiredSkillRule("assignment snapshot needs this field"),
                new AssignmentSnapshotCodePrefixRule("X"),
                new AssignmentSnapshotNameLengthRule(255),
                new AssignmentSnapshotRequiredNameRule("assignment snapshot needs an entry"),
            });
    }
}
