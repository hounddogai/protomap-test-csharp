namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>A rule a primary statement of work window must pass. Check returns its problems, or none.</summary>
public interface IPrimaryStatementOfWorkWindowRule
{
    List<string> Check(PrimaryStatementOfWorkWindow item);
}

public sealed record PrimaryStatementOfWorkWindowCodeLengthRule(int MaxLength) : IPrimaryStatementOfWorkWindowRule
{
    public List<string> Check(PrimaryStatementOfWorkWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryStatementOfWorkWindowRequiredTitleRule(string Message) : IPrimaryStatementOfWorkWindowRule
{
    public List<string> Check(PrimaryStatementOfWorkWindow item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryStatementOfWorkWindowCostCenterLengthRule(int MaxLength) : IPrimaryStatementOfWorkWindowRule
{
    public List<string> Check(PrimaryStatementOfWorkWindow item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryStatementOfWorkWindowRequiredCostCenterRule(string Message) : IPrimaryStatementOfWorkWindowRule
{
    public List<string> Check(PrimaryStatementOfWorkWindow item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryStatementOfWorkWindowCodePrefixRule(string Prefix) : IPrimaryStatementOfWorkWindowRule
{
    public List<string> Check(PrimaryStatementOfWorkWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary statement of work window.</summary>
public sealed record PrimaryStatementOfWorkWindowValidator(List<IPrimaryStatementOfWorkWindowRule> Rules)
{
    public List<string> Validate(PrimaryStatementOfWorkWindow item)
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

    public void RequireValid(PrimaryStatementOfWorkWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryStatementOfWorkWindow> items)
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

/// <summary>Functions of primary statement of work window rules.</summary>
public static class PrimaryStatementOfWorkWindowRules
{
    public static PrimaryStatementOfWorkWindowValidator DefaultPrimaryStatementOfWorkWindowValidator()
    {
        return new PrimaryStatementOfWorkWindowValidator(
            new List<IPrimaryStatementOfWorkWindowRule>
            {
                new PrimaryStatementOfWorkWindowCodeLengthRule(86400),
                new PrimaryStatementOfWorkWindowRequiredTitleRule("primary statement of work window needs a value"),
                new PrimaryStatementOfWorkWindowCostCenterLengthRule(1000),
                new PrimaryStatementOfWorkWindowRequiredCostCenterRule(
                    "primary statement of work window needs an entry"),
                new PrimaryStatementOfWorkWindowCodePrefixRule("Q"),
            });
    }
}
