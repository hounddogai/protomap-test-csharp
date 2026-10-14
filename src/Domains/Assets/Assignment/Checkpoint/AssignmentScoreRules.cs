namespace Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>A rule a assignment score must pass. Check returns its problems, or none.</summary>
public interface IAssignmentScoreRule
{
    List<string> Check(AssignmentScore item);
}

public sealed record AssignmentScoreExternalRefLengthRule(int MaxLength) : IAssignmentScoreRule
{
    public List<string> Check(AssignmentScore item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentScoreCodePrefixRule(string Prefix) : IAssignmentScoreRule
{
    public List<string> Check(AssignmentScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentScoreCodeLengthRule(int MaxLength) : IAssignmentScoreRule
{
    public List<string> Check(AssignmentScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment score.</summary>
public sealed record AssignmentScoreValidator(List<IAssignmentScoreRule> Rules)
{
    public List<string> Validate(AssignmentScore item)
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

    public void RequireValid(AssignmentScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentScore> items)
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

/// <summary>Functions of assignment score rules.</summary>
public static class AssignmentScoreRules
{
    public static AssignmentScoreValidator DefaultAssignmentScoreValidator()
    {
        return new AssignmentScoreValidator(
            new List<IAssignmentScoreRule>
            {
                new AssignmentScoreExternalRefLengthRule(32),
                new AssignmentScoreCodePrefixRule("X"),
                new AssignmentScoreCodeLengthRule(3600),
            });
    }
}
