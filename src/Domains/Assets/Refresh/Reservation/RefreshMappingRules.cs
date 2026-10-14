namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh mapping must pass. Check returns its problems, or none.</summary>
public interface IRefreshMappingRule
{
    List<string> Check(RefreshMapping item);
}

public sealed record RefreshMappingLabelLengthRule(int MaxLength) : IRefreshMappingRule
{
    public List<string> Check(RefreshMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshMappingAllowedTitleRule(List<string> Allowed) : IRefreshMappingRule
{
    public List<string> Check(RefreshMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record RefreshMappingCodePrefixRule(string Prefix) : IRefreshMappingRule
{
    public List<string> Check(RefreshMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshMappingPriorityRangeRule(int Min, int Max) : IRefreshMappingRule
{
    public List<string> Check(RefreshMapping item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshMappingTitleLengthRule(int MaxLength) : IRefreshMappingRule
{
    public List<string> Check(RefreshMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshMappingRequiredCodeRule(string Message) : IRefreshMappingRule
{
    public List<string> Check(RefreshMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh mapping.</summary>
public sealed record RefreshMappingValidator(List<IRefreshMappingRule> Rules)
{
    public List<string> Validate(RefreshMapping item)
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

    public void RequireValid(RefreshMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshMapping> items)
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

/// <summary>Functions of refresh mapping rules.</summary>
public static class RefreshMappingRules
{
    public static RefreshMappingValidator DefaultRefreshMappingValidator()
    {
        return new RefreshMappingValidator(
            new List<IRefreshMappingRule>
            {
                new RefreshMappingLabelLengthRule(3600),
                new RefreshMappingAllowedTitleRule(new List<string> { "fallback", "legacy" }),
                new RefreshMappingCodePrefixRule("REF"),
                new RefreshMappingPriorityRangeRule(64, 255),
                new RefreshMappingTitleLengthRule(64),
                new RefreshMappingRequiredCodeRule("refresh mapping needs a value"),
            });
    }
}
