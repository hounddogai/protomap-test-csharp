namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback policy ack preference must pass. Check returns its problems, or none.</summary>
public interface IFallbackPolicyAckPreferenceRule
{
    List<string> Check(FallbackPolicyAckPreference item);
}

public sealed record FallbackPolicyAckPreferenceRequiredUnitRule(string Message) : IFallbackPolicyAckPreferenceRule
{
    public List<string> Check(FallbackPolicyAckPreference item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckPreferenceDiscountRangeRule(double Min, double Max) : IFallbackPolicyAckPreferenceRule
{
    public List<string> Check(FallbackPolicyAckPreference item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckPreferenceAllowedTitleRule(List<string> Allowed) : IFallbackPolicyAckPreferenceRule
{
    public List<string> Check(FallbackPolicyAckPreference item)
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

public sealed record FallbackPolicyAckPreferenceRequiredCodeRule(string Message) : IFallbackPolicyAckPreferenceRule
{
    public List<string> Check(FallbackPolicyAckPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckPreferenceCountryLengthRule(int MaxLength) : IFallbackPolicyAckPreferenceRule
{
    public List<string> Check(FallbackPolicyAckPreference item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPolicyAckPreferenceRequiredCountryRule(string Message) : IFallbackPolicyAckPreferenceRule
{
    public List<string> Check(FallbackPolicyAckPreference item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback policy ack preference.</summary>
public sealed record FallbackPolicyAckPreferenceValidator(List<IFallbackPolicyAckPreferenceRule> Rules)
{
    public List<string> Validate(FallbackPolicyAckPreference item)
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

    public void RequireValid(FallbackPolicyAckPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPolicyAckPreference> items)
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

/// <summary>Functions of fallback policy ack preference rules.</summary>
public static class FallbackPolicyAckPreferenceRules
{
    public static FallbackPolicyAckPreferenceValidator DefaultFallbackPolicyAckPreferenceValidator()
    {
        return new FallbackPolicyAckPreferenceValidator(
            new List<IFallbackPolicyAckPreferenceRule>
            {
                new FallbackPolicyAckPreferenceRequiredUnitRule("fallback policy ack preference needs this field"),
                new FallbackPolicyAckPreferenceDiscountRangeRule(2.1, 4941.2),
                new FallbackPolicyAckPreferenceAllowedTitleRule(new List<string> { "weighted", "regional" }),
                new FallbackPolicyAckPreferenceRequiredCodeRule("fallback policy ack preference needs a value"),
                new FallbackPolicyAckPreferenceCountryLengthRule(80),
                new FallbackPolicyAckPreferenceRequiredCountryRule("fallback policy ack preference needs an entry"),
            });
    }
}
