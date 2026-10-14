namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>A rule a legacy visitor checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILegacyVisitorCheckpointRule
{
    List<string> Check(LegacyVisitorCheckpoint item);
}

public sealed record LegacyVisitorCheckpointDepartmentLengthRule(int MaxLength) : ILegacyVisitorCheckpointRule
{
    public List<string> Check(LegacyVisitorCheckpoint item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVisitorCheckpointRequiredTitleRule(string Message) : ILegacyVisitorCheckpointRule
{
    public List<string> Check(LegacyVisitorCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyVisitorCheckpointTitleLengthRule(int MaxLength) : ILegacyVisitorCheckpointRule
{
    public List<string> Check(LegacyVisitorCheckpoint item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVisitorCheckpointCodePrefixRule(string Prefix) : ILegacyVisitorCheckpointRule
{
    public List<string> Check(LegacyVisitorCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy visitor checkpoint.</summary>
public sealed record LegacyVisitorCheckpointValidator(List<ILegacyVisitorCheckpointRule> Rules)
{
    public List<string> Validate(LegacyVisitorCheckpoint item)
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

    public void RequireValid(LegacyVisitorCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyVisitorCheckpoint> items)
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

/// <summary>Functions of legacy visitor checkpoint rules.</summary>
public static class LegacyVisitorCheckpointRules
{
    public static LegacyVisitorCheckpointValidator DefaultLegacyVisitorCheckpointValidator()
    {
        return new LegacyVisitorCheckpointValidator(
            new List<ILegacyVisitorCheckpointRule>
            {
                new LegacyVisitorCheckpointDepartmentLengthRule(1000),
                new LegacyVisitorCheckpointRequiredTitleRule("legacy visitor checkpoint needs a value"),
                new LegacyVisitorCheckpointTitleLengthRule(120),
                new LegacyVisitorCheckpointCodePrefixRule("X"),
            });
    }
}
