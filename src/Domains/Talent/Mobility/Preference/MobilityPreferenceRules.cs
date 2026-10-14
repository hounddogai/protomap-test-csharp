namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility preference must pass. Check returns its problems, or none.</summary>
public interface IMobilityPreferenceRule
{
    List<string> Check(MobilityPreference item);
}

public sealed record MobilityPreferenceRequiredTitleRule(string Message) : IMobilityPreferenceRule
{
    public List<string> Check(MobilityPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityPreferenceAllowedBadgeRule(List<string> Allowed) : IMobilityPreferenceRule
{
    public List<string> Check(MobilityPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record MobilityPreferenceScoreRangeRule(double Min, double Max) : IMobilityPreferenceRule
{
    public List<string> Check(MobilityPreference item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityPreferenceTitleLengthRule(int MaxLength) : IMobilityPreferenceRule
{
    public List<string> Check(MobilityPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityPreferenceApprovedGuardRule(int Limit) : IMobilityPreferenceRule
{
    public List<string> Check(MobilityPreference item)
    {
        if (item.Approved && item.TenureMonths > Limit)
        {
            return new List<string> { $"tenure months {item.TenureMonths} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record MobilityPreferenceCodePrefixRule(string Prefix) : IMobilityPreferenceRule
{
    public List<string> Check(MobilityPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility preference.</summary>
public sealed record MobilityPreferenceValidator(List<IMobilityPreferenceRule> Rules)
{
    public List<string> Validate(MobilityPreference item)
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

    public void RequireValid(MobilityPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityPreference> items)
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

/// <summary>Functions of mobility preference rules.</summary>
public static class MobilityPreferenceRules
{
    public static MobilityPreferenceValidator DefaultMobilityPreferenceValidator()
    {
        return new MobilityPreferenceValidator(
            new List<IMobilityPreferenceRule>
            {
                new MobilityPreferenceRequiredTitleRule("mobility preference needs a value"),
                new MobilityPreferenceAllowedBadgeRule(new List<string> { "tiered", "weighted" }),
                new MobilityPreferenceScoreRangeRule(1.8, 2765.2),
                new MobilityPreferenceTitleLengthRule(3600),
                new MobilityPreferenceApprovedGuardRule(80),
                new MobilityPreferenceCodePrefixRule("ACM"),
            });
    }
}
