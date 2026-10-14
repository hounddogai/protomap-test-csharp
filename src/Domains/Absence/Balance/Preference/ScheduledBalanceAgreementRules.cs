namespace Acme.Workforce.Absence.Balance.Preference;

/// <summary>A rule a scheduled balance agreement must pass. Check returns its problems, or none.</summary>
public interface IScheduledBalanceAgreementRule
{
    List<string> Check(ScheduledBalanceAgreement item);
}

public sealed record ScheduledBalanceAgreementGraceDaysRangeRule(int Min, int Max) : IScheduledBalanceAgreementRule
{
    public List<string> Check(ScheduledBalanceAgreement item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBalanceAgreementRequiredTitleRule(string Message) : IScheduledBalanceAgreementRule
{
    public List<string> Check(ScheduledBalanceAgreement item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBalanceAgreementSourceLengthRule(int MaxLength) : IScheduledBalanceAgreementRule
{
    public List<string> Check(ScheduledBalanceAgreement item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled balance agreement.</summary>
public sealed record ScheduledBalanceAgreementValidator(List<IScheduledBalanceAgreementRule> Rules)
{
    public List<string> Validate(ScheduledBalanceAgreement item)
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

    public void RequireValid(ScheduledBalanceAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBalanceAgreement> items)
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

/// <summary>Functions of scheduled balance agreement rules.</summary>
public static class ScheduledBalanceAgreementRules
{
    public static ScheduledBalanceAgreementValidator DefaultScheduledBalanceAgreementValidator()
    {
        return new ScheduledBalanceAgreementValidator(
            new List<IScheduledBalanceAgreementRule>
            {
                new ScheduledBalanceAgreementGraceDaysRangeRule(86400, 86400),
                new ScheduledBalanceAgreementRequiredTitleRule("scheduled balance agreement needs an entry"),
                new ScheduledBalanceAgreementSourceLengthRule(1000),
            });
    }
}
