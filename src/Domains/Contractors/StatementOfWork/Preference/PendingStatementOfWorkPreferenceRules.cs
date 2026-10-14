namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>A rule a pending statement of work preference must pass. Check returns its problems, or none.</summary>
public interface IPendingStatementOfWorkPreferenceRule
{
    List<string> Check(PendingStatementOfWorkPreference item);
}

public sealed record PendingStatementOfWorkPreferenceDiscountRangeRule(double Min, double Max) : IPendingStatementOfWorkPreferenceRule
{
    public List<string> Check(PendingStatementOfWorkPreference item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingStatementOfWorkPreferenceNameLengthRule(int MaxLength) : IPendingStatementOfWorkPreferenceRule
{
    public List<string> Check(PendingStatementOfWorkPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingStatementOfWorkPreferenceDurationMinutesRangeRule(int Min, int Max) : IPendingStatementOfWorkPreferenceRule
{
    public List<string> Check(PendingStatementOfWorkPreference item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingStatementOfWorkPreferenceFteRangeRule(double Min, double Max) : IPendingStatementOfWorkPreferenceRule
{
    public List<string> Check(PendingStatementOfWorkPreference item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending statement of work preference.</summary>
public sealed record PendingStatementOfWorkPreferenceValidator(List<IPendingStatementOfWorkPreferenceRule> Rules)
{
    public List<string> Validate(PendingStatementOfWorkPreference item)
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

    public void RequireValid(PendingStatementOfWorkPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingStatementOfWorkPreference> items)
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

/// <summary>Functions of pending statement of work preference rules.</summary>
public static class PendingStatementOfWorkPreferenceRules
{
    public static PendingStatementOfWorkPreferenceValidator DefaultPendingStatementOfWorkPreferenceValidator()
    {
        return new PendingStatementOfWorkPreferenceValidator(
            new List<IPendingStatementOfWorkPreferenceRule>
            {
                new PendingStatementOfWorkPreferenceDiscountRangeRule(5.2, 518.1),
                new PendingStatementOfWorkPreferenceNameLengthRule(64),
                new PendingStatementOfWorkPreferenceDurationMinutesRangeRule(80, 120),
                new PendingStatementOfWorkPreferenceFteRangeRule(3.1, 947.2),
            });
    }
}
