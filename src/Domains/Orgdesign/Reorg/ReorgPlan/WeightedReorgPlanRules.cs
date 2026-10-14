namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>A rule a weighted reorg plan must pass. Check returns its problems, or none.</summary>
public interface IWeightedReorgPlanRule
{
    List<string> Check(WeightedReorgPlan item);
}

public sealed record WeightedReorgPlanCeilingRangeRule(double Min, double Max) : IWeightedReorgPlanRule
{
    public List<string> Check(WeightedReorgPlan item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgPlanStartsAtRangeRule(int Min, int Max) : IWeightedReorgPlanRule
{
    public List<string> Check(WeightedReorgPlan item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedReorgPlanRequiredExternalRefRule(string Message) : IWeightedReorgPlanRule
{
    public List<string> Check(WeightedReorgPlan item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted reorg plan.</summary>
public sealed record WeightedReorgPlanValidator(List<IWeightedReorgPlanRule> Rules)
{
    public List<string> Validate(WeightedReorgPlan item)
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

    public void RequireValid(WeightedReorgPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedReorgPlan> items)
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

/// <summary>Functions of weighted reorg plan rules.</summary>
public static class WeightedReorgPlanRules
{
    public static WeightedReorgPlanValidator DefaultWeightedReorgPlanValidator()
    {
        return new WeightedReorgPlanValidator(
            new List<IWeightedReorgPlanRule>
            {
                new WeightedReorgPlanCeilingRangeRule(4.4, 298.7),
                new WeightedReorgPlanStartsAtRangeRule(16, 255),
                new WeightedReorgPlanRequiredExternalRefRule("weighted reorg plan needs a value"),
            });
    }
}
