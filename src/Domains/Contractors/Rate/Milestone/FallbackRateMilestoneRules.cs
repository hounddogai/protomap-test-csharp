namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback rate milestone must pass. Check returns its problems, or none.</summary>
public interface IFallbackRateMilestoneRule
{
    List<string> Check(FallbackRateMilestone item);
}

public sealed record FallbackRateMilestoneRequiredCodeRule(string Message) : IFallbackRateMilestoneRule
{
    public List<string> Check(FallbackRateMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateMilestoneAllowedCodeRule(List<string> Allowed) : IFallbackRateMilestoneRule
{
    public List<string> Check(FallbackRateMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateMilestoneCodePrefixRule(string Prefix) : IFallbackRateMilestoneRule
{
    public List<string> Check(FallbackRateMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateMilestoneRevisionRangeRule(int Min, int Max) : IFallbackRateMilestoneRule
{
    public List<string> Check(FallbackRateMilestone item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateMilestoneRequiredNameRule(string Message) : IFallbackRateMilestoneRule
{
    public List<string> Check(FallbackRateMilestone item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRateMilestoneCodeLengthRule(int MaxLength) : IFallbackRateMilestoneRule
{
    public List<string> Check(FallbackRateMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback rate milestone.</summary>
public sealed record FallbackRateMilestoneValidator(List<IFallbackRateMilestoneRule> Rules)
{
    public List<string> Validate(FallbackRateMilestone item)
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

    public void RequireValid(FallbackRateMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRateMilestone> items)
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

/// <summary>Functions of fallback rate milestone rules.</summary>
public static class FallbackRateMilestoneRules
{
    public static FallbackRateMilestoneValidator DefaultFallbackRateMilestoneValidator()
    {
        return new FallbackRateMilestoneValidator(
            new List<IFallbackRateMilestoneRule>
            {
                new FallbackRateMilestoneRequiredCodeRule("fallback rate milestone needs an entry"),
                new FallbackRateMilestoneAllowedCodeRule(new List<string> { "adaptive", "regional" }),
                new FallbackRateMilestoneCodePrefixRule("REF"),
                new FallbackRateMilestoneRevisionRangeRule(80, 255),
                new FallbackRateMilestoneRequiredNameRule("fallback rate milestone needs a value"),
                new FallbackRateMilestoneCodeLengthRule(3600),
            });
    }
}
