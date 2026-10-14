namespace Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>A rule a assignment override must pass. Check returns its problems, or none.</summary>
public interface IAssignmentOverrideRule
{
    List<string> Check(AssignmentOverride item);
}

public sealed record AssignmentOverrideLabelLengthRule(int MaxLength) : IAssignmentOverrideRule
{
    public List<string> Check(AssignmentOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentOverridePinnedGuardRule(int Limit) : IAssignmentOverrideRule
{
    public List<string> Check(AssignmentOverride item)
    {
        if (item.Pinned && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentOverrideCodePrefixRule(string Prefix) : IAssignmentOverrideRule
{
    public List<string> Check(AssignmentOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentOverrideRequiredTeamRule(string Message) : IAssignmentOverrideRule
{
    public List<string> Check(AssignmentOverride item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment override.</summary>
public sealed record AssignmentOverrideValidator(List<IAssignmentOverrideRule> Rules)
{
    public List<string> Validate(AssignmentOverride item)
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

    public void RequireValid(AssignmentOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentOverride> items)
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

/// <summary>Functions of assignment override rules.</summary>
public static class AssignmentOverrideRules
{
    public static AssignmentOverrideValidator DefaultAssignmentOverrideValidator()
    {
        return new AssignmentOverrideValidator(
            new List<IAssignmentOverrideRule>
            {
                new AssignmentOverrideLabelLengthRule(80),
                new AssignmentOverridePinnedGuardRule(1000),
                new AssignmentOverrideCodePrefixRule("Q"),
                new AssignmentOverrideRequiredTeamRule("assignment override needs this field"),
            });
    }
}
