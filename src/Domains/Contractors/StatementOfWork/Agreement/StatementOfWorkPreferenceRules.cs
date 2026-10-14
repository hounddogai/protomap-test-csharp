namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>A rule a statement of work preference must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkPreferenceRule
{
    List<string> Check(StatementOfWorkPreference item);
}

public sealed record StatementOfWorkPreferenceRequiredNameRule(string Message) : IStatementOfWorkPreferenceRule
{
    public List<string> Check(StatementOfWorkPreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkPreferenceUpdatedAtRangeRule(int Min, int Max) : IStatementOfWorkPreferenceRule
{
    public List<string> Check(StatementOfWorkPreference item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkPreferenceSalaryBandLengthRule(int MaxLength) : IStatementOfWorkPreferenceRule
{
    public List<string> Check(StatementOfWorkPreference item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkPreferenceArchivedGuardRule(double Limit) : IStatementOfWorkPreferenceRule
{
    public List<string> Check(StatementOfWorkPreference item)
    {
        if (item.Archived && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkPreferenceRequiredPhoneRule(string Message) : IStatementOfWorkPreferenceRule
{
    public List<string> Check(StatementOfWorkPreference item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work preference.</summary>
public sealed record StatementOfWorkPreferenceValidator(List<IStatementOfWorkPreferenceRule> Rules)
{
    public List<string> Validate(StatementOfWorkPreference item)
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

    public void RequireValid(StatementOfWorkPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkPreference> items)
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

/// <summary>Functions of statement of work preference rules.</summary>
public static class StatementOfWorkPreferenceRules
{
    public static StatementOfWorkPreferenceValidator DefaultStatementOfWorkPreferenceValidator()
    {
        return new StatementOfWorkPreferenceValidator(
            new List<IStatementOfWorkPreferenceRule>
            {
                new StatementOfWorkPreferenceRequiredNameRule("statement of work preference needs a value"),
                new StatementOfWorkPreferenceUpdatedAtRangeRule(64, 1000),
                new StatementOfWorkPreferenceSalaryBandLengthRule(3600),
                new StatementOfWorkPreferenceArchivedGuardRule(1.8),
                new StatementOfWorkPreferenceRequiredPhoneRule("statement of work preference needs this field"),
            });
    }
}
