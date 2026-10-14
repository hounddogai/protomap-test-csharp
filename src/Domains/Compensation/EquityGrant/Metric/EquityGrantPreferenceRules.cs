namespace Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>A rule a equity grant preference must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantPreferenceRule
{
    List<string> Check(EquityGrantPreference item);
}

public sealed record EquityGrantPreferenceEndsAtRangeRule(int Min, int Max) : IEquityGrantPreferenceRule
{
    public List<string> Check(EquityGrantPreference item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantPreferenceLabelLengthRule(int MaxLength) : IEquityGrantPreferenceRule
{
    public List<string> Check(EquityGrantPreference item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantPreferenceCodePrefixRule(string Prefix) : IEquityGrantPreferenceRule
{
    public List<string> Check(EquityGrantPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantPreferenceRequiredLocationRule(string Message) : IEquityGrantPreferenceRule
{
    public List<string> Check(EquityGrantPreference item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant preference.</summary>
public sealed record EquityGrantPreferenceValidator(List<IEquityGrantPreferenceRule> Rules)
{
    public List<string> Validate(EquityGrantPreference item)
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

    public void RequireValid(EquityGrantPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantPreference> items)
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

/// <summary>Functions of equity grant preference rules.</summary>
public static class EquityGrantPreferenceRules
{
    public static EquityGrantPreferenceValidator DefaultEquityGrantPreferenceValidator()
    {
        return new EquityGrantPreferenceValidator(
            new List<IEquityGrantPreferenceRule>
            {
                new EquityGrantPreferenceEndsAtRangeRule(32, 1000),
                new EquityGrantPreferenceLabelLengthRule(120),
                new EquityGrantPreferenceCodePrefixRule("OPS"),
                new EquityGrantPreferenceRequiredLocationRule("equity grant preference needs a value"),
            });
    }
}
