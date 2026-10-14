namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive access badge checkpoint must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAccessBadgeCheckpointRule
{
    List<string> Check(AdaptiveAccessBadgeCheckpoint item);
}

public sealed record AdaptiveAccessBadgeCheckpointDiscountRangeRule(double Min, double Max) : IAdaptiveAccessBadgeCheckpointRule
{
    public List<string> Check(AdaptiveAccessBadgeCheckpoint item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAccessBadgeCheckpointAllowedCodeRule(List<string> Allowed) : IAdaptiveAccessBadgeCheckpointRule
{
    public List<string> Check(AdaptiveAccessBadgeCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAccessBadgeCheckpointNameLengthRule(int MaxLength) : IAdaptiveAccessBadgeCheckpointRule
{
    public List<string> Check(AdaptiveAccessBadgeCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAccessBadgeCheckpointArchivedGuardRule(double Limit) : IAdaptiveAccessBadgeCheckpointRule
{
    public List<string> Check(AdaptiveAccessBadgeCheckpoint item)
    {
        if (item.Archived && item.Minimum > Limit)
        {
            return new List<string> { $"minimum {item.Minimum} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAccessBadgeCheckpointRequiredNameRule(string Message) : IAdaptiveAccessBadgeCheckpointRule
{
    public List<string> Check(AdaptiveAccessBadgeCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive access badge checkpoint.</summary>
public sealed record AdaptiveAccessBadgeCheckpointValidator(List<IAdaptiveAccessBadgeCheckpointRule> Rules)
{
    public List<string> Validate(AdaptiveAccessBadgeCheckpoint item)
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

    public void RequireValid(AdaptiveAccessBadgeCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAccessBadgeCheckpoint> items)
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

/// <summary>Functions of adaptive access badge checkpoint rules.</summary>
public static class AdaptiveAccessBadgeCheckpointRules
{
    public static AdaptiveAccessBadgeCheckpointValidator DefaultAdaptiveAccessBadgeCheckpointValidator()
    {
        return new AdaptiveAccessBadgeCheckpointValidator(
            new List<IAdaptiveAccessBadgeCheckpointRule>
            {
                new AdaptiveAccessBadgeCheckpointDiscountRangeRule(9.2, 2238.4),
                new AdaptiveAccessBadgeCheckpointAllowedCodeRule(new List<string> { "seasonal", "lenient" }),
                new AdaptiveAccessBadgeCheckpointNameLengthRule(120),
                new AdaptiveAccessBadgeCheckpointArchivedGuardRule(3.4),
                new AdaptiveAccessBadgeCheckpointRequiredNameRule("adaptive access badge checkpoint needs a value"),
            });
    }
}
