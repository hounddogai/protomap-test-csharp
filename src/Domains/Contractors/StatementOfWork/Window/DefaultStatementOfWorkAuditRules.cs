namespace Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>A rule a default statement of work audit must pass. Check returns its problems, or none.</summary>
public interface IDefaultStatementOfWorkAuditRule
{
    List<string> Check(DefaultStatementOfWorkAudit item);
}

public sealed record DefaultStatementOfWorkAuditRequiredReviewerRule(string Message) : IDefaultStatementOfWorkAuditRule
{
    public List<string> Check(DefaultStatementOfWorkAudit item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkAuditBatchSizeRangeRule(int Min, int Max) : IDefaultStatementOfWorkAuditRule
{
    public List<string> Check(DefaultStatementOfWorkAudit item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultStatementOfWorkAuditRequiredCodeRule(string Message) : IDefaultStatementOfWorkAuditRule
{
    public List<string> Check(DefaultStatementOfWorkAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkAuditCodePrefixRule(string Prefix) : IDefaultStatementOfWorkAuditRule
{
    public List<string> Check(DefaultStatementOfWorkAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultStatementOfWorkAuditRequiredNameRule(string Message) : IDefaultStatementOfWorkAuditRule
{
    public List<string> Check(DefaultStatementOfWorkAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default statement of work audit.</summary>
public sealed record DefaultStatementOfWorkAuditValidator(List<IDefaultStatementOfWorkAuditRule> Rules)
{
    public List<string> Validate(DefaultStatementOfWorkAudit item)
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

    public void RequireValid(DefaultStatementOfWorkAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultStatementOfWorkAudit> items)
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

/// <summary>Functions of default statement of work audit rules.</summary>
public static class DefaultStatementOfWorkAuditRules
{
    public static DefaultStatementOfWorkAuditValidator DefaultDefaultStatementOfWorkAuditValidator()
    {
        return new DefaultStatementOfWorkAuditValidator(
            new List<IDefaultStatementOfWorkAuditRule>
            {
                new DefaultStatementOfWorkAuditRequiredReviewerRule(
                    "default statement of work audit needs this field"),
                new DefaultStatementOfWorkAuditBatchSizeRangeRule(120, 1000),
                new DefaultStatementOfWorkAuditRequiredCodeRule("default statement of work audit needs a value"),
                new DefaultStatementOfWorkAuditCodePrefixRule("Q"),
                new DefaultStatementOfWorkAuditRequiredNameRule("default statement of work audit needs this field"),
            });
    }
}
