namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>A rule a rolling statement of work variance must pass. Check returns its problems, or none.</summary>
public interface IRollingStatementOfWorkVarianceRule
{
    List<string> Check(RollingStatementOfWorkVariance item);
}

public sealed record RollingStatementOfWorkVarianceRequiredTitleRule(string Message) : IRollingStatementOfWorkVarianceRule
{
    public List<string> Check(RollingStatementOfWorkVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingStatementOfWorkVarianceCodeLengthRule(int MaxLength) : IRollingStatementOfWorkVarianceRule
{
    public List<string> Check(RollingStatementOfWorkVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingStatementOfWorkVariancePinnedGuardRule(int Limit) : IRollingStatementOfWorkVarianceRule
{
    public List<string> Check(RollingStatementOfWorkVariance item)
    {
        if (item.Pinned && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RollingStatementOfWorkVarianceRequiredGradeRule(string Message) : IRollingStatementOfWorkVarianceRule
{
    public List<string> Check(RollingStatementOfWorkVariance item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling statement of work variance.</summary>
public sealed record RollingStatementOfWorkVarianceValidator(List<IRollingStatementOfWorkVarianceRule> Rules)
{
    public List<string> Validate(RollingStatementOfWorkVariance item)
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

    public void RequireValid(RollingStatementOfWorkVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingStatementOfWorkVariance> items)
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

/// <summary>Functions of rolling statement of work variance rules.</summary>
public static class RollingStatementOfWorkVarianceRules
{
    public static RollingStatementOfWorkVarianceValidator DefaultRollingStatementOfWorkVarianceValidator()
    {
        return new RollingStatementOfWorkVarianceValidator(
            new List<IRollingStatementOfWorkVarianceRule>
            {
                new RollingStatementOfWorkVarianceRequiredTitleRule(
                    "rolling statement of work variance needs this field"),
                new RollingStatementOfWorkVarianceCodeLengthRule(80),
                new RollingStatementOfWorkVariancePinnedGuardRule(64),
                new RollingStatementOfWorkVarianceRequiredGradeRule(
                    "rolling statement of work variance needs this field"),
            });
    }
}
