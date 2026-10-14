namespace Acme.Workforce.Absence.Balance.Preference;

/// <summary>A rule a default balance preference must pass. Check returns its problems, or none.</summary>
public interface IDefaultBalancePreferenceRule
{
    List<string> Check(DefaultBalancePreference item);
}

public sealed record DefaultBalancePreferenceRequiredNameRule(string Message) : IDefaultBalancePreferenceRule
{
    public List<string> Check(DefaultBalancePreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBalancePreferenceRequiredLabelRule(string Message) : IDefaultBalancePreferenceRule
{
    public List<string> Check(DefaultBalancePreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBalancePreferenceLeaveDaysRangeRule(int Min, int Max) : IDefaultBalancePreferenceRule
{
    public List<string> Check(DefaultBalancePreference item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default balance preference.</summary>
public sealed record DefaultBalancePreferenceValidator(List<IDefaultBalancePreferenceRule> Rules)
{
    public List<string> Validate(DefaultBalancePreference item)
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

    public void RequireValid(DefaultBalancePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBalancePreference> items)
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

/// <summary>Functions of default balance preference rules.</summary>
public static class DefaultBalancePreferenceRules
{
    public static DefaultBalancePreferenceValidator DefaultDefaultBalancePreferenceValidator()
    {
        return new DefaultBalancePreferenceValidator(
            new List<IDefaultBalancePreferenceRule>
            {
                new DefaultBalancePreferenceRequiredNameRule("default balance preference needs this field"),
                new DefaultBalancePreferenceRequiredLabelRule("default balance preference needs an entry"),
                new DefaultBalancePreferenceLeaveDaysRangeRule(120, 1000),
            });
    }
}
