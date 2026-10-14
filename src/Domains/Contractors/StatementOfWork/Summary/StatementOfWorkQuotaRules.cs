namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>A rule a statement of work quota must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkQuotaRule
{
    List<string> Check(StatementOfWorkQuota item);
}

public sealed record StatementOfWorkQuotaRequiredCodeRule(string Message) : IStatementOfWorkQuotaRule
{
    public List<string> Check(StatementOfWorkQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkQuotaRequiredContactEmailRule(string Message) : IStatementOfWorkQuotaRule
{
    public List<string> Check(StatementOfWorkQuota item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkQuotaCodePrefixRule(string Prefix) : IStatementOfWorkQuotaRule
{
    public List<string> Check(StatementOfWorkQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkQuotaContactEmailLengthRule(int MaxLength) : IStatementOfWorkQuotaRule
{
    public List<string> Check(StatementOfWorkQuota item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work quota.</summary>
public sealed record StatementOfWorkQuotaValidator(List<IStatementOfWorkQuotaRule> Rules)
{
    public List<string> Validate(StatementOfWorkQuota item)
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

    public void RequireValid(StatementOfWorkQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkQuota> items)
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

/// <summary>Functions of statement of work quota rules.</summary>
public static class StatementOfWorkQuotaRules
{
    public static StatementOfWorkQuotaValidator DefaultStatementOfWorkQuotaValidator()
    {
        return new StatementOfWorkQuotaValidator(
            new List<IStatementOfWorkQuotaRule>
            {
                new StatementOfWorkQuotaRequiredCodeRule("statement of work quota needs this field"),
                new StatementOfWorkQuotaRequiredContactEmailRule("statement of work quota needs a value"),
                new StatementOfWorkQuotaCodePrefixRule("OPS"),
                new StatementOfWorkQuotaContactEmailLengthRule(32),
            });
    }
}
