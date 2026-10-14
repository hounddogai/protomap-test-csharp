namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>A rule a scheduled statement of work variance must pass. Check returns its problems, or none.</summary>
public interface IScheduledStatementOfWorkVarianceRule
{
    List<string> Check(ScheduledStatementOfWorkVariance item);
}

public sealed record ScheduledStatementOfWorkVarianceRequiredGradeRule(string Message) : IScheduledStatementOfWorkVarianceRule
{
    public List<string> Check(ScheduledStatementOfWorkVariance item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkVarianceCodePrefixRule(string Prefix) : IScheduledStatementOfWorkVarianceRule
{
    public List<string> Check(ScheduledStatementOfWorkVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledStatementOfWorkVarianceWeightRangeRule(double Min, double Max) : IScheduledStatementOfWorkVarianceRule
{
    public List<string> Check(ScheduledStatementOfWorkVariance item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled statement of work variance.</summary>
public sealed record ScheduledStatementOfWorkVarianceValidator(List<IScheduledStatementOfWorkVarianceRule> Rules)
{
    public List<string> Validate(ScheduledStatementOfWorkVariance item)
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

    public void RequireValid(ScheduledStatementOfWorkVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledStatementOfWorkVariance> items)
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

/// <summary>Functions of scheduled statement of work variance rules.</summary>
public static class ScheduledStatementOfWorkVarianceRules
{
    public static ScheduledStatementOfWorkVarianceValidator DefaultScheduledStatementOfWorkVarianceValidator()
    {
        return new ScheduledStatementOfWorkVarianceValidator(
            new List<IScheduledStatementOfWorkVarianceRule>
            {
                new ScheduledStatementOfWorkVarianceRequiredGradeRule(
                    "scheduled statement of work variance needs an entry"),
                new ScheduledStatementOfWorkVarianceCodePrefixRule("ACM"),
                new ScheduledStatementOfWorkVarianceWeightRangeRule(0.5, 4488.3),
            });
    }
}
