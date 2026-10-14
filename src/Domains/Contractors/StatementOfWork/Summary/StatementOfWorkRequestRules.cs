namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work request must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkRequestRule
{
    List<string> Check(StatementOfWorkRequest item);
}

public sealed record StatementOfWorkRequestCodePrefixRule(string Prefix) : IStatementOfWorkRequestRule
{
    public List<string> Check(StatementOfWorkRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkRequestRequiredTeamRule(string Message) : IStatementOfWorkRequestRule
{
    public List<string> Check(StatementOfWorkRequest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkRequestSalaryBandLengthRule(int MaxLength) : IStatementOfWorkRequestRule
{
    public List<string> Check(StatementOfWorkRequest item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkRequestAllowedLocaleRule(List<string> Allowed) : IStatementOfWorkRequestRule
{
    public List<string> Check(StatementOfWorkRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkRequestRequiredTitleRule(string Message) : IStatementOfWorkRequestRule
{
    public List<string> Check(StatementOfWorkRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work request.</summary>
public sealed record StatementOfWorkRequestValidator(List<IStatementOfWorkRequestRule> Rules)
{
    public List<string> Validate(StatementOfWorkRequest item)
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

    public void RequireValid(StatementOfWorkRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkRequest> items)
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

/// <summary>Functions of statement of work request rules.</summary>
public static class StatementOfWorkRequestRules
{
    public static StatementOfWorkRequestValidator DefaultStatementOfWorkRequestValidator()
    {
        return new StatementOfWorkRequestValidator(
            new List<IStatementOfWorkRequestRule>
            {
                new StatementOfWorkRequestCodePrefixRule("OPS"),
                new StatementOfWorkRequestRequiredTeamRule("statement of work request needs an entry"),
                new StatementOfWorkRequestSalaryBandLengthRule(32),
                new StatementOfWorkRequestAllowedLocaleRule(new List<string> { "batched", "scheduled" }),
                new StatementOfWorkRequestRequiredTitleRule("statement of work request needs this field"),
            });
    }
}
