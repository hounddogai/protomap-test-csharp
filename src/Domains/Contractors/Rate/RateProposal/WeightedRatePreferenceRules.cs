namespace Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>A rule a weighted rate preference must pass. Check returns its problems, or none.</summary>
public interface IWeightedRatePreferenceRule
{
    List<string> Check(WeightedRatePreference item);
}

public sealed record WeightedRatePreferenceRequiredTitleRule(string Message) : IWeightedRatePreferenceRule
{
    public List<string> Check(WeightedRatePreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedRatePreferenceCodePrefixRule(string Prefix) : IWeightedRatePreferenceRule
{
    public List<string> Check(WeightedRatePreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedRatePreferenceEndsAtRangeRule(int Min, int Max) : IWeightedRatePreferenceRule
{
    public List<string> Check(WeightedRatePreference item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted rate preference.</summary>
public sealed record WeightedRatePreferenceValidator(List<IWeightedRatePreferenceRule> Rules)
{
    public List<string> Validate(WeightedRatePreference item)
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

    public void RequireValid(WeightedRatePreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedRatePreference> items)
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

/// <summary>Functions of weighted rate preference rules.</summary>
public static class WeightedRatePreferenceRules
{
    public static WeightedRatePreferenceValidator DefaultWeightedRatePreferenceValidator()
    {
        return new WeightedRatePreferenceValidator(
            new List<IWeightedRatePreferenceRule>
            {
                new WeightedRatePreferenceRequiredTitleRule("weighted rate preference needs this field"),
                new WeightedRatePreferenceCodePrefixRule("REF"),
                new WeightedRatePreferenceEndsAtRangeRule(32, 64),
            });
    }
}
