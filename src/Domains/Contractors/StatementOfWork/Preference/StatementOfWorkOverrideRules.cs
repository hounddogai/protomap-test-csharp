namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>A rule a statement of work override must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkOverrideRule
{
    List<string> Check(StatementOfWorkOverride item);
}

public sealed record StatementOfWorkOverrideRequiredCodeRule(string Message) : IStatementOfWorkOverrideRule
{
    public List<string> Check(StatementOfWorkOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkOverrideCurrencyLengthRule(int MaxLength) : IStatementOfWorkOverrideRule
{
    public List<string> Check(StatementOfWorkOverride item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkOverrideLabelLengthRule(int MaxLength) : IStatementOfWorkOverrideRule
{
    public List<string> Check(StatementOfWorkOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkOverrideRequiredCountryRule(string Message) : IStatementOfWorkOverrideRule
{
    public List<string> Check(StatementOfWorkOverride item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work override.</summary>
public sealed record StatementOfWorkOverrideValidator(List<IStatementOfWorkOverrideRule> Rules)
{
    public List<string> Validate(StatementOfWorkOverride item)
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

    public void RequireValid(StatementOfWorkOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkOverride> items)
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

/// <summary>Functions of statement of work override rules.</summary>
public static class StatementOfWorkOverrideRules
{
    public static StatementOfWorkOverrideValidator DefaultStatementOfWorkOverrideValidator()
    {
        return new StatementOfWorkOverrideValidator(
            new List<IStatementOfWorkOverrideRule>
            {
                new StatementOfWorkOverrideRequiredCodeRule("statement of work override needs this field"),
                new StatementOfWorkOverrideCurrencyLengthRule(255),
                new StatementOfWorkOverrideLabelLengthRule(86400),
                new StatementOfWorkOverrideRequiredCountryRule("statement of work override needs a value"),
            });
    }
}
