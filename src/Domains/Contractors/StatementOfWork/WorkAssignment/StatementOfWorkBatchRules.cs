namespace Acme.Workforce.Contractors.StatementOfWork.WorkAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work batch must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkBatchRule
{
    List<string> Check(StatementOfWorkBatch item);
}

public sealed record StatementOfWorkBatchCodePrefixRule(string Prefix) : IStatementOfWorkBatchRule
{
    public List<string> Check(StatementOfWorkBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBatchApprovedGuardRule(int Limit) : IStatementOfWorkBatchRule
{
    public List<string> Check(StatementOfWorkBatch item)
    {
        if (item.Approved && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkBatchAllowedLabelRule(List<string> Allowed) : IStatementOfWorkBatchRule
{
    public List<string> Check(StatementOfWorkBatch item)
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

public sealed record StatementOfWorkBatchPriorityRangeRule(int Min, int Max) : IStatementOfWorkBatchRule
{
    public List<string> Check(StatementOfWorkBatch item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkBatchRequiredExternalRefRule(string Message) : IStatementOfWorkBatchRule
{
    public List<string> Check(StatementOfWorkBatch item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work batch.</summary>
public sealed record StatementOfWorkBatchValidator(List<IStatementOfWorkBatchRule> Rules)
{
    public List<string> Validate(StatementOfWorkBatch item)
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

    public void RequireValid(StatementOfWorkBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkBatch> items)
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

/// <summary>Functions of statement of work batch rules.</summary>
public static class StatementOfWorkBatchRules
{
    public static StatementOfWorkBatchValidator DefaultStatementOfWorkBatchValidator()
    {
        return new StatementOfWorkBatchValidator(
            new List<IStatementOfWorkBatchRule>
            {
                new StatementOfWorkBatchCodePrefixRule("REF"),
                new StatementOfWorkBatchApprovedGuardRule(80),
                new StatementOfWorkBatchAllowedLabelRule(new List<string>()),
                new StatementOfWorkBatchPriorityRangeRule(80, 86400),
                new StatementOfWorkBatchRequiredExternalRefRule("statement of work batch needs this field"),
            });
    }
}
