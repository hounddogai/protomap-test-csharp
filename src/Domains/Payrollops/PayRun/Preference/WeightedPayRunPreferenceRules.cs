namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted pay run preference must pass. Check returns its problems, or none.</summary>
public interface IWeightedPayRunPreferenceRule
{
    List<string> Check(WeightedPayRunPreference item);
}

public sealed record WeightedPayRunPreferenceScoreRangeRule(double Min, double Max) : IWeightedPayRunPreferenceRule
{
    public List<string> Check(WeightedPayRunPreference item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedPayRunPreferenceRegionLengthRule(int MaxLength) : IWeightedPayRunPreferenceRule
{
    public List<string> Check(WeightedPayRunPreference item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayRunPreferenceCodePrefixRule(string Prefix) : IWeightedPayRunPreferenceRule
{
    public List<string> Check(WeightedPayRunPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayRunPreferenceAllowedTitleRule(List<string> Allowed) : IWeightedPayRunPreferenceRule
{
    public List<string> Check(WeightedPayRunPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayRunPreferenceRequiredSourceRule(string Message) : IWeightedPayRunPreferenceRule
{
    public List<string> Check(WeightedPayRunPreference item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedPayRunPreferenceLimitRangeRule(int Min, int Max) : IWeightedPayRunPreferenceRule
{
    public List<string> Check(WeightedPayRunPreference item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a weighted pay run preference.</summary>
public sealed record WeightedPayRunPreferenceValidator(List<IWeightedPayRunPreferenceRule> Rules)
{
    public List<string> Validate(WeightedPayRunPreference item)
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

    public void RequireValid(WeightedPayRunPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPayRunPreference> items)
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

/// <summary>Functions of weighted pay run preference rules.</summary>
public static class WeightedPayRunPreferenceRules
{
    public static WeightedPayRunPreferenceValidator DefaultWeightedPayRunPreferenceValidator()
    {
        return new WeightedPayRunPreferenceValidator(
            new List<IWeightedPayRunPreferenceRule>
            {
                new WeightedPayRunPreferenceScoreRangeRule(9.0, 3649.4),
                new WeightedPayRunPreferenceRegionLengthRule(255),
                new WeightedPayRunPreferenceCodePrefixRule("OPS"),
                new WeightedPayRunPreferenceAllowedTitleRule(new List<string> { "manual" }),
                new WeightedPayRunPreferenceRequiredSourceRule("weighted pay run preference needs this field"),
                new WeightedPayRunPreferenceLimitRangeRule(255, 1000),
            });
    }
}
