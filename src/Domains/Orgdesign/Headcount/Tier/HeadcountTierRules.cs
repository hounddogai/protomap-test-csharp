namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount tier must pass. Check returns its problems, or none.</summary>
public interface IHeadcountTierRule
{
    List<string> Check(HeadcountTier item);
}

public sealed record HeadcountTierCodePrefixRule(string Prefix) : IHeadcountTierRule
{
    public List<string> Check(HeadcountTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTierAllowedLabelRule(List<string> Allowed) : IHeadcountTierRule
{
    public List<string> Check(HeadcountTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTierRequiredCodeRule(string Message) : IHeadcountTierRule
{
    public List<string> Check(HeadcountTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTierCodeLengthRule(int MaxLength) : IHeadcountTierRule
{
    public List<string> Check(HeadcountTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTierFteRangeRule(double Min, double Max) : IHeadcountTierRule
{
    public List<string> Check(HeadcountTier item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountTierContactEmailLengthRule(int MaxLength) : IHeadcountTierRule
{
    public List<string> Check(HeadcountTier item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount tier.</summary>
public sealed record HeadcountTierValidator(List<IHeadcountTierRule> Rules)
{
    public List<string> Validate(HeadcountTier item)
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

    public void RequireValid(HeadcountTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountTier> items)
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

/// <summary>Functions of headcount tier rules.</summary>
public static class HeadcountTierRules
{
    public static HeadcountTierValidator DefaultHeadcountTierValidator()
    {
        return new HeadcountTierValidator(
            new List<IHeadcountTierRule>
            {
                new HeadcountTierCodePrefixRule("OPS"),
                new HeadcountTierAllowedLabelRule(new List<string> { "rolling", "lenient" }),
                new HeadcountTierRequiredCodeRule("headcount tier needs this field"),
                new HeadcountTierCodeLengthRule(16),
                new HeadcountTierFteRangeRule(5.9, 502.0),
                new HeadcountTierContactEmailLengthRule(120),
            });
    }
}
