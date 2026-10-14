namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict pay band preference must pass. Check returns its problems, or none.</summary>
public interface IStrictPayBandPreferenceRule
{
    List<string> Check(StrictPayBandPreference item);
}

public sealed record StrictPayBandPreferenceCodeLengthRule(int MaxLength) : IStrictPayBandPreferenceRule
{
    public List<string> Check(StrictPayBandPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayBandPreferenceAllowedCodeRule(List<string> Allowed) : IStrictPayBandPreferenceRule
{
    public List<string> Check(StrictPayBandPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayBandPreferenceLimitRangeRule(int Min, int Max) : IStrictPayBandPreferenceRule
{
    public List<string> Check(StrictPayBandPreference item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPayBandPreferenceSequenceRangeRule(int Min, int Max) : IStrictPayBandPreferenceRule
{
    public List<string> Check(StrictPayBandPreference item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pay band preference.</summary>
public sealed record StrictPayBandPreferenceValidator(List<IStrictPayBandPreferenceRule> Rules)
{
    public List<string> Validate(StrictPayBandPreference item)
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

    public void RequireValid(StrictPayBandPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPayBandPreference> items)
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

/// <summary>Functions of strict pay band preference rules.</summary>
public static class StrictPayBandPreferenceRules
{
    public static StrictPayBandPreferenceValidator DefaultStrictPayBandPreferenceValidator()
    {
        return new StrictPayBandPreferenceValidator(
            new List<IStrictPayBandPreferenceRule>
            {
                new StrictPayBandPreferenceCodeLengthRule(16),
                new StrictPayBandPreferenceAllowedCodeRule(new List<string> { "primary", "strict" }),
                new StrictPayBandPreferenceLimitRangeRule(64, 255),
                new StrictPayBandPreferenceSequenceRangeRule(32, 255),
            });
    }
}
