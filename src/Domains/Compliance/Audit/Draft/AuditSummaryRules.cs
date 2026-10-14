namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit summary must pass. Check returns its problems, or none.</summary>
public interface IAuditSummaryRule
{
    List<string> Check(AuditSummary item);
}

public sealed record AuditSummaryDiscountRangeRule(double Min, double Max) : IAuditSummaryRule
{
    public List<string> Check(AuditSummary item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditSummaryWindowDaysRangeRule(int Min, int Max) : IAuditSummaryRule
{
    public List<string> Check(AuditSummary item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AuditSummaryCodeLengthRule(int MaxLength) : IAuditSummaryRule
{
    public List<string> Check(AuditSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AuditSummaryAllowedLabelRule(List<string> Allowed) : IAuditSummaryRule
{
    public List<string> Check(AuditSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AuditSummaryRequiredLabelRule(string Message) : IAuditSummaryRule
{
    public List<string> Check(AuditSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit summary.</summary>
public sealed record AuditSummaryValidator(List<IAuditSummaryRule> Rules)
{
    public List<string> Validate(AuditSummary item)
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

    public void RequireValid(AuditSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditSummary> items)
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

/// <summary>Functions of audit summary rules.</summary>
public static class AuditSummaryRules
{
    public static AuditSummaryValidator DefaultAuditSummaryValidator()
    {
        return new AuditSummaryValidator(
            new List<IAuditSummaryRule>
            {
                new AuditSummaryDiscountRangeRule(4.4, 1835.3),
                new AuditSummaryWindowDaysRangeRule(16, 80),
                new AuditSummaryCodeLengthRule(1000),
                new AuditSummaryAllowedLabelRule(new List<string> { "pending", "rolling" }),
                new AuditSummaryRequiredLabelRule("audit summary needs an entry"),
            });
    }
}
