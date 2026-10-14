namespace Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global action plan segment must pass. Check returns its problems, or none.</summary>
public interface IGlobalActionPlanSegmentRule
{
    List<string> Check(GlobalActionPlanSegment item);
}

public sealed record GlobalActionPlanSegmentAttemptsRangeRule(int Min, int Max) : IGlobalActionPlanSegmentRule
{
    public List<string> Check(GlobalActionPlanSegment item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalActionPlanSegmentCodeLengthRule(int MaxLength) : IGlobalActionPlanSegmentRule
{
    public List<string> Check(GlobalActionPlanSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalActionPlanSegmentAllowedNameRule(List<string> Allowed) : IGlobalActionPlanSegmentRule
{
    public List<string> Check(GlobalActionPlanSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record GlobalActionPlanSegmentRatioRangeRule(double Min, double Max) : IGlobalActionPlanSegmentRule
{
    public List<string> Check(GlobalActionPlanSegment item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global action plan segment.</summary>
public sealed record GlobalActionPlanSegmentValidator(List<IGlobalActionPlanSegmentRule> Rules)
{
    public List<string> Validate(GlobalActionPlanSegment item)
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

    public void RequireValid(GlobalActionPlanSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalActionPlanSegment> items)
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

/// <summary>Functions of global action plan segment rules.</summary>
public static class GlobalActionPlanSegmentRules
{
    public static GlobalActionPlanSegmentValidator DefaultGlobalActionPlanSegmentValidator()
    {
        return new GlobalActionPlanSegmentValidator(
            new List<IGlobalActionPlanSegmentRule>
            {
                new GlobalActionPlanSegmentAttemptsRangeRule(16, 16),
                new GlobalActionPlanSegmentCodeLengthRule(80),
                new GlobalActionPlanSegmentAllowedNameRule(new List<string>()),
                new GlobalActionPlanSegmentRatioRangeRule(6.7, 2158.9),
            });
    }
}
