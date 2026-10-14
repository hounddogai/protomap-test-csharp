namespace Acme.Workforce.Talent.Pool.Plan;

/// <summary>A rule a capped pool audit must pass. Check returns its problems, or none.</summary>
public interface ICappedPoolAuditRule
{
    List<string> Check(CappedPoolAudit item);
}

public sealed record CappedPoolAuditRequiredReviewerRule(string Message) : ICappedPoolAuditRule
{
    public List<string> Check(CappedPoolAudit item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPoolAuditLabelLengthRule(int MaxLength) : ICappedPoolAuditRule
{
    public List<string> Check(CappedPoolAudit item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPoolAuditStartsAtRangeRule(int Min, int Max) : ICappedPoolAuditRule
{
    public List<string> Check(CappedPoolAudit item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPoolAuditRequiredCodeRule(string Message) : ICappedPoolAuditRule
{
    public List<string> Check(CappedPoolAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPoolAuditBudgetRangeRule(double Min, double Max) : ICappedPoolAuditRule
{
    public List<string> Check(CappedPoolAudit item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pool audit.</summary>
public sealed record CappedPoolAuditValidator(List<ICappedPoolAuditRule> Rules)
{
    public List<string> Validate(CappedPoolAudit item)
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

    public void RequireValid(CappedPoolAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPoolAudit> items)
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

/// <summary>Functions of capped pool audit rules.</summary>
public static class CappedPoolAuditRules
{
    public static CappedPoolAuditValidator DefaultCappedPoolAuditValidator()
    {
        return new CappedPoolAuditValidator(
            new List<ICappedPoolAuditRule>
            {
                new CappedPoolAuditRequiredReviewerRule("capped pool audit needs a value"),
                new CappedPoolAuditLabelLengthRule(3600),
                new CappedPoolAuditStartsAtRangeRule(3600, 86400),
                new CappedPoolAuditRequiredCodeRule("capped pool audit needs this field"),
                new CappedPoolAuditBudgetRangeRule(1.2, 687.6),
            });
    }
}
