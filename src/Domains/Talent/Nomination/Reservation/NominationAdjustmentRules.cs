namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination adjustment must pass. Check returns its problems, or none.</summary>
public interface INominationAdjustmentRule
{
    List<string> Check(NominationAdjustment item);
}

public sealed record NominationAdjustmentAllowedTitleRule(List<string> Allowed) : INominationAdjustmentRule
{
    public List<string> Check(NominationAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record NominationAdjustmentRequiredCodeRule(string Message) : INominationAdjustmentRule
{
    public List<string> Check(NominationAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationAdjustmentWeightRangeRule(double Min, double Max) : INominationAdjustmentRule
{
    public List<string> Check(NominationAdjustment item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record NominationAdjustmentDiscountRangeRule(double Min, double Max) : INominationAdjustmentRule
{
    public List<string> Check(NominationAdjustment item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record NominationAdjustmentPhoneLengthRule(int MaxLength) : INominationAdjustmentRule
{
    public List<string> Check(NominationAdjustment item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationAdjustmentCodePrefixRule(string Prefix) : INominationAdjustmentRule
{
    public List<string> Check(NominationAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination adjustment.</summary>
public sealed record NominationAdjustmentValidator(List<INominationAdjustmentRule> Rules)
{
    public List<string> Validate(NominationAdjustment item)
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

    public void RequireValid(NominationAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationAdjustment> items)
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

/// <summary>Functions of nomination adjustment rules.</summary>
public static class NominationAdjustmentRules
{
    public static NominationAdjustmentValidator DefaultNominationAdjustmentValidator()
    {
        return new NominationAdjustmentValidator(
            new List<INominationAdjustmentRule>
            {
                new NominationAdjustmentAllowedTitleRule(new List<string> { "manual", "seasonal", "capped" }),
                new NominationAdjustmentRequiredCodeRule("nomination adjustment needs an entry"),
                new NominationAdjustmentWeightRangeRule(0.7, 1508.6),
                new NominationAdjustmentDiscountRangeRule(1.5, 3404.1),
                new NominationAdjustmentPhoneLengthRule(32),
                new NominationAdjustmentCodePrefixRule("ACM"),
            });
    }
}
