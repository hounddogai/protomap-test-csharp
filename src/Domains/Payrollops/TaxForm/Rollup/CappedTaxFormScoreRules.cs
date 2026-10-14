namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>A rule a capped tax form score must pass. Check returns its problems, or none.</summary>
public interface ICappedTaxFormScoreRule
{
    List<string> Check(CappedTaxFormScore item);
}

public sealed record CappedTaxFormScoreContactEmailLengthRule(int MaxLength) : ICappedTaxFormScoreRule
{
    public List<string> Check(CappedTaxFormScore item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTaxFormScoreCodePrefixRule(string Prefix) : ICappedTaxFormScoreRule
{
    public List<string> Check(CappedTaxFormScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTaxFormScoreManagerIdLengthRule(int MaxLength) : ICappedTaxFormScoreRule
{
    public List<string> Check(CappedTaxFormScore item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped tax form score.</summary>
public sealed record CappedTaxFormScoreValidator(List<ICappedTaxFormScoreRule> Rules)
{
    public List<string> Validate(CappedTaxFormScore item)
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

    public void RequireValid(CappedTaxFormScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedTaxFormScore> items)
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

/// <summary>Functions of capped tax form score rules.</summary>
public static class CappedTaxFormScoreRules
{
    public static CappedTaxFormScoreValidator DefaultCappedTaxFormScoreValidator()
    {
        return new CappedTaxFormScoreValidator(
            new List<ICappedTaxFormScoreRule>
            {
                new CappedTaxFormScoreContactEmailLengthRule(1000),
                new CappedTaxFormScoreCodePrefixRule("ACM"),
                new CappedTaxFormScoreManagerIdLengthRule(120),
            });
    }
}
