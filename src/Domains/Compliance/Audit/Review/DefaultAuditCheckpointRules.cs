namespace Acme.Workforce.Compliance.Audit.Review;

/// <summary>A rule a default audit checkpoint must pass. Check returns its problems, or none.</summary>
public interface IDefaultAuditCheckpointRule
{
    List<string> Check(DefaultAuditCheckpoint item);
}

public sealed record DefaultAuditCheckpointCodePrefixRule(string Prefix) : IDefaultAuditCheckpointRule
{
    public List<string> Check(DefaultAuditCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAuditCheckpointNameLengthRule(int MaxLength) : IDefaultAuditCheckpointRule
{
    public List<string> Check(DefaultAuditCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAuditCheckpointRequiredCodeRule(string Message) : IDefaultAuditCheckpointRule
{
    public List<string> Check(DefaultAuditCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAuditCheckpointRequiredEmployeeIdRule(string Message) : IDefaultAuditCheckpointRule
{
    public List<string> Check(DefaultAuditCheckpoint item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default audit checkpoint.</summary>
public sealed record DefaultAuditCheckpointValidator(List<IDefaultAuditCheckpointRule> Rules)
{
    public List<string> Validate(DefaultAuditCheckpoint item)
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

    public void RequireValid(DefaultAuditCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAuditCheckpoint> items)
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

/// <summary>Functions of default audit checkpoint rules.</summary>
public static class DefaultAuditCheckpointRules
{
    public static DefaultAuditCheckpointValidator DefaultDefaultAuditCheckpointValidator()
    {
        return new DefaultAuditCheckpointValidator(
            new List<IDefaultAuditCheckpointRule>
            {
                new DefaultAuditCheckpointCodePrefixRule("ACM"),
                new DefaultAuditCheckpointNameLengthRule(16),
                new DefaultAuditCheckpointRequiredCodeRule("default audit checkpoint needs this field"),
                new DefaultAuditCheckpointRequiredEmployeeIdRule("default audit checkpoint needs a value"),
            });
    }
}
