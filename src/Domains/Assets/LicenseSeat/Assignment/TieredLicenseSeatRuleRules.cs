namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>A rule a tiered license seat rule must pass. Check returns its problems, or none.</summary>
public interface ITieredLicenseSeatRuleRule
{
    List<string> Check(TieredLicenseSeatRule item);
}

public sealed record TieredLicenseSeatRuleRequiredPhoneRule(string Message) : ITieredLicenseSeatRuleRule
{
    public List<string> Check(TieredLicenseSeatRule item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLicenseSeatRuleCodePrefixRule(string Prefix) : ITieredLicenseSeatRuleRule
{
    public List<string> Check(TieredLicenseSeatRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLicenseSeatRulePhoneLengthRule(int MaxLength) : ITieredLicenseSeatRuleRule
{
    public List<string> Check(TieredLicenseSeatRule item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered license seat rule.</summary>
public sealed record TieredLicenseSeatRuleValidator(List<ITieredLicenseSeatRuleRule> Rules)
{
    public List<string> Validate(TieredLicenseSeatRule item)
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

    public void RequireValid(TieredLicenseSeatRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLicenseSeatRule> items)
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

/// <summary>Functions of tiered license seat rule rules.</summary>
public static class TieredLicenseSeatRuleRules
{
    public static TieredLicenseSeatRuleValidator DefaultTieredLicenseSeatRuleValidator()
    {
        return new TieredLicenseSeatRuleValidator(
            new List<ITieredLicenseSeatRuleRule>
            {
                new TieredLicenseSeatRuleRequiredPhoneRule("tiered license seat rule needs this field"),
                new TieredLicenseSeatRuleCodePrefixRule("REF"),
                new TieredLicenseSeatRulePhoneLengthRule(120),
            });
    }
}
