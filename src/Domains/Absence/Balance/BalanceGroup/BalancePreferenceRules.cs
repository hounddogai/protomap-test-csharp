namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance preference must pass. Check returns its problems, or none.</summary>
public interface IBalancePreferenceRule
{
    List<string> Check(BalancePreference item);
}

public sealed record BalancePreferenceTitleLengthRule(int MaxLength) : IBalancePreferenceRule
{
    public List<string> Check(BalancePreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalancePreferenceCodeLengthRule(int MaxLength) : IBalancePreferenceRule
{
    public List<string> Check(BalancePreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalancePreferenceRequiredCodeRule(string Message) : IBalancePreferenceRule
{
    public List<string> Check(BalancePreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalancePreferenceRequiredTitleRule(string Message) : IBalancePreferenceRule
{
    public List<string> Check(BalancePreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalancePreferenceRatingRangeRule(double Min, double Max) : IBalancePreferenceRule
{
    public List<string> Check(BalancePreference item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BalancePreferenceAllowedCurrencyRule(List<string> Allowed) : IBalancePreferenceRule
{
    public List<string> Check(BalancePreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance preference.</summary>
public sealed record BalancePreferenceValidator(List<IBalancePreferenceRule> Rules)
{
    public List<string> Validate(BalancePreference item)
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

    public void RequireValid(BalancePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalancePreference> items)
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

/// <summary>Functions of balance preference rules.</summary>
public static class BalancePreferenceRules
{
    public static BalancePreferenceValidator DefaultBalancePreferenceValidator()
    {
        return new BalancePreferenceValidator(
            new List<IBalancePreferenceRule>
            {
                new BalancePreferenceTitleLengthRule(120),
                new BalancePreferenceCodeLengthRule(255),
                new BalancePreferenceRequiredCodeRule("balance preference needs an entry"),
                new BalancePreferenceRequiredTitleRule("balance preference needs an entry"),
                new BalancePreferenceRatingRangeRule(4.8, 791.7),
                new BalancePreferenceAllowedCurrencyRule(new List<string>()),
            });
    }
}
