namespace Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>A rule a adjustment preference must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentPreferenceRule
{
    List<string> Check(AdjustmentPreference item);
}

public sealed record AdjustmentPreferenceMinimumRangeRule(double Min, double Max) : IAdjustmentPreferenceRule
{
    public List<string> Check(AdjustmentPreference item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentPreferenceCurrencyLengthRule(int MaxLength) : IAdjustmentPreferenceRule
{
    public List<string> Check(AdjustmentPreference item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentPreferenceRequiredCodeRule(string Message) : IAdjustmentPreferenceRule
{
    public List<string> Check(AdjustmentPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentPreferenceTitleLengthRule(int MaxLength) : IAdjustmentPreferenceRule
{
    public List<string> Check(AdjustmentPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentPreferenceBudgetRangeRule(double Min, double Max) : IAdjustmentPreferenceRule
{
    public List<string> Check(AdjustmentPreference item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentPreferenceRequiredLabelRule(string Message) : IAdjustmentPreferenceRule
{
    public List<string> Check(AdjustmentPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment preference.</summary>
public sealed record AdjustmentPreferenceValidator(List<IAdjustmentPreferenceRule> Rules)
{
    public List<string> Validate(AdjustmentPreference item)
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

    public void RequireValid(AdjustmentPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentPreference> items)
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

/// <summary>Functions of adjustment preference rules.</summary>
public static class AdjustmentPreferenceRules
{
    public static AdjustmentPreferenceValidator DefaultAdjustmentPreferenceValidator()
    {
        return new AdjustmentPreferenceValidator(
            new List<IAdjustmentPreferenceRule>
            {
                new AdjustmentPreferenceMinimumRangeRule(3.8, 2462.5),
                new AdjustmentPreferenceCurrencyLengthRule(255),
                new AdjustmentPreferenceRequiredCodeRule("adjustment preference needs this field"),
                new AdjustmentPreferenceTitleLengthRule(86400),
                new AdjustmentPreferenceBudgetRangeRule(8.1, 3200.8),
                new AdjustmentPreferenceRequiredLabelRule("adjustment preference needs an entry"),
            });
    }
}
