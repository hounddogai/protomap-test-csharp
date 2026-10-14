namespace Acme.Workforce.Assets.Refresh.Policy;

/// <summary>A rule a refresh target must pass. Check returns its problems, or none.</summary>
public interface IRefreshTargetRule
{
    List<string> Check(RefreshTarget item);
}

public sealed record RefreshTargetJobCodeLengthRule(int MaxLength) : IRefreshTargetRule
{
    public List<string> Check(RefreshTarget item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshTargetRequiredTitleRule(string Message) : IRefreshTargetRule
{
    public List<string> Check(RefreshTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshTargetCodeLengthRule(int MaxLength) : IRefreshTargetRule
{
    public List<string> Check(RefreshTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshTargetRequiredJobCodeRule(string Message) : IRefreshTargetRule
{
    public List<string> Check(RefreshTarget item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshTargetMaximumRangeRule(double Min, double Max) : IRefreshTargetRule
{
    public List<string> Check(RefreshTarget item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshTargetCodePrefixRule(string Prefix) : IRefreshTargetRule
{
    public List<string> Check(RefreshTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh target.</summary>
public sealed record RefreshTargetValidator(List<IRefreshTargetRule> Rules)
{
    public List<string> Validate(RefreshTarget item)
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

    public void RequireValid(RefreshTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshTarget> items)
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

/// <summary>Functions of refresh target rules.</summary>
public static class RefreshTargetRules
{
    public static RefreshTargetValidator DefaultRefreshTargetValidator()
    {
        return new RefreshTargetValidator(
            new List<IRefreshTargetRule>
            {
                new RefreshTargetJobCodeLengthRule(16),
                new RefreshTargetRequiredTitleRule("refresh target needs an entry"),
                new RefreshTargetCodeLengthRule(1000),
                new RefreshTargetRequiredJobCodeRule("refresh target needs this field"),
                new RefreshTargetMaximumRangeRule(9.8, 605.4),
                new RefreshTargetCodePrefixRule("REF"),
            });
    }
}
