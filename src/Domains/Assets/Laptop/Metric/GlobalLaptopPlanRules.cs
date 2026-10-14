namespace Acme.Workforce.Assets.Laptop.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global laptop plan must pass. Check returns its problems, or none.</summary>
public interface IGlobalLaptopPlanRule
{
    List<string> Check(GlobalLaptopPlan item);
}

public sealed record GlobalLaptopPlanAllowedLabelRule(List<string> Allowed) : IGlobalLaptopPlanRule
{
    public List<string> Check(GlobalLaptopPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLaptopPlanChannelLengthRule(int MaxLength) : IGlobalLaptopPlanRule
{
    public List<string> Check(GlobalLaptopPlan item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLaptopPlanCodeLengthRule(int MaxLength) : IGlobalLaptopPlanRule
{
    public List<string> Check(GlobalLaptopPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLaptopPlanSegmentLengthRule(int MaxLength) : IGlobalLaptopPlanRule
{
    public List<string> Check(GlobalLaptopPlan item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLaptopPlanLabelLengthRule(int MaxLength) : IGlobalLaptopPlanRule
{
    public List<string> Check(GlobalLaptopPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global laptop plan.</summary>
public sealed record GlobalLaptopPlanValidator(List<IGlobalLaptopPlanRule> Rules)
{
    public List<string> Validate(GlobalLaptopPlan item)
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

    public void RequireValid(GlobalLaptopPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalLaptopPlan> items)
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

/// <summary>Functions of global laptop plan rules.</summary>
public static class GlobalLaptopPlanRules
{
    public static GlobalLaptopPlanValidator DefaultGlobalLaptopPlanValidator()
    {
        return new GlobalLaptopPlanValidator(
            new List<IGlobalLaptopPlanRule>
            {
                new GlobalLaptopPlanAllowedLabelRule(new List<string> { "manual", "fallback" }),
                new GlobalLaptopPlanChannelLengthRule(120),
                new GlobalLaptopPlanCodeLengthRule(64),
                new GlobalLaptopPlanSegmentLengthRule(255),
                new GlobalLaptopPlanLabelLengthRule(255),
            });
    }
}
