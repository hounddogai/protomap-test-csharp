namespace Acme.Workforce.Compliance.PolicyAck.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered policy ack variance must pass. Check returns its problems, or none.</summary>
public interface ITieredPolicyAckVarianceRule
{
    List<string> Check(TieredPolicyAckVariance item);
}

public sealed record TieredPolicyAckVarianceAllowedCostCenterRule(List<string> Allowed) : ITieredPolicyAckVarianceRule
{
    public List<string> Check(TieredPolicyAckVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is too long" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckVarianceCodeLengthRule(int MaxLength) : ITieredPolicyAckVarianceRule
{
    public List<string> Check(TieredPolicyAckVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckVarianceRequiredSourceRule(string Message) : ITieredPolicyAckVarianceRule
{
    public List<string> Check(TieredPolicyAckVariance item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPolicyAckVarianceCodePrefixRule(string Prefix) : ITieredPolicyAckVarianceRule
{
    public List<string> Check(TieredPolicyAckVariance item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered policy ack variance.</summary>
public sealed record TieredPolicyAckVarianceValidator(List<ITieredPolicyAckVarianceRule> Rules)
{
    public List<string> Validate(TieredPolicyAckVariance item)
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

    public void RequireValid(TieredPolicyAckVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPolicyAckVariance> items)
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

/// <summary>Functions of tiered policy ack variance rules.</summary>
public static class TieredPolicyAckVarianceRules
{
    public static TieredPolicyAckVarianceValidator DefaultTieredPolicyAckVarianceValidator()
    {
        return new TieredPolicyAckVarianceValidator(
            new List<ITieredPolicyAckVarianceRule>
            {
                new TieredPolicyAckVarianceAllowedCostCenterRule(new List<string> { "lenient" }),
                new TieredPolicyAckVarianceCodeLengthRule(32),
                new TieredPolicyAckVarianceRequiredSourceRule("tiered policy ack variance needs a value"),
                new TieredPolicyAckVarianceCodePrefixRule("Q"),
            });
    }
}
