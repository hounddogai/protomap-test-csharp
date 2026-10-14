namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global audit summary must pass. Check returns its problems, or none.</summary>
public interface IGlobalAuditSummaryRule
{
    List<string> Check(GlobalAuditSummary item);
}

public sealed record GlobalAuditSummaryRequiredLabelRule(string Message) : IGlobalAuditSummaryRule
{
    public List<string> Check(GlobalAuditSummary item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditSummaryCodeLengthRule(int MaxLength) : IGlobalAuditSummaryRule
{
    public List<string> Check(GlobalAuditSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditSummaryLimitRangeRule(int Min, int Max) : IGlobalAuditSummaryRule
{
    public List<string> Check(GlobalAuditSummary item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalAuditSummaryJobCodeLengthRule(int MaxLength) : IGlobalAuditSummaryRule
{
    public List<string> Check(GlobalAuditSummary item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditSummaryAllowedJobCodeRule(List<string> Allowed) : IGlobalAuditSummaryRule
{
    public List<string> Check(GlobalAuditSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAuditSummaryMinimumRangeRule(double Min, double Max) : IGlobalAuditSummaryRule
{
    public List<string> Check(GlobalAuditSummary item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global audit summary.</summary>
public sealed record GlobalAuditSummaryValidator(List<IGlobalAuditSummaryRule> Rules)
{
    public List<string> Validate(GlobalAuditSummary item)
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

    public void RequireValid(GlobalAuditSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAuditSummary> items)
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

/// <summary>Functions of global audit summary rules.</summary>
public static class GlobalAuditSummaryRules
{
    public static GlobalAuditSummaryValidator DefaultGlobalAuditSummaryValidator()
    {
        return new GlobalAuditSummaryValidator(
            new List<IGlobalAuditSummaryRule>
            {
                new GlobalAuditSummaryRequiredLabelRule("global audit summary needs a value"),
                new GlobalAuditSummaryCodeLengthRule(1000),
                new GlobalAuditSummaryLimitRangeRule(86400, 86400),
                new GlobalAuditSummaryJobCodeLengthRule(32),
                new GlobalAuditSummaryAllowedJobCodeRule(new List<string> { "weighted" }),
                new GlobalAuditSummaryMinimumRangeRule(1.8, 1787.6),
            });
    }
}
