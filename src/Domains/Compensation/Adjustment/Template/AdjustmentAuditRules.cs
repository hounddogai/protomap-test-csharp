namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment audit must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentAuditRule
{
    List<string> Check(AdjustmentAudit item);
}

public sealed record AdjustmentAuditQuantityRangeRule(int Min, int Max) : IAdjustmentAuditRule
{
    public List<string> Check(AdjustmentAudit item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentAuditRequiredExternalRefRule(string Message) : IAdjustmentAuditRule
{
    public List<string> Check(AdjustmentAudit item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAuditAllowedExternalRefRule(List<string> Allowed) : IAdjustmentAuditRule
{
    public List<string> Check(AdjustmentAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment audit.</summary>
public sealed record AdjustmentAuditValidator(List<IAdjustmentAuditRule> Rules)
{
    public List<string> Validate(AdjustmentAudit item)
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

    public void RequireValid(AdjustmentAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentAudit> items)
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

/// <summary>Functions of adjustment audit rules.</summary>
public static class AdjustmentAuditRules
{
    public static AdjustmentAuditValidator DefaultAdjustmentAuditValidator()
    {
        return new AdjustmentAuditValidator(
            new List<IAdjustmentAuditRule>
            {
                new AdjustmentAuditQuantityRangeRule(80, 120),
                new AdjustmentAuditRequiredExternalRefRule("adjustment audit needs this field"),
                new AdjustmentAuditAllowedExternalRefRule(new List<string> { "scheduled" }),
            });
    }
}
