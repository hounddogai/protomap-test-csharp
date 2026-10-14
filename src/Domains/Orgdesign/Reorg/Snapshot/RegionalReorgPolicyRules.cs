namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>A rule a regional reorg policy must pass. Check returns its problems, or none.</summary>
public interface IRegionalReorgPolicyRule
{
    List<string> Check(RegionalReorgPolicy item);
}

public sealed record RegionalReorgPolicyRequiredNameRule(string Message) : IRegionalReorgPolicyRule
{
    public List<string> Check(RegionalReorgPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgPolicyRequiredCostCenterRule(string Message) : IRegionalReorgPolicyRule
{
    public List<string> Check(RegionalReorgPolicy item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalReorgPolicyRequiredGradeRule(string Message) : IRegionalReorgPolicyRule
{
    public List<string> Check(RegionalReorgPolicy item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional reorg policy.</summary>
public sealed record RegionalReorgPolicyValidator(List<IRegionalReorgPolicyRule> Rules)
{
    public List<string> Validate(RegionalReorgPolicy item)
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

    public void RequireValid(RegionalReorgPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalReorgPolicy> items)
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

/// <summary>Functions of regional reorg policy rules.</summary>
public static class RegionalReorgPolicyRules
{
    public static RegionalReorgPolicyValidator DefaultRegionalReorgPolicyValidator()
    {
        return new RegionalReorgPolicyValidator(
            new List<IRegionalReorgPolicyRule>
            {
                new RegionalReorgPolicyRequiredNameRule("regional reorg policy needs this field"),
                new RegionalReorgPolicyRequiredCostCenterRule("regional reorg policy needs a value"),
                new RegionalReorgPolicyRequiredGradeRule("regional reorg policy needs an entry"),
            });
    }
}
