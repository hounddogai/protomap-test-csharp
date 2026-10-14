namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a engagement claim must pass. Check returns its problems, or none.</summary>
public interface IEngagementClaimRule
{
    List<string> Check(EngagementClaim item);
}

public sealed record EngagementClaimRequiredExternalRefRule(string Message) : IEngagementClaimRule
{
    public List<string> Check(EngagementClaim item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementClaimDiscountRangeRule(double Min, double Max) : IEngagementClaimRule
{
    public List<string> Check(EngagementClaim item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementClaimCostRangeRule(double Min, double Max) : IEngagementClaimRule
{
    public List<string> Check(EngagementClaim item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EngagementClaimRequiredLabelRule(string Message) : IEngagementClaimRule
{
    public List<string> Check(EngagementClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementClaimAllowedLabelRule(List<string> Allowed) : IEngagementClaimRule
{
    public List<string> Check(EngagementClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement claim.</summary>
public sealed record EngagementClaimValidator(List<IEngagementClaimRule> Rules)
{
    public List<string> Validate(EngagementClaim item)
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

    public void RequireValid(EngagementClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementClaim> items)
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

/// <summary>Functions of engagement claim rules.</summary>
public static class EngagementClaimRules
{
    public static EngagementClaimValidator DefaultEngagementClaimValidator()
    {
        return new EngagementClaimValidator(
            new List<IEngagementClaimRule>
            {
                new EngagementClaimRequiredExternalRefRule("engagement claim needs this field"),
                new EngagementClaimDiscountRangeRule(3.7, 652.1),
                new EngagementClaimCostRangeRule(1.2, 3556.9),
                new EngagementClaimRequiredLabelRule("engagement claim needs a value"),
                new EngagementClaimAllowedLabelRule(new List<string> { "adaptive" }),
            });
    }
}
