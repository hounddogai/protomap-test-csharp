namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

/// <summary>A rule a license seat rule must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatRuleRule
{
    List<string> Check(LicenseSeatRule item);
}

public sealed record LicenseSeatRuleCostCenterLengthRule(int MaxLength) : ILicenseSeatRuleRule
{
    public List<string> Check(LicenseSeatRule item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatRuleCodePrefixRule(string Prefix) : ILicenseSeatRuleRule
{
    public List<string> Check(LicenseSeatRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatRuleRequiredCodeRule(string Message) : ILicenseSeatRuleRule
{
    public List<string> Check(LicenseSeatRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatRuleCodeLengthRule(int MaxLength) : ILicenseSeatRuleRule
{
    public List<string> Check(LicenseSeatRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatRuleRequiredCostCenterRule(string Message) : ILicenseSeatRuleRule
{
    public List<string> Check(LicenseSeatRule item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat rule.</summary>
public sealed record LicenseSeatRuleValidator(List<ILicenseSeatRuleRule> Rules)
{
    public List<string> Validate(LicenseSeatRule item)
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

    public void RequireValid(LicenseSeatRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatRule> items)
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

/// <summary>Functions of license seat rule rules.</summary>
public static class LicenseSeatRuleRules
{
    public static LicenseSeatRuleValidator DefaultLicenseSeatRuleValidator()
    {
        return new LicenseSeatRuleValidator(
            new List<ILicenseSeatRuleRule>
            {
                new LicenseSeatRuleCostCenterLengthRule(255),
                new LicenseSeatRuleCodePrefixRule("ACM"),
                new LicenseSeatRuleRequiredCodeRule("license seat rule needs this field"),
                new LicenseSeatRuleCodeLengthRule(3600),
                new LicenseSeatRuleRequiredCostCenterRule("license seat rule needs a value"),
            });
    }
}
