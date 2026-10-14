namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment mapping must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentMappingRule
{
    List<string> Check(AdjustmentMapping item);
}

public sealed record AdjustmentMappingAllowedTitleRule(List<string> Allowed) : IAdjustmentMappingRule
{
    public List<string> Check(AdjustmentMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMappingMinimumRangeRule(double Min, double Max) : IAdjustmentMappingRule
{
    public List<string> Check(AdjustmentMapping item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMappingRequiredTitleRule(string Message) : IAdjustmentMappingRule
{
    public List<string> Check(AdjustmentMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentMappingTenureMonthsRangeRule(int Min, int Max) : IAdjustmentMappingRule
{
    public List<string> Check(AdjustmentMapping item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adjustment mapping.</summary>
public sealed record AdjustmentMappingValidator(List<IAdjustmentMappingRule> Rules)
{
    public List<string> Validate(AdjustmentMapping item)
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

    public void RequireValid(AdjustmentMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentMapping> items)
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

/// <summary>Functions of adjustment mapping rules.</summary>
public static class AdjustmentMappingRules
{
    public static AdjustmentMappingValidator DefaultAdjustmentMappingValidator()
    {
        return new AdjustmentMappingValidator(
            new List<IAdjustmentMappingRule>
            {
                new AdjustmentMappingAllowedTitleRule(new List<string> { "legacy", "adaptive", "weighted" }),
                new AdjustmentMappingMinimumRangeRule(2.9, 2700.3),
                new AdjustmentMappingRequiredTitleRule("adjustment mapping needs a value"),
                new AdjustmentMappingTenureMonthsRangeRule(16, 64),
            });
    }
}
