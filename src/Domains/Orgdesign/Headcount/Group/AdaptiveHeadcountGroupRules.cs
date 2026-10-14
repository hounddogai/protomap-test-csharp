namespace Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>A rule a adaptive headcount group must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveHeadcountGroupRule
{
    List<string> Check(AdaptiveHeadcountGroup item);
}

public sealed record AdaptiveHeadcountGroupRequiredCodeRule(string Message) : IAdaptiveHeadcountGroupRule
{
    public List<string> Check(AdaptiveHeadcountGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountGroupManagerIdLengthRule(int MaxLength) : IAdaptiveHeadcountGroupRule
{
    public List<string> Check(AdaptiveHeadcountGroup item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountGroupCodePrefixRule(string Prefix) : IAdaptiveHeadcountGroupRule
{
    public List<string> Check(AdaptiveHeadcountGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountGroupRequiredLocaleRule(string Message) : IAdaptiveHeadcountGroupRule
{
    public List<string> Check(AdaptiveHeadcountGroup item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountGroupWindowDaysRangeRule(int Min, int Max) : IAdaptiveHeadcountGroupRule
{
    public List<string> Check(AdaptiveHeadcountGroup item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHeadcountGroupRequiredTitleRule(string Message) : IAdaptiveHeadcountGroupRule
{
    public List<string> Check(AdaptiveHeadcountGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive headcount group.</summary>
public sealed record AdaptiveHeadcountGroupValidator(List<IAdaptiveHeadcountGroupRule> Rules)
{
    public List<string> Validate(AdaptiveHeadcountGroup item)
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

    public void RequireValid(AdaptiveHeadcountGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveHeadcountGroup> items)
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

/// <summary>Functions of adaptive headcount group rules.</summary>
public static class AdaptiveHeadcountGroupRules
{
    public static AdaptiveHeadcountGroupValidator DefaultAdaptiveHeadcountGroupValidator()
    {
        return new AdaptiveHeadcountGroupValidator(
            new List<IAdaptiveHeadcountGroupRule>
            {
                new AdaptiveHeadcountGroupRequiredCodeRule("adaptive headcount group needs this field"),
                new AdaptiveHeadcountGroupManagerIdLengthRule(86400),
                new AdaptiveHeadcountGroupCodePrefixRule("ACM"),
                new AdaptiveHeadcountGroupRequiredLocaleRule("adaptive headcount group needs this field"),
                new AdaptiveHeadcountGroupWindowDaysRangeRule(80, 3600),
                new AdaptiveHeadcountGroupRequiredTitleRule("adaptive headcount group needs a value"),
            });
    }
}
