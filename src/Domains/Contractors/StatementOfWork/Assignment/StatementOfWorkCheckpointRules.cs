namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work checkpoint must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkCheckpointRule
{
    List<string> Check(StatementOfWorkCheckpoint item);
}

public sealed record StatementOfWorkCheckpointCodePrefixRule(string Prefix) : IStatementOfWorkCheckpointRule
{
    public List<string> Check(StatementOfWorkCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkCheckpointAllowedCodeRule(List<string> Allowed) : IStatementOfWorkCheckpointRule
{
    public List<string> Check(StatementOfWorkCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkCheckpointRequiredTitleRule(string Message) : IStatementOfWorkCheckpointRule
{
    public List<string> Check(StatementOfWorkCheckpoint item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkCheckpointMaximumRangeRule(double Min, double Max) : IStatementOfWorkCheckpointRule
{
    public List<string> Check(StatementOfWorkCheckpoint item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work checkpoint.</summary>
public sealed record StatementOfWorkCheckpointValidator(List<IStatementOfWorkCheckpointRule> Rules)
{
    public List<string> Validate(StatementOfWorkCheckpoint item)
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

    public void RequireValid(StatementOfWorkCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkCheckpoint> items)
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

/// <summary>Functions of statement of work checkpoint rules.</summary>
public static class StatementOfWorkCheckpointRules
{
    public static StatementOfWorkCheckpointValidator DefaultStatementOfWorkCheckpointValidator()
    {
        return new StatementOfWorkCheckpointValidator(
            new List<IStatementOfWorkCheckpointRule>
            {
                new StatementOfWorkCheckpointCodePrefixRule("Q"),
                new StatementOfWorkCheckpointAllowedCodeRule(new List<string>()),
                new StatementOfWorkCheckpointRequiredTitleRule("statement of work checkpoint needs an entry"),
                new StatementOfWorkCheckpointMaximumRangeRule(2.4, 2306.3),
            });
    }
}
