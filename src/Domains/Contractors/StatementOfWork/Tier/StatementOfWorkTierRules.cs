namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>A rule a statement of work tier must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkTierRule
{
    List<string> Check(StatementOfWorkTier item);
}

public sealed record StatementOfWorkTierRequiredDescriptionRule(string Message) : IStatementOfWorkTierRule
{
    public List<string> Check(StatementOfWorkTier item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTierRequiredTimezoneRule(string Message) : IStatementOfWorkTierRule
{
    public List<string> Check(StatementOfWorkTier item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTierLabelLengthRule(int MaxLength) : IStatementOfWorkTierRule
{
    public List<string> Check(StatementOfWorkTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTierWindowDaysRangeRule(int Min, int Max) : IStatementOfWorkTierRule
{
    public List<string> Check(StatementOfWorkTier item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a statement of work tier.</summary>
public sealed record StatementOfWorkTierValidator(List<IStatementOfWorkTierRule> Rules)
{
    public List<string> Validate(StatementOfWorkTier item)
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

    public void RequireValid(StatementOfWorkTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkTier> items)
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

/// <summary>Functions of statement of work tier rules.</summary>
public static class StatementOfWorkTierRules
{
    public static StatementOfWorkTierValidator DefaultStatementOfWorkTierValidator()
    {
        return new StatementOfWorkTierValidator(
            new List<IStatementOfWorkTierRule>
            {
                new StatementOfWorkTierRequiredDescriptionRule("statement of work tier needs a value"),
                new StatementOfWorkTierRequiredTimezoneRule("statement of work tier needs an entry"),
                new StatementOfWorkTierLabelLengthRule(16),
                new StatementOfWorkTierWindowDaysRangeRule(32, 255),
            });
    }
}
