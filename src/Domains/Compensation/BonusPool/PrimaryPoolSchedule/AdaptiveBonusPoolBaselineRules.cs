namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>A rule a adaptive bonus pool baseline must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveBonusPoolBaselineRule
{
    List<string> Check(AdaptiveBonusPoolBaseline item);
}

public sealed record AdaptiveBonusPoolBaselineVisibleGuardRule(int Limit) : IAdaptiveBonusPoolBaselineRule
{
    public List<string> Check(AdaptiveBonusPoolBaseline item)
    {
        if (item.Visible && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolBaselineCodePrefixRule(string Prefix) : IAdaptiveBonusPoolBaselineRule
{
    public List<string> Check(AdaptiveBonusPoolBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveBonusPoolBaselineRequiredLabelRule(string Message) : IAdaptiveBonusPoolBaselineRule
{
    public List<string> Check(AdaptiveBonusPoolBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive bonus pool baseline.</summary>
public sealed record AdaptiveBonusPoolBaselineValidator(List<IAdaptiveBonusPoolBaselineRule> Rules)
{
    public List<string> Validate(AdaptiveBonusPoolBaseline item)
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

    public void RequireValid(AdaptiveBonusPoolBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveBonusPoolBaseline> items)
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

/// <summary>Functions of adaptive bonus pool baseline rules.</summary>
public static class AdaptiveBonusPoolBaselineRules
{
    public static AdaptiveBonusPoolBaselineValidator DefaultAdaptiveBonusPoolBaselineValidator()
    {
        return new AdaptiveBonusPoolBaselineValidator(
            new List<IAdaptiveBonusPoolBaselineRule>
            {
                new AdaptiveBonusPoolBaselineVisibleGuardRule(86400),
                new AdaptiveBonusPoolBaselineCodePrefixRule("Q"),
                new AdaptiveBonusPoolBaselineRequiredLabelRule("adaptive bonus pool baseline needs this field"),
            });
    }
}
