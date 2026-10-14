namespace Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>A rule a adaptive position plan must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePositionPlanRule
{
    List<string> Check(AdaptivePositionPlan item);
}

public sealed record AdaptivePositionPlanRequiredCodeRule(string Message) : IAdaptivePositionPlanRule
{
    public List<string> Check(AdaptivePositionPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionPlanQuantityRangeRule(int Min, int Max) : IAdaptivePositionPlanRule
{
    public List<string> Check(AdaptivePositionPlan item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePositionPlanCodePrefixRule(string Prefix) : IAdaptivePositionPlanRule
{
    public List<string> Check(AdaptivePositionPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive position plan.</summary>
public sealed record AdaptivePositionPlanValidator(List<IAdaptivePositionPlanRule> Rules)
{
    public List<string> Validate(AdaptivePositionPlan item)
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

    public void RequireValid(AdaptivePositionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePositionPlan> items)
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

/// <summary>Functions of adaptive position plan rules.</summary>
public static class AdaptivePositionPlanRules
{
    public static AdaptivePositionPlanValidator DefaultAdaptivePositionPlanValidator()
    {
        return new AdaptivePositionPlanValidator(
            new List<IAdaptivePositionPlanRule>
            {
                new AdaptivePositionPlanRequiredCodeRule("adaptive position plan needs a value"),
                new AdaptivePositionPlanQuantityRangeRule(80, 86400),
                new AdaptivePositionPlanCodePrefixRule("X"),
            });
    }
}
