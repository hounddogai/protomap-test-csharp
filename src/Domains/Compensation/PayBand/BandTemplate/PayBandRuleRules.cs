namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>A rule a pay band rule must pass. Check returns its problems, or none.</summary>
public interface IPayBandRuleRule
{
    List<string> Check(PayBandRule item);
}

public sealed record PayBandRuleDurationMinutesRangeRule(int Min, int Max) : IPayBandRuleRule
{
    public List<string> Check(PayBandRule item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayBandRuleContactEmailLengthRule(int MaxLength) : IPayBandRuleRule
{
    public List<string> Check(PayBandRule item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandRuleRequiredLabelRule(string Message) : IPayBandRuleRule
{
    public List<string> Check(PayBandRule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandRuleSourceLengthRule(int MaxLength) : IPayBandRuleRule
{
    public List<string> Check(PayBandRule item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band rule.</summary>
public sealed record PayBandRuleValidator(List<IPayBandRuleRule> Rules)
{
    public List<string> Validate(PayBandRule item)
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

    public void RequireValid(PayBandRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandRule> items)
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

/// <summary>Functions of pay band rule rules.</summary>
public static class PayBandRuleRules
{
    public static PayBandRuleValidator DefaultPayBandRuleValidator()
    {
        return new PayBandRuleValidator(
            new List<IPayBandRuleRule>
            {
                new PayBandRuleDurationMinutesRangeRule(16, 1000),
                new PayBandRuleContactEmailLengthRule(1000),
                new PayBandRuleRequiredLabelRule("pay band rule needs an entry"),
                new PayBandRuleSourceLengthRule(1000),
            });
    }
}
