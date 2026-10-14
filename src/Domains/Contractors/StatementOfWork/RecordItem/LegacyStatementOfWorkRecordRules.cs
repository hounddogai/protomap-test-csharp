namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>A rule a legacy statement of work record must pass. Check returns its problems, or none.</summary>
public interface ILegacyStatementOfWorkRecordRule
{
    List<string> Check(LegacyStatementOfWorkRecord item);
}

public sealed record LegacyStatementOfWorkRecordRequiredCodeRule(string Message) : ILegacyStatementOfWorkRecordRule
{
    public List<string> Check(LegacyStatementOfWorkRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyStatementOfWorkRecordRequiredDepartmentRule(string Message) : ILegacyStatementOfWorkRecordRule
{
    public List<string> Check(LegacyStatementOfWorkRecord item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyStatementOfWorkRecordRatioRangeRule(double Min, double Max) : ILegacyStatementOfWorkRecordRule
{
    public List<string> Check(LegacyStatementOfWorkRecord item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyStatementOfWorkRecordArchivedGuardRule(int Limit) : ILegacyStatementOfWorkRecordRule
{
    public List<string> Check(LegacyStatementOfWorkRecord item)
    {
        if (item.Archived && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy statement of work record.</summary>
public sealed record LegacyStatementOfWorkRecordValidator(List<ILegacyStatementOfWorkRecordRule> Rules)
{
    public List<string> Validate(LegacyStatementOfWorkRecord item)
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

    public void RequireValid(LegacyStatementOfWorkRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyStatementOfWorkRecord> items)
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

/// <summary>Functions of legacy statement of work record rules.</summary>
public static class LegacyStatementOfWorkRecordRules
{
    public static LegacyStatementOfWorkRecordValidator DefaultLegacyStatementOfWorkRecordValidator()
    {
        return new LegacyStatementOfWorkRecordValidator(
            new List<ILegacyStatementOfWorkRecordRule>
            {
                new LegacyStatementOfWorkRecordRequiredCodeRule("legacy statement of work record needs an entry"),
                new LegacyStatementOfWorkRecordRequiredDepartmentRule(
                    "legacy statement of work record needs an entry"),
                new LegacyStatementOfWorkRecordRatioRangeRule(9.4, 4298.7),
                new LegacyStatementOfWorkRecordArchivedGuardRule(1000),
            });
    }
}
