namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>A rule a adaptive succession plan policy must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveSuccessionPlanPolicyRule
{
    List<string> Check(AdaptiveSuccessionPlanPolicy item);
}

public sealed record AdaptiveSuccessionPlanPolicyRequiredCodeRule(string Message) : IAdaptiveSuccessionPlanPolicyRule
{
    public List<string> Check(AdaptiveSuccessionPlanPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSuccessionPlanPolicyNameLengthRule(int MaxLength) : IAdaptiveSuccessionPlanPolicyRule
{
    public List<string> Check(AdaptiveSuccessionPlanPolicy item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSuccessionPlanPolicyFteRangeRule(double Min, double Max) : IAdaptiveSuccessionPlanPolicyRule
{
    public List<string> Check(AdaptiveSuccessionPlanPolicy item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive succession plan policy.</summary>
public sealed record AdaptiveSuccessionPlanPolicyValidator(List<IAdaptiveSuccessionPlanPolicyRule> Rules)
{
    public List<string> Validate(AdaptiveSuccessionPlanPolicy item)
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

    public void RequireValid(AdaptiveSuccessionPlanPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveSuccessionPlanPolicy> items)
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

/// <summary>Functions of adaptive succession plan policy rules.</summary>
public static class AdaptiveSuccessionPlanPolicyRules
{
    public static AdaptiveSuccessionPlanPolicyValidator DefaultAdaptiveSuccessionPlanPolicyValidator()
    {
        return new AdaptiveSuccessionPlanPolicyValidator(
            new List<IAdaptiveSuccessionPlanPolicyRule>
            {
                new AdaptiveSuccessionPlanPolicyRequiredCodeRule("adaptive succession plan policy needs a value"),
                new AdaptiveSuccessionPlanPolicyNameLengthRule(255),
                new AdaptiveSuccessionPlanPolicyFteRangeRule(3.2, 1944.1),
            });
    }
}
