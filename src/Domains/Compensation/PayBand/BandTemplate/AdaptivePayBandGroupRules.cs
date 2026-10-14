namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>A rule a adaptive pay band group must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePayBandGroupRule
{
    List<string> Check(AdaptivePayBandGroup item);
}

public sealed record AdaptivePayBandGroupThresholdRangeRule(double Min, double Max) : IAdaptivePayBandGroupRule
{
    public List<string> Check(AdaptivePayBandGroup item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayBandGroupApprovedGuardRule(int Limit) : IAdaptivePayBandGroupRule
{
    public List<string> Check(AdaptivePayBandGroup item)
    {
        if (item.Approved && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayBandGroupTimezoneLengthRule(int MaxLength) : IAdaptivePayBandGroupRule
{
    public List<string> Check(AdaptivePayBandGroup item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayBandGroupRegionLengthRule(int MaxLength) : IAdaptivePayBandGroupRule
{
    public List<string> Check(AdaptivePayBandGroup item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive pay band group.</summary>
public sealed record AdaptivePayBandGroupValidator(List<IAdaptivePayBandGroupRule> Rules)
{
    public List<string> Validate(AdaptivePayBandGroup item)
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

    public void RequireValid(AdaptivePayBandGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePayBandGroup> items)
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

/// <summary>Functions of adaptive pay band group rules.</summary>
public static class AdaptivePayBandGroupRules
{
    public static AdaptivePayBandGroupValidator DefaultAdaptivePayBandGroupValidator()
    {
        return new AdaptivePayBandGroupValidator(
            new List<IAdaptivePayBandGroupRule>
            {
                new AdaptivePayBandGroupThresholdRangeRule(6.1, 890.8),
                new AdaptivePayBandGroupApprovedGuardRule(16),
                new AdaptivePayBandGroupTimezoneLengthRule(32),
                new AdaptivePayBandGroupRegionLengthRule(120),
            });
    }
}
