namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>A rule a statement of work policy must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkPolicyRule
{
    List<string> Check(StatementOfWorkPolicy item);
}

public sealed record StatementOfWorkPolicyRequiredCountryRule(string Message) : IStatementOfWorkPolicyRule
{
    public List<string> Check(StatementOfWorkPolicy item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkPolicyCodePrefixRule(string Prefix) : IStatementOfWorkPolicyRule
{
    public List<string> Check(StatementOfWorkPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkPolicyRequiredLocaleRule(string Message) : IStatementOfWorkPolicyRule
{
    public List<string> Check(StatementOfWorkPolicy item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work policy.</summary>
public sealed record StatementOfWorkPolicyValidator(List<IStatementOfWorkPolicyRule> Rules)
{
    public List<string> Validate(StatementOfWorkPolicy item)
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

    public void RequireValid(StatementOfWorkPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkPolicy> items)
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

/// <summary>Functions of statement of work policy rules.</summary>
public static class StatementOfWorkPolicyRules
{
    public static StatementOfWorkPolicyValidator DefaultStatementOfWorkPolicyValidator()
    {
        return new StatementOfWorkPolicyValidator(
            new List<IStatementOfWorkPolicyRule>
            {
                new StatementOfWorkPolicyRequiredCountryRule("statement of work policy needs an entry"),
                new StatementOfWorkPolicyCodePrefixRule("X"),
                new StatementOfWorkPolicyRequiredLocaleRule("statement of work policy needs this field"),
            });
    }
}
