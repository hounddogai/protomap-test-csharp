namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work ledger must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkLedgerRule
{
    List<string> Check(StatementOfWorkLedger item);
}

public sealed record StatementOfWorkLedgerRequiredNameRule(string Message) : IStatementOfWorkLedgerRule
{
    public List<string> Check(StatementOfWorkLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkLedgerRequiredCodeRule(string Message) : IStatementOfWorkLedgerRule
{
    public List<string> Check(StatementOfWorkLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkLedgerRateRangeRule(double Min, double Max) : IStatementOfWorkLedgerRule
{
    public List<string> Check(StatementOfWorkLedger item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkLedgerAllowedManagerIdRule(List<string> Allowed) : IStatementOfWorkLedgerRule
{
    public List<string> Check(StatementOfWorkLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkLedgerCodeLengthRule(int MaxLength) : IStatementOfWorkLedgerRule
{
    public List<string> Check(StatementOfWorkLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkLedgerRequiredSummaryRule(string Message) : IStatementOfWorkLedgerRule
{
    public List<string> Check(StatementOfWorkLedger item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work ledger.</summary>
public sealed record StatementOfWorkLedgerValidator(List<IStatementOfWorkLedgerRule> Rules)
{
    public List<string> Validate(StatementOfWorkLedger item)
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

    public void RequireValid(StatementOfWorkLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkLedger> items)
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

/// <summary>Functions of statement of work ledger rules.</summary>
public static class StatementOfWorkLedgerRules
{
    public static StatementOfWorkLedgerValidator DefaultStatementOfWorkLedgerValidator()
    {
        return new StatementOfWorkLedgerValidator(
            new List<IStatementOfWorkLedgerRule>
            {
                new StatementOfWorkLedgerRequiredNameRule("statement of work ledger needs a value"),
                new StatementOfWorkLedgerRequiredCodeRule("statement of work ledger needs this field"),
                new StatementOfWorkLedgerRateRangeRule(6.6, 4010.3),
                new StatementOfWorkLedgerAllowedManagerIdRule(new List<string> { "seasonal" }),
                new StatementOfWorkLedgerCodeLengthRule(32),
                new StatementOfWorkLedgerRequiredSummaryRule("statement of work ledger needs a value"),
            });
    }
}
