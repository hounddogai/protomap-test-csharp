namespace Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>A rule a recognition plan must pass. Check returns its problems, or none.</summary>
public interface IRecognitionPlanRule
{
    List<string> Check(RecognitionPlan item);
}

public sealed record RecognitionPlanCodePrefixRule(string Prefix) : IRecognitionPlanRule
{
    public List<string> Check(RecognitionPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPlanQuantityRangeRule(int Min, int Max) : IRecognitionPlanRule
{
    public List<string> Check(RecognitionPlan item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPlanRequiredRegionRule(string Message) : IRecognitionPlanRule
{
    public List<string> Check(RecognitionPlan item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition plan.</summary>
public sealed record RecognitionPlanValidator(List<IRecognitionPlanRule> Rules)
{
    public List<string> Validate(RecognitionPlan item)
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

    public void RequireValid(RecognitionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionPlan> items)
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

/// <summary>Functions of recognition plan rules.</summary>
public static class RecognitionPlanRules
{
    public static RecognitionPlanValidator DefaultRecognitionPlanValidator()
    {
        return new RecognitionPlanValidator(
            new List<IRecognitionPlanRule>
            {
                new RecognitionPlanCodePrefixRule("REF"),
                new RecognitionPlanQuantityRangeRule(64, 1000),
                new RecognitionPlanRequiredRegionRule("recognition plan needs this field"),
            });
    }
}
