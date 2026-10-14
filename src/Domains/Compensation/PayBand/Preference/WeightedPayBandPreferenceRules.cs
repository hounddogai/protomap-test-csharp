namespace Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>A rule a weighted pay band preference must pass. Check returns its problems, or none.</summary>
public interface IWeightedPayBandPreferenceRule
{
    List<string> Check(WeightedPayBandPreference item);
}

public sealed record WeightedPayBandPreferenceAmountRangeRule(double Min, double Max) : IWeightedPayBandPreferenceRule
{
    public List<string> Check(WeightedPayBandPreference item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedPayBandPreferenceReviewerLengthRule(int MaxLength) : IWeightedPayBandPreferenceRule
{
    public List<string> Check(WeightedPayBandPreference item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayBandPreferenceRequiredNameRule(string Message) : IWeightedPayBandPreferenceRule
{
    public List<string> Check(WeightedPayBandPreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayBandPreferenceNameLengthRule(int MaxLength) : IWeightedPayBandPreferenceRule
{
    public List<string> Check(WeightedPayBandPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayBandPreferenceRequiredReviewerRule(string Message) : IWeightedPayBandPreferenceRule
{
    public List<string> Check(WeightedPayBandPreference item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted pay band preference.</summary>
public sealed record WeightedPayBandPreferenceValidator(List<IWeightedPayBandPreferenceRule> Rules)
{
    public List<string> Validate(WeightedPayBandPreference item)
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

    public void RequireValid(WeightedPayBandPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPayBandPreference> items)
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

/// <summary>Functions of weighted pay band preference rules.</summary>
public static class WeightedPayBandPreferenceRules
{
    public static WeightedPayBandPreferenceValidator DefaultWeightedPayBandPreferenceValidator()
    {
        return new WeightedPayBandPreferenceValidator(
            new List<IWeightedPayBandPreferenceRule>
            {
                new WeightedPayBandPreferenceAmountRangeRule(6.2, 2244.6),
                new WeightedPayBandPreferenceReviewerLengthRule(16),
                new WeightedPayBandPreferenceRequiredNameRule("weighted pay band preference needs a value"),
                new WeightedPayBandPreferenceNameLengthRule(80),
                new WeightedPayBandPreferenceRequiredReviewerRule("weighted pay band preference needs an entry"),
            });
    }
}
