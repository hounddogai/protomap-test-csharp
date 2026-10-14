namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a cost model tier must pass. Check returns its problems, or none.</summary>
public interface ICostModelTierRule
{
    List<string> Check(CostModelTier item);
}

public sealed record CostModelTierApprovedGuardRule(double Limit) : ICostModelTierRule
{
    public List<string> Check(CostModelTier item)
    {
        if (item.Approved && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record CostModelTierAllowedNameRule(List<string> Allowed) : ICostModelTierRule
{
    public List<string> Check(CostModelTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record CostModelTierChannelLengthRule(int MaxLength) : ICostModelTierRule
{
    public List<string> Check(CostModelTier item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelTierCodePrefixRule(string Prefix) : ICostModelTierRule
{
    public List<string> Check(CostModelTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelTierRequiredNameRule(string Message) : ICostModelTierRule
{
    public List<string> Check(CostModelTier item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CostModelTierNameLengthRule(int MaxLength) : ICostModelTierRule
{
    public List<string> Check(CostModelTier item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model tier.</summary>
public sealed record CostModelTierValidator(List<ICostModelTierRule> Rules)
{
    public List<string> Validate(CostModelTier item)
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

    public void RequireValid(CostModelTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelTier> items)
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

/// <summary>Functions of cost model tier rules.</summary>
public static class CostModelTierRules
{
    public static CostModelTierValidator DefaultCostModelTierValidator()
    {
        return new CostModelTierValidator(
            new List<ICostModelTierRule>
            {
                new CostModelTierApprovedGuardRule(1.5),
                new CostModelTierAllowedNameRule(new List<string> { "strict" }),
                new CostModelTierChannelLengthRule(3600),
                new CostModelTierCodePrefixRule("OPS"),
                new CostModelTierRequiredNameRule("cost model tier needs an entry"),
                new CostModelTierNameLengthRule(32),
            });
    }
}
