namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>A rule a regional statement of work digest must pass. Check returns its problems, or none.</summary>
public interface IRegionalStatementOfWorkDigestRule
{
    List<string> Check(RegionalStatementOfWorkDigest item);
}

public sealed record RegionalStatementOfWorkDigestCodeLengthRule(int MaxLength) : IRegionalStatementOfWorkDigestRule
{
    public List<string> Check(RegionalStatementOfWorkDigest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalStatementOfWorkDigestRequiredNameRule(string Message) : IRegionalStatementOfWorkDigestRule
{
    public List<string> Check(RegionalStatementOfWorkDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalStatementOfWorkDigestRequiredTitleRule(string Message) : IRegionalStatementOfWorkDigestRule
{
    public List<string> Check(RegionalStatementOfWorkDigest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional statement of work digest.</summary>
public sealed record RegionalStatementOfWorkDigestValidator(List<IRegionalStatementOfWorkDigestRule> Rules)
{
    public List<string> Validate(RegionalStatementOfWorkDigest item)
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

    public void RequireValid(RegionalStatementOfWorkDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalStatementOfWorkDigest> items)
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

/// <summary>Functions of regional statement of work digest rules.</summary>
public static class RegionalStatementOfWorkDigestRules
{
    public static RegionalStatementOfWorkDigestValidator DefaultRegionalStatementOfWorkDigestValidator()
    {
        return new RegionalStatementOfWorkDigestValidator(
            new List<IRegionalStatementOfWorkDigestRule>
            {
                new RegionalStatementOfWorkDigestCodeLengthRule(1000),
                new RegionalStatementOfWorkDigestRequiredNameRule("regional statement of work digest needs an entry"),
                new RegionalStatementOfWorkDigestRequiredTitleRule("regional statement of work digest needs a value"),
            });
    }
}
