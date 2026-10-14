namespace Acme.Workforce.Talent.Pool.Audit;

/// <summary>A rule a pool audit must pass. Check returns its problems, or none.</summary>
public interface IPoolAuditRule
{
    List<string> Check(PoolAudit item);
}

public sealed record PoolAuditSegmentLengthRule(int MaxLength) : IPoolAuditRule
{
    public List<string> Check(PoolAudit item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolAuditEmployeeIdLengthRule(int MaxLength) : IPoolAuditRule
{
    public List<string> Check(PoolAudit item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PoolAuditCostRangeRule(double Min, double Max) : IPoolAuditRule
{
    public List<string> Check(PoolAudit item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolAuditRequiredEmployeeIdRule(string Message) : IPoolAuditRule
{
    public List<string> Check(PoolAudit item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PoolAuditRetriesRangeRule(int Min, int Max) : IPoolAuditRule
{
    public List<string> Check(PoolAudit item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PoolAuditCodeLengthRule(int MaxLength) : IPoolAuditRule
{
    public List<string> Check(PoolAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pool audit.</summary>
public sealed record PoolAuditValidator(List<IPoolAuditRule> Rules)
{
    public List<string> Validate(PoolAudit item)
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

    public void RequireValid(PoolAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PoolAudit> items)
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

/// <summary>Functions of pool audit rules.</summary>
public static class PoolAuditRules
{
    public static PoolAuditValidator DefaultPoolAuditValidator()
    {
        return new PoolAuditValidator(
            new List<IPoolAuditRule>
            {
                new PoolAuditSegmentLengthRule(64),
                new PoolAuditEmployeeIdLengthRule(64),
                new PoolAuditCostRangeRule(0.1, 4724.4),
                new PoolAuditRequiredEmployeeIdRule("pool audit needs a value"),
                new PoolAuditRetriesRangeRule(1000, 1000),
                new PoolAuditCodeLengthRule(255),
            });
    }
}
