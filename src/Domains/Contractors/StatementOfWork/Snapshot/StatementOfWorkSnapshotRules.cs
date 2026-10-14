namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>A rule a statement of work snapshot must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkSnapshotRule
{
    List<string> Check(StatementOfWorkSnapshot item);
}

public sealed record StatementOfWorkSnapshotCodePrefixRule(string Prefix) : IStatementOfWorkSnapshotRule
{
    public List<string> Check(StatementOfWorkSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSnapshotTeamLengthRule(int MaxLength) : IStatementOfWorkSnapshotRule
{
    public List<string> Check(StatementOfWorkSnapshot item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSnapshotHoursRangeRule(double Min, double Max) : IStatementOfWorkSnapshotRule
{
    public List<string> Check(StatementOfWorkSnapshot item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkSnapshotRequiredLabelRule(string Message) : IStatementOfWorkSnapshotRule
{
    public List<string> Check(StatementOfWorkSnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work snapshot.</summary>
public sealed record StatementOfWorkSnapshotValidator(List<IStatementOfWorkSnapshotRule> Rules)
{
    public List<string> Validate(StatementOfWorkSnapshot item)
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

    public void RequireValid(StatementOfWorkSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkSnapshot> items)
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

/// <summary>Functions of statement of work snapshot rules.</summary>
public static class StatementOfWorkSnapshotRules
{
    public static StatementOfWorkSnapshotValidator DefaultStatementOfWorkSnapshotValidator()
    {
        return new StatementOfWorkSnapshotValidator(
            new List<IStatementOfWorkSnapshotRule>
            {
                new StatementOfWorkSnapshotCodePrefixRule("X"),
                new StatementOfWorkSnapshotTeamLengthRule(86400),
                new StatementOfWorkSnapshotHoursRangeRule(9.5, 3450.1),
                new StatementOfWorkSnapshotRequiredLabelRule("statement of work snapshot needs this field"),
            });
    }
}
