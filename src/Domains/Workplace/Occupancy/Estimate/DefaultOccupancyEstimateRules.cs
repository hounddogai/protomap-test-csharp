namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default occupancy estimate must pass. Check returns its problems, or none.</summary>
public interface IDefaultOccupancyEstimateRule
{
    List<string> Check(DefaultOccupancyEstimate item);
}

public sealed record DefaultOccupancyEstimateApprovedGuardRule(double Limit) : IDefaultOccupancyEstimateRule
{
    public List<string> Check(DefaultOccupancyEstimate item)
    {
        if (item.Approved && item.Margin > Limit)
        {
            return new List<string> { $"margin {item.Margin} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record DefaultOccupancyEstimateCodePrefixRule(string Prefix) : IDefaultOccupancyEstimateRule
{
    public List<string> Check(DefaultOccupancyEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultOccupancyEstimateAllowedPhoneRule(List<string> Allowed) : IDefaultOccupancyEstimateRule
{
    public List<string> Check(DefaultOccupancyEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record DefaultOccupancyEstimateRequiredPhoneRule(string Message) : IDefaultOccupancyEstimateRule
{
    public List<string> Check(DefaultOccupancyEstimate item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default occupancy estimate.</summary>
public sealed record DefaultOccupancyEstimateValidator(List<IDefaultOccupancyEstimateRule> Rules)
{
    public List<string> Validate(DefaultOccupancyEstimate item)
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

    public void RequireValid(DefaultOccupancyEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultOccupancyEstimate> items)
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

/// <summary>Functions of default occupancy estimate rules.</summary>
public static class DefaultOccupancyEstimateRules
{
    public static DefaultOccupancyEstimateValidator DefaultDefaultOccupancyEstimateValidator()
    {
        return new DefaultOccupancyEstimateValidator(
            new List<IDefaultOccupancyEstimateRule>
            {
                new DefaultOccupancyEstimateApprovedGuardRule(2.8),
                new DefaultOccupancyEstimateCodePrefixRule("X"),
                new DefaultOccupancyEstimateAllowedPhoneRule(new List<string> { "global", "global", "scheduled" }),
                new DefaultOccupancyEstimateRequiredPhoneRule("default occupancy estimate needs this field"),
            });
    }
}
