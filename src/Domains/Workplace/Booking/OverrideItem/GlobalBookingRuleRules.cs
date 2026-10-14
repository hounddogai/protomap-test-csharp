namespace Acme.Workforce.Workplace.Booking.OverrideItem;

/// <summary>A rule a global booking rule must pass. Check returns its problems, or none.</summary>
public interface IGlobalBookingRuleRule
{
    List<string> Check(GlobalBookingRule item);
}

public sealed record GlobalBookingRuleScoreRangeRule(double Min, double Max) : IGlobalBookingRuleRule
{
    public List<string> Check(GlobalBookingRule item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBookingRuleCodePrefixRule(string Prefix) : IGlobalBookingRuleRule
{
    public List<string> Check(GlobalBookingRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBookingRuleLabelLengthRule(int MaxLength) : IGlobalBookingRuleRule
{
    public List<string> Check(GlobalBookingRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalBookingRuleRequiredLabelRule(string Message) : IGlobalBookingRuleRule
{
    public List<string> Check(GlobalBookingRule item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global booking rule.</summary>
public sealed record GlobalBookingRuleValidator(List<IGlobalBookingRuleRule> Rules)
{
    public List<string> Validate(GlobalBookingRule item)
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

    public void RequireValid(GlobalBookingRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalBookingRule> items)
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

/// <summary>Functions of global booking rule rules.</summary>
public static class GlobalBookingRuleRules
{
    public static GlobalBookingRuleValidator DefaultGlobalBookingRuleValidator()
    {
        return new GlobalBookingRuleValidator(
            new List<IGlobalBookingRuleRule>
            {
                new GlobalBookingRuleScoreRangeRule(4.1, 2683.9),
                new GlobalBookingRuleCodePrefixRule("OPS"),
                new GlobalBookingRuleLabelLengthRule(80),
                new GlobalBookingRuleRequiredLabelRule("global booking rule needs this field"),
            });
    }
}
