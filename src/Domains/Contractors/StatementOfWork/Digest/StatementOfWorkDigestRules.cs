namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>A rule a statement of work digest must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkDigestRule
{
    List<string> Check(StatementOfWorkDigest item);
}

public sealed record StatementOfWorkDigestPriorityRangeRule(int Min, int Max) : IStatementOfWorkDigestRule
{
    public List<string> Check(StatementOfWorkDigest item)
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

public sealed record StatementOfWorkDigestRequiredSalaryBandRule(string Message) : IStatementOfWorkDigestRule
{
    public List<string> Check(StatementOfWorkDigest item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkDigestRequiredRegionRule(string Message) : IStatementOfWorkDigestRule
{
    public List<string> Check(StatementOfWorkDigest item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkDigestRequiredNameRule(string Message) : IStatementOfWorkDigestRule
{
    public List<string> Check(StatementOfWorkDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work digest.</summary>
public sealed record StatementOfWorkDigestValidator(List<IStatementOfWorkDigestRule> Rules)
{
    public List<string> Validate(StatementOfWorkDigest item)
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

    public void RequireValid(StatementOfWorkDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkDigest> items)
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

/// <summary>Functions of statement of work digest rules.</summary>
public static class StatementOfWorkDigestRules
{
    public static StatementOfWorkDigestValidator DefaultStatementOfWorkDigestValidator()
    {
        return new StatementOfWorkDigestValidator(
            new List<IStatementOfWorkDigestRule>
            {
                new StatementOfWorkDigestPriorityRangeRule(3600, 86400),
                new StatementOfWorkDigestRequiredSalaryBandRule("statement of work digest needs a value"),
                new StatementOfWorkDigestRequiredRegionRule("statement of work digest needs an entry"),
                new StatementOfWorkDigestRequiredNameRule("statement of work digest needs this field"),
            });
    }
}
