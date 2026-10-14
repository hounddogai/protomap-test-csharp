namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>A rule a batched scenario audit must pass. Check returns its problems, or none.</summary>
public interface IBatchedScenarioAuditRule
{
    List<string> Check(BatchedScenarioAudit item);
}

public sealed record BatchedScenarioAuditCodePrefixRule(string Prefix) : IBatchedScenarioAuditRule
{
    public List<string> Check(BatchedScenarioAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedScenarioAuditPinnedGuardRule(double Limit) : IBatchedScenarioAuditRule
{
    public List<string> Check(BatchedScenarioAudit item)
    {
        if (item.Pinned && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record BatchedScenarioAuditCodeLengthRule(int MaxLength) : IBatchedScenarioAuditRule
{
    public List<string> Check(BatchedScenarioAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedScenarioAuditLabelLengthRule(int MaxLength) : IBatchedScenarioAuditRule
{
    public List<string> Check(BatchedScenarioAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched scenario audit.</summary>
public sealed record BatchedScenarioAuditValidator(List<IBatchedScenarioAuditRule> Rules)
{
    public List<string> Validate(BatchedScenarioAudit item)
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

    public void RequireValid(BatchedScenarioAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedScenarioAudit> items)
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

/// <summary>Functions of batched scenario audit rules.</summary>
public static class BatchedScenarioAuditRules
{
    public static BatchedScenarioAuditValidator DefaultBatchedScenarioAuditValidator()
    {
        return new BatchedScenarioAuditValidator(
            new List<IBatchedScenarioAuditRule>
            {
                new BatchedScenarioAuditCodePrefixRule("OPS"),
                new BatchedScenarioAuditPinnedGuardRule(6.6),
                new BatchedScenarioAuditCodeLengthRule(80),
                new BatchedScenarioAuditLabelLengthRule(3600),
            });
    }
}
