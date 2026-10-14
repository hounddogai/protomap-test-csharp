namespace Acme.Workforce.Assets.Warranty.Metric;

/// <summary>A rule a warranty audit must pass. Check returns its problems, or none.</summary>
public interface IWarrantyAuditRule
{
    List<string> Check(WarrantyAudit item);
}

public sealed record WarrantyAuditUnitLengthRule(int MaxLength) : IWarrantyAuditRule
{
    public List<string> Check(WarrantyAudit item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAuditSummaryLengthRule(int MaxLength) : IWarrantyAuditRule
{
    public List<string> Check(WarrantyAudit item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAuditHoursRangeRule(double Min, double Max) : IWarrantyAuditRule
{
    public List<string> Check(WarrantyAudit item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a warranty audit.</summary>
public sealed record WarrantyAuditValidator(List<IWarrantyAuditRule> Rules)
{
    public List<string> Validate(WarrantyAudit item)
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

    public void RequireValid(WarrantyAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyAudit> items)
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

/// <summary>Functions of warranty audit rules.</summary>
public static class WarrantyAuditRules
{
    public static WarrantyAuditValidator DefaultWarrantyAuditValidator()
    {
        return new WarrantyAuditValidator(
            new List<IWarrantyAuditRule>
            {
                new WarrantyAuditUnitLengthRule(120),
                new WarrantyAuditSummaryLengthRule(32),
                new WarrantyAuditHoursRangeRule(9.5, 739.2),
            });
    }
}
