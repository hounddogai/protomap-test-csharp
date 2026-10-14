namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>A rule a primary retro pay preference must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRetroPayPreferenceRule
{
    List<string> Check(PrimaryRetroPayPreference item);
}

public sealed record PrimaryRetroPayPreferenceCodePrefixRule(string Prefix) : IPrimaryRetroPayPreferenceRule
{
    public List<string> Check(PrimaryRetroPayPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRetroPayPreferenceRequiredBadgeRule(string Message) : IPrimaryRetroPayPreferenceRule
{
    public List<string> Check(PrimaryRetroPayPreference item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRetroPayPreferenceEndsAtRangeRule(int Min, int Max) : IPrimaryRetroPayPreferenceRule
{
    public List<string> Check(PrimaryRetroPayPreference item)
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

public sealed record PrimaryRetroPayPreferenceVisibleGuardRule(int Limit) : IPrimaryRetroPayPreferenceRule
{
    public List<string> Check(PrimaryRetroPayPreference item)
    {
        if (item.Visible && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary retro pay preference.</summary>
public sealed record PrimaryRetroPayPreferenceValidator(List<IPrimaryRetroPayPreferenceRule> Rules)
{
    public List<string> Validate(PrimaryRetroPayPreference item)
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

    public void RequireValid(PrimaryRetroPayPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRetroPayPreference> items)
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

/// <summary>Functions of primary retro pay preference rules.</summary>
public static class PrimaryRetroPayPreferenceRules
{
    public static PrimaryRetroPayPreferenceValidator DefaultPrimaryRetroPayPreferenceValidator()
    {
        return new PrimaryRetroPayPreferenceValidator(
            new List<IPrimaryRetroPayPreferenceRule>
            {
                new PrimaryRetroPayPreferenceCodePrefixRule("OPS"),
                new PrimaryRetroPayPreferenceRequiredBadgeRule("primary retro pay preference needs a value"),
                new PrimaryRetroPayPreferenceEndsAtRangeRule(80, 255),
                new PrimaryRetroPayPreferenceVisibleGuardRule(120),
            });
    }
}
