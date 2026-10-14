namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>A rule a rolling cost model audit must pass. Check returns its problems, or none.</summary>
public interface IRollingCostModelAuditRule
{
    List<string> Check(RollingCostModelAudit item);
}

public sealed record RollingCostModelAuditCapacityRangeRule(int Min, int Max) : IRollingCostModelAuditRule
{
    public List<string> Check(RollingCostModelAudit item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingCostModelAuditCodeLengthRule(int MaxLength) : IRollingCostModelAuditRule
{
    public List<string> Check(RollingCostModelAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelAuditUpdatedAtRangeRule(int Min, int Max) : IRollingCostModelAuditRule
{
    public List<string> Check(RollingCostModelAudit item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingCostModelAuditMaximumRangeRule(double Min, double Max) : IRollingCostModelAuditRule
{
    public List<string> Check(RollingCostModelAudit item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingCostModelAuditRequiredNameRule(string Message) : IRollingCostModelAuditRule
{
    public List<string> Check(RollingCostModelAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelAuditQuantityRangeRule(int Min, int Max) : IRollingCostModelAuditRule
{
    public List<string> Check(RollingCostModelAudit item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling cost model audit.</summary>
public sealed record RollingCostModelAuditValidator(List<IRollingCostModelAuditRule> Rules)
{
    public List<string> Validate(RollingCostModelAudit item)
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

    public void RequireValid(RollingCostModelAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingCostModelAudit> items)
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

/// <summary>Functions of rolling cost model audit rules.</summary>
public static class RollingCostModelAuditRules
{
    public static RollingCostModelAuditValidator DefaultRollingCostModelAuditValidator()
    {
        return new RollingCostModelAuditValidator(
            new List<IRollingCostModelAuditRule>
            {
                new RollingCostModelAuditCapacityRangeRule(16, 120),
                new RollingCostModelAuditCodeLengthRule(3600),
                new RollingCostModelAuditUpdatedAtRangeRule(64, 1000),
                new RollingCostModelAuditMaximumRangeRule(8.9, 1890.4),
                new RollingCostModelAuditRequiredNameRule("rolling cost model audit needs a value"),
                new RollingCostModelAuditQuantityRangeRule(64, 3600),
            });
    }
}
