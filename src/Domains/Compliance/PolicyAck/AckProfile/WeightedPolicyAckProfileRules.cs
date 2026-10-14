namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted policy ack profile must pass. Check returns its problems, or none.</summary>
public interface IWeightedPolicyAckProfileRule
{
    List<string> Check(WeightedPolicyAckProfile item);
}

public sealed record WeightedPolicyAckProfileAllowedCodeRule(List<string> Allowed) : IWeightedPolicyAckProfileRule
{
    public List<string> Check(WeightedPolicyAckProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckProfileCountryLengthRule(int MaxLength) : IWeightedPolicyAckProfileRule
{
    public List<string> Check(WeightedPolicyAckProfile item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckProfileRequiredCodeRule(string Message) : IWeightedPolicyAckProfileRule
{
    public List<string> Check(WeightedPolicyAckProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted policy ack profile.</summary>
public sealed record WeightedPolicyAckProfileValidator(List<IWeightedPolicyAckProfileRule> Rules)
{
    public List<string> Validate(WeightedPolicyAckProfile item)
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

    public void RequireValid(WeightedPolicyAckProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPolicyAckProfile> items)
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

/// <summary>Functions of weighted policy ack profile rules.</summary>
public static class WeightedPolicyAckProfileRules
{
    public static WeightedPolicyAckProfileValidator DefaultWeightedPolicyAckProfileValidator()
    {
        return new WeightedPolicyAckProfileValidator(
            new List<IWeightedPolicyAckProfileRule>
            {
                new WeightedPolicyAckProfileAllowedCodeRule(new List<string> { "global", "pending", "batched" }),
                new WeightedPolicyAckProfileCountryLengthRule(3600),
                new WeightedPolicyAckProfileRequiredCodeRule("weighted policy ack profile needs an entry"),
            });
    }
}
