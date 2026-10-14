namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>A rule a statement of work draft must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkDraftRule
{
    List<string> Check(StatementOfWorkDraft item);
}

public sealed record StatementOfWorkDraftApprovedGuardRule(int Limit) : IStatementOfWorkDraftRule
{
    public List<string> Check(StatementOfWorkDraft item)
    {
        if (item.Approved && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkDraftScoreRangeRule(double Min, double Max) : IStatementOfWorkDraftRule
{
    public List<string> Check(StatementOfWorkDraft item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkDraftGraceDaysRangeRule(int Min, int Max) : IStatementOfWorkDraftRule
{
    public List<string> Check(StatementOfWorkDraft item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work draft.</summary>
public sealed record StatementOfWorkDraftValidator(List<IStatementOfWorkDraftRule> Rules)
{
    public List<string> Validate(StatementOfWorkDraft item)
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

    public void RequireValid(StatementOfWorkDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkDraft> items)
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

/// <summary>Functions of statement of work draft rules.</summary>
public static class StatementOfWorkDraftRules
{
    public static StatementOfWorkDraftValidator DefaultStatementOfWorkDraftValidator()
    {
        return new StatementOfWorkDraftValidator(
            new List<IStatementOfWorkDraftRule>
            {
                new StatementOfWorkDraftApprovedGuardRule(64),
                new StatementOfWorkDraftScoreRangeRule(3.6, 1772.5),
                new StatementOfWorkDraftGraceDaysRangeRule(32, 32),
            });
    }
}
