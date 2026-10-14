namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>A rule a capped statement of work profile must pass. Check returns its problems, or none.</summary>
public interface ICappedStatementOfWorkProfileRule
{
    List<string> Check(CappedStatementOfWorkProfile item);
}

public sealed record CappedStatementOfWorkProfileRequiredExternalRefRule(string Message) : ICappedStatementOfWorkProfileRule
{
    public List<string> Check(CappedStatementOfWorkProfile item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedStatementOfWorkProfileExternalRefLengthRule(int MaxLength) : ICappedStatementOfWorkProfileRule
{
    public List<string> Check(CappedStatementOfWorkProfile item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedStatementOfWorkProfileCapacityRangeRule(int Min, int Max) : ICappedStatementOfWorkProfileRule
{
    public List<string> Check(CappedStatementOfWorkProfile item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedStatementOfWorkProfileCodePrefixRule(string Prefix) : ICappedStatementOfWorkProfileRule
{
    public List<string> Check(CappedStatementOfWorkProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped statement of work profile.</summary>
public sealed record CappedStatementOfWorkProfileValidator(List<ICappedStatementOfWorkProfileRule> Rules)
{
    public List<string> Validate(CappedStatementOfWorkProfile item)
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

    public void RequireValid(CappedStatementOfWorkProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedStatementOfWorkProfile> items)
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

/// <summary>Functions of capped statement of work profile rules.</summary>
public static class CappedStatementOfWorkProfileRules
{
    public static CappedStatementOfWorkProfileValidator DefaultCappedStatementOfWorkProfileValidator()
    {
        return new CappedStatementOfWorkProfileValidator(
            new List<ICappedStatementOfWorkProfileRule>
            {
                new CappedStatementOfWorkProfileRequiredExternalRefRule(
                    "capped statement of work profile needs an entry"),
                new CappedStatementOfWorkProfileExternalRefLengthRule(64),
                new CappedStatementOfWorkProfileCapacityRangeRule(64, 3600),
                new CappedStatementOfWorkProfileCodePrefixRule("ACM"),
            });
    }
}
