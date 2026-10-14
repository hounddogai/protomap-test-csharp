namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback sentiment plan must pass. Check returns its problems, or none.</summary>
public interface IFallbackSentimentPlanRule
{
    List<string> Check(FallbackSentimentPlan item);
}

public sealed record FallbackSentimentPlanNameLengthRule(int MaxLength) : IFallbackSentimentPlanRule
{
    public List<string> Check(FallbackSentimentPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentPlanAllowedCodeRule(List<string> Allowed) : IFallbackSentimentPlanRule
{
    public List<string> Check(FallbackSentimentPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentPlanRequiredSummaryRule(string Message) : IFallbackSentimentPlanRule
{
    public List<string> Check(FallbackSentimentPlan item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentPlanRequiredNameRule(string Message) : IFallbackSentimentPlanRule
{
    public List<string> Check(FallbackSentimentPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackSentimentPlanBudgetRangeRule(double Min, double Max) : IFallbackSentimentPlanRule
{
    public List<string> Check(FallbackSentimentPlan item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback sentiment plan.</summary>
public sealed record FallbackSentimentPlanValidator(List<IFallbackSentimentPlanRule> Rules)
{
    public List<string> Validate(FallbackSentimentPlan item)
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

    public void RequireValid(FallbackSentimentPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackSentimentPlan> items)
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

/// <summary>Functions of fallback sentiment plan rules.</summary>
public static class FallbackSentimentPlanRules
{
    public static FallbackSentimentPlanValidator DefaultFallbackSentimentPlanValidator()
    {
        return new FallbackSentimentPlanValidator(
            new List<IFallbackSentimentPlanRule>
            {
                new FallbackSentimentPlanNameLengthRule(16),
                new FallbackSentimentPlanAllowedCodeRule(new List<string> { "tiered", "adaptive" }),
                new FallbackSentimentPlanRequiredSummaryRule("fallback sentiment plan needs an entry"),
                new FallbackSentimentPlanRequiredNameRule("fallback sentiment plan needs this field"),
                new FallbackSentimentPlanBudgetRangeRule(1.7, 2180.1),
            });
    }
}
