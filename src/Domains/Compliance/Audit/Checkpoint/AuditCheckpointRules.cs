namespace Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>A rule a audit checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAuditCheckpointRule
{
    List<string> Check(AuditCheckpoint item);
}

public sealed record AuditCheckpointRegionLengthRule(int MaxLength) : IAuditCheckpointRule
{
    public List<string> Check(AuditCheckpoint item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditCheckpointRequiredNameRule(string Message) : IAuditCheckpointRule
{
    public List<string> Check(AuditCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditCheckpointLeaveDaysRangeRule(int Min, int Max) : IAuditCheckpointRule
{
    public List<string> Check(AuditCheckpoint item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditCheckpointRequiredRegionRule(string Message) : IAuditCheckpointRule
{
    public List<string> Check(AuditCheckpoint item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditCheckpointRequiredCodeRule(string Message) : IAuditCheckpointRule
{
    public List<string> Check(AuditCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditCheckpointNameLengthRule(int MaxLength) : IAuditCheckpointRule
{
    public List<string> Check(AuditCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit checkpoint.</summary>
public sealed record AuditCheckpointValidator(List<IAuditCheckpointRule> Rules)
{
    public List<string> Validate(AuditCheckpoint item)
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

    public void RequireValid(AuditCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditCheckpoint> items)
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

/// <summary>Functions of audit checkpoint rules.</summary>
public static class AuditCheckpointRules
{
    public static AuditCheckpointValidator DefaultAuditCheckpointValidator()
    {
        return new AuditCheckpointValidator(
            new List<IAuditCheckpointRule>
            {
                new AuditCheckpointRegionLengthRule(1000),
                new AuditCheckpointRequiredNameRule("audit checkpoint needs a value"),
                new AuditCheckpointLeaveDaysRangeRule(255, 3600),
                new AuditCheckpointRequiredRegionRule("audit checkpoint needs a value"),
                new AuditCheckpointRequiredCodeRule("audit checkpoint needs a value"),
                new AuditCheckpointNameLengthRule(255),
            });
    }
}
