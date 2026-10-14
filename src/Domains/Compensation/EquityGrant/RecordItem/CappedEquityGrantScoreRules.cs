namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped equity grant score must pass. Check returns its problems, or none.</summary>
public interface ICappedEquityGrantScoreRule
{
    List<string> Check(CappedEquityGrantScore item);
}

public sealed record CappedEquityGrantScoreAllowedCountryRule(List<string> Allowed) : ICappedEquityGrantScoreRule
{
    public List<string> Check(CappedEquityGrantScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is expired" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantScoreJobCodeLengthRule(int MaxLength) : ICappedEquityGrantScoreRule
{
    public List<string> Check(CappedEquityGrantScore item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantScoreRequiredNameRule(string Message) : ICappedEquityGrantScoreRule
{
    public List<string> Check(CappedEquityGrantScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantScoreCodeLengthRule(int MaxLength) : ICappedEquityGrantScoreRule
{
    public List<string> Check(CappedEquityGrantScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantScoreRequiredJobCodeRule(string Message) : ICappedEquityGrantScoreRule
{
    public List<string> Check(CappedEquityGrantScore item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedEquityGrantScoreCodePrefixRule(string Prefix) : ICappedEquityGrantScoreRule
{
    public List<string> Check(CappedEquityGrantScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped equity grant score.</summary>
public sealed record CappedEquityGrantScoreValidator(List<ICappedEquityGrantScoreRule> Rules)
{
    public List<string> Validate(CappedEquityGrantScore item)
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

    public void RequireValid(CappedEquityGrantScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedEquityGrantScore> items)
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

/// <summary>Functions of capped equity grant score rules.</summary>
public static class CappedEquityGrantScoreRules
{
    public static CappedEquityGrantScoreValidator DefaultCappedEquityGrantScoreValidator()
    {
        return new CappedEquityGrantScoreValidator(
            new List<ICappedEquityGrantScoreRule>
            {
                new CappedEquityGrantScoreAllowedCountryRule(new List<string> { "scheduled", "seasonal" }),
                new CappedEquityGrantScoreJobCodeLengthRule(80),
                new CappedEquityGrantScoreRequiredNameRule("capped equity grant score needs this field"),
                new CappedEquityGrantScoreCodeLengthRule(64),
                new CappedEquityGrantScoreRequiredJobCodeRule("capped equity grant score needs an entry"),
                new CappedEquityGrantScoreCodePrefixRule("ACM"),
            });
    }
}
