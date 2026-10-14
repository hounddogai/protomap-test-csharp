namespace Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>A rule a adaptive cost model policy must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCostModelPolicyRule
{
    List<string> Check(AdaptiveCostModelPolicy item);
}

public sealed record AdaptiveCostModelPolicyRequiredCodeRule(string Message) : IAdaptiveCostModelPolicyRule
{
    public List<string> Check(AdaptiveCostModelPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCostModelPolicyRequiredExternalRefRule(string Message) : IAdaptiveCostModelPolicyRule
{
    public List<string> Check(AdaptiveCostModelPolicy item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCostModelPolicyMaximumRangeRule(double Min, double Max) : IAdaptiveCostModelPolicyRule
{
    public List<string> Check(AdaptiveCostModelPolicy item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCostModelPolicyRequiredLabelRule(string Message) : IAdaptiveCostModelPolicyRule
{
    public List<string> Check(AdaptiveCostModelPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive cost model policy.</summary>
public sealed record AdaptiveCostModelPolicyValidator(List<IAdaptiveCostModelPolicyRule> Rules)
{
    public List<string> Validate(AdaptiveCostModelPolicy item)
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

    public void RequireValid(AdaptiveCostModelPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCostModelPolicy> items)
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

/// <summary>Functions of adaptive cost model policy rules.</summary>
public static class AdaptiveCostModelPolicyRules
{
    public static AdaptiveCostModelPolicyValidator DefaultAdaptiveCostModelPolicyValidator()
    {
        return new AdaptiveCostModelPolicyValidator(
            new List<IAdaptiveCostModelPolicyRule>
            {
                new AdaptiveCostModelPolicyRequiredCodeRule("adaptive cost model policy needs an entry"),
                new AdaptiveCostModelPolicyRequiredExternalRefRule("adaptive cost model policy needs a value"),
                new AdaptiveCostModelPolicyMaximumRangeRule(5.2, 3311.2),
                new AdaptiveCostModelPolicyRequiredLabelRule("adaptive cost model policy needs a value"),
            });
    }
}
