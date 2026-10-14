namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback balance threshold must pass. Check returns its problems, or none.</summary>
public interface IFallbackBalanceThresholdRule
{
    List<string> Check(FallbackBalanceThreshold item);
}

public sealed record FallbackBalanceThresholdAllowedCategoryRule(List<string> Allowed) : IFallbackBalanceThresholdRule
{
    public List<string> Check(FallbackBalanceThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceThresholdFteRangeRule(double Min, double Max) : IFallbackBalanceThresholdRule
{
    public List<string> Check(FallbackBalanceThreshold item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceThresholdRequiredCodeRule(string Message) : IFallbackBalanceThresholdRule
{
    public List<string> Check(FallbackBalanceThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback balance threshold.</summary>
public sealed record FallbackBalanceThresholdValidator(List<IFallbackBalanceThresholdRule> Rules)
{
    public List<string> Validate(FallbackBalanceThreshold item)
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

    public void RequireValid(FallbackBalanceThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBalanceThreshold> items)
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

/// <summary>Functions of fallback balance threshold rules.</summary>
public static class FallbackBalanceThresholdRules
{
    public static FallbackBalanceThresholdValidator DefaultFallbackBalanceThresholdValidator()
    {
        return new FallbackBalanceThresholdValidator(
            new List<IFallbackBalanceThresholdRule>
            {
                new FallbackBalanceThresholdAllowedCategoryRule(new List<string> { "lenient" }),
                new FallbackBalanceThresholdFteRangeRule(3.2, 3006.1),
                new FallbackBalanceThresholdRequiredCodeRule("fallback balance threshold needs an entry"),
            });
    }
}
