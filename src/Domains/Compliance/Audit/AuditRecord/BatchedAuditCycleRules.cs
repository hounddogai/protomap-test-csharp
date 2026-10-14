namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched audit cycle must pass. Check returns its problems, or none.</summary>
public interface IBatchedAuditCycleRule
{
    List<string> Check(BatchedAuditCycle item);
}

public sealed record BatchedAuditCycleRequiredEmployeeIdRule(string Message) : IBatchedAuditCycleRule
{
    public List<string> Check(BatchedAuditCycle item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAuditCycleRequiredNameRule(string Message) : IBatchedAuditCycleRule
{
    public List<string> Check(BatchedAuditCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAuditCycleAllowedEmployeeIdRule(List<string> Allowed) : IBatchedAuditCycleRule
{
    public List<string> Check(BatchedAuditCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAuditCycleRequiredCountryRule(string Message) : IBatchedAuditCycleRule
{
    public List<string> Check(BatchedAuditCycle item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedAuditCycleCodePrefixRule(string Prefix) : IBatchedAuditCycleRule
{
    public List<string> Check(BatchedAuditCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedAuditCycleGraceDaysRangeRule(int Min, int Max) : IBatchedAuditCycleRule
{
    public List<string> Check(BatchedAuditCycle item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched audit cycle.</summary>
public sealed record BatchedAuditCycleValidator(List<IBatchedAuditCycleRule> Rules)
{
    public List<string> Validate(BatchedAuditCycle item)
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

    public void RequireValid(BatchedAuditCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedAuditCycle> items)
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

/// <summary>Functions of batched audit cycle rules.</summary>
public static class BatchedAuditCycleRules
{
    public static BatchedAuditCycleValidator DefaultBatchedAuditCycleValidator()
    {
        return new BatchedAuditCycleValidator(
            new List<IBatchedAuditCycleRule>
            {
                new BatchedAuditCycleRequiredEmployeeIdRule("batched audit cycle needs an entry"),
                new BatchedAuditCycleRequiredNameRule("batched audit cycle needs this field"),
                new BatchedAuditCycleAllowedEmployeeIdRule(new List<string> { "regional", "strict" }),
                new BatchedAuditCycleRequiredCountryRule("batched audit cycle needs an entry"),
                new BatchedAuditCycleCodePrefixRule("ACM"),
                new BatchedAuditCycleGraceDaysRangeRule(16, 86400),
            });
    }
}
