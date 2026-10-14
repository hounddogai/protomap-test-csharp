namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>A rule a adaptive adjustment claim must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAdjustmentClaimRule
{
    List<string> Check(AdaptiveAdjustmentClaim item);
}

public sealed record AdaptiveAdjustmentClaimLocaleLengthRule(int MaxLength) : IAdaptiveAdjustmentClaimRule
{
    public List<string> Check(AdaptiveAdjustmentClaim item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAdjustmentClaimTitleLengthRule(int MaxLength) : IAdaptiveAdjustmentClaimRule
{
    public List<string> Check(AdaptiveAdjustmentClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAdjustmentClaimRequiredTitleRule(string Message) : IAdaptiveAdjustmentClaimRule
{
    public List<string> Check(AdaptiveAdjustmentClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive adjustment claim.</summary>
public sealed record AdaptiveAdjustmentClaimValidator(List<IAdaptiveAdjustmentClaimRule> Rules)
{
    public List<string> Validate(AdaptiveAdjustmentClaim item)
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

    public void RequireValid(AdaptiveAdjustmentClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAdjustmentClaim> items)
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

/// <summary>Functions of adaptive adjustment claim rules.</summary>
public static class AdaptiveAdjustmentClaimRules
{
    public static AdaptiveAdjustmentClaimValidator DefaultAdaptiveAdjustmentClaimValidator()
    {
        return new AdaptiveAdjustmentClaimValidator(
            new List<IAdaptiveAdjustmentClaimRule>
            {
                new AdaptiveAdjustmentClaimLocaleLengthRule(255),
                new AdaptiveAdjustmentClaimTitleLengthRule(16),
                new AdaptiveAdjustmentClaimRequiredTitleRule("adaptive adjustment claim needs this field"),
            });
    }
}
