namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg audit must pass. Check returns its problems, or none.</summary>
public interface IReorgAuditRule
{
    List<string> Check(ReorgAudit item);
}

public sealed record ReorgAuditNameLengthRule(int MaxLength) : IReorgAuditRule
{
    public List<string> Check(ReorgAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAuditCostRangeRule(double Min, double Max) : IReorgAuditRule
{
    public List<string> Check(ReorgAudit item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReorgAuditAllowedNameRule(List<string> Allowed) : IReorgAuditRule
{
    public List<string> Check(ReorgAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAuditChecksumLengthRule(int MaxLength) : IReorgAuditRule
{
    public List<string> Check(ReorgAudit item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg audit.</summary>
public sealed record ReorgAuditValidator(List<IReorgAuditRule> Rules)
{
    public List<string> Validate(ReorgAudit item)
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

    public void RequireValid(ReorgAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgAudit> items)
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

/// <summary>Functions of reorg audit rules.</summary>
public static class ReorgAuditRules
{
    public static ReorgAuditValidator DefaultReorgAuditValidator()
    {
        return new ReorgAuditValidator(
            new List<IReorgAuditRule>
            {
                new ReorgAuditNameLengthRule(120),
                new ReorgAuditCostRangeRule(7.0, 1838.3),
                new ReorgAuditAllowedNameRule(new List<string> { "seasonal" }),
                new ReorgAuditChecksumLengthRule(1000),
            });
    }
}
