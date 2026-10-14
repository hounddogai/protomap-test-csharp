namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>A rule a pay run preference must pass. Check returns its problems, or none.</summary>
public interface IPayRunPreferenceRule
{
    List<string> Check(PayRunPreference item);
}

public sealed record PayRunPreferenceBudgetRangeRule(double Min, double Max) : IPayRunPreferenceRule
{
    public List<string> Check(PayRunPreference item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunPreferenceExpiresAtRangeRule(int Min, int Max) : IPayRunPreferenceRule
{
    public List<string> Check(PayRunPreference item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunPreferenceEndsAtRangeRule(int Min, int Max) : IPayRunPreferenceRule
{
    public List<string> Check(PayRunPreference item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunPreferenceRequiredCountryRule(string Message) : IPayRunPreferenceRule
{
    public List<string> Check(PayRunPreference item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunPreferenceCodeLengthRule(int MaxLength) : IPayRunPreferenceRule
{
    public List<string> Check(PayRunPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run preference.</summary>
public sealed record PayRunPreferenceValidator(List<IPayRunPreferenceRule> Rules)
{
    public List<string> Validate(PayRunPreference item)
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

    public void RequireValid(PayRunPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunPreference> items)
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

/// <summary>Functions of pay run preference rules.</summary>
public static class PayRunPreferenceRules
{
    public static PayRunPreferenceValidator DefaultPayRunPreferenceValidator()
    {
        return new PayRunPreferenceValidator(
            new List<IPayRunPreferenceRule>
            {
                new PayRunPreferenceBudgetRangeRule(2.0, 2051.9),
                new PayRunPreferenceExpiresAtRangeRule(255, 86400),
                new PayRunPreferenceEndsAtRangeRule(80, 3600),
                new PayRunPreferenceRequiredCountryRule("pay run preference needs an entry"),
                new PayRunPreferenceCodeLengthRule(16),
            });
    }
}
