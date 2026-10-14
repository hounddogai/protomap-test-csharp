namespace Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>A rule a pay band preference must pass. Check returns its problems, or none.</summary>
public interface IPayBandPreferenceRule
{
    List<string> Check(PayBandPreference item);
}

public sealed record PayBandPreferenceRequiredTitleRule(string Message) : IPayBandPreferenceRule
{
    public List<string> Check(PayBandPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandPreferenceTitleLengthRule(int MaxLength) : IPayBandPreferenceRule
{
    public List<string> Check(PayBandPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandPreferenceActiveGuardRule(int Limit) : IPayBandPreferenceRule
{
    public List<string> Check(PayBandPreference item)
    {
        if (item.Active && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band preference.</summary>
public sealed record PayBandPreferenceValidator(List<IPayBandPreferenceRule> Rules)
{
    public List<string> Validate(PayBandPreference item)
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

    public void RequireValid(PayBandPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandPreference> items)
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

/// <summary>Functions of pay band preference rules.</summary>
public static class PayBandPreferenceRules
{
    public static PayBandPreferenceValidator DefaultPayBandPreferenceValidator()
    {
        return new PayBandPreferenceValidator(
            new List<IPayBandPreferenceRule>
            {
                new PayBandPreferenceRequiredTitleRule("pay band preference needs an entry"),
                new PayBandPreferenceTitleLengthRule(3600),
                new PayBandPreferenceActiveGuardRule(64),
            });
    }
}
