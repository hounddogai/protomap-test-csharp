namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>A rule a adaptive policy ack claim must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePolicyAckClaimRule
{
    List<string> Check(AdaptivePolicyAckClaim item);
}

public sealed record AdaptivePolicyAckClaimSourceLengthRule(int MaxLength) : IAdaptivePolicyAckClaimRule
{
    public List<string> Check(AdaptivePolicyAckClaim item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyAckClaimTenureMonthsRangeRule(int Min, int Max) : IAdaptivePolicyAckClaimRule
{
    public List<string> Check(AdaptivePolicyAckClaim item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyAckClaimLimitRangeRule(int Min, int Max) : IAdaptivePolicyAckClaimRule
{
    public List<string> Check(AdaptivePolicyAckClaim item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive policy ack claim.</summary>
public sealed record AdaptivePolicyAckClaimValidator(List<IAdaptivePolicyAckClaimRule> Rules)
{
    public List<string> Validate(AdaptivePolicyAckClaim item)
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

    public void RequireValid(AdaptivePolicyAckClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePolicyAckClaim> items)
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

/// <summary>Functions of adaptive policy ack claim rules.</summary>
public static class AdaptivePolicyAckClaimRules
{
    public static AdaptivePolicyAckClaimValidator DefaultAdaptivePolicyAckClaimValidator()
    {
        return new AdaptivePolicyAckClaimValidator(
            new List<IAdaptivePolicyAckClaimRule>
            {
                new AdaptivePolicyAckClaimSourceLengthRule(1000),
                new AdaptivePolicyAckClaimTenureMonthsRangeRule(1000, 86400),
                new AdaptivePolicyAckClaimLimitRangeRule(3600, 86400),
            });
    }
}
