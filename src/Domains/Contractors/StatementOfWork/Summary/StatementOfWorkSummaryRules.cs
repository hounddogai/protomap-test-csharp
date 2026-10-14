namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work summary must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkSummaryRule
{
    List<string> Check(StatementOfWorkSummary item);
}

public sealed record StatementOfWorkSummaryRequiredNotesRule(string Message) : IStatementOfWorkSummaryRule
{
    public List<string> Check(StatementOfWorkSummary item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSummaryRequiredRegionRule(string Message) : IStatementOfWorkSummaryRule
{
    public List<string> Check(StatementOfWorkSummary item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSummaryCodePrefixRule(string Prefix) : IStatementOfWorkSummaryRule
{
    public List<string> Check(StatementOfWorkSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSummaryVisibleGuardRule(int Limit) : IStatementOfWorkSummaryRule
{
    public List<string> Check(StatementOfWorkSummary item)
    {
        if (item.Visible && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSummaryAllowedLabelRule(List<string> Allowed) : IStatementOfWorkSummaryRule
{
    public List<string> Check(StatementOfWorkSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSummaryRequiredCodeRule(string Message) : IStatementOfWorkSummaryRule
{
    public List<string> Check(StatementOfWorkSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work summary.</summary>
public sealed record StatementOfWorkSummaryValidator(List<IStatementOfWorkSummaryRule> Rules)
{
    public List<string> Validate(StatementOfWorkSummary item)
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

    public void RequireValid(StatementOfWorkSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkSummary> items)
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

/// <summary>Functions of statement of work summary rules.</summary>
public static class StatementOfWorkSummaryRules
{
    public static StatementOfWorkSummaryValidator DefaultStatementOfWorkSummaryValidator()
    {
        return new StatementOfWorkSummaryValidator(
            new List<IStatementOfWorkSummaryRule>
            {
                new StatementOfWorkSummaryRequiredNotesRule("statement of work summary needs this field"),
                new StatementOfWorkSummaryRequiredRegionRule("statement of work summary needs an entry"),
                new StatementOfWorkSummaryCodePrefixRule("Q"),
                new StatementOfWorkSummaryVisibleGuardRule(255),
                new StatementOfWorkSummaryAllowedLabelRule(new List<string> { "manual", "manual" }),
                new StatementOfWorkSummaryRequiredCodeRule("statement of work summary needs a value"),
            });
    }
}
