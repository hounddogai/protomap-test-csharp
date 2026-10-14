namespace Acme.Workforce.Compensation.BonusPool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback bonus pool profile must pass. Check returns its problems, or none.</summary>
public interface IFallbackBonusPoolProfileRule
{
    List<string> Check(FallbackBonusPoolProfile item);
}

public sealed record FallbackBonusPoolProfileRequiredCodeRule(string Message) : IFallbackBonusPoolProfileRule
{
    public List<string> Check(FallbackBonusPoolProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolProfileCodeLengthRule(int MaxLength) : IFallbackBonusPoolProfileRule
{
    public List<string> Check(FallbackBonusPoolProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolProfileAllowedTitleRule(List<string> Allowed) : IFallbackBonusPoolProfileRule
{
    public List<string> Check(FallbackBonusPoolProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolProfileBatchSizeRangeRule(int Min, int Max) : IFallbackBonusPoolProfileRule
{
    public List<string> Check(FallbackBonusPoolProfile item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolProfileRequiredTitleRule(string Message) : IFallbackBonusPoolProfileRule
{
    public List<string> Check(FallbackBonusPoolProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback bonus pool profile.</summary>
public sealed record FallbackBonusPoolProfileValidator(List<IFallbackBonusPoolProfileRule> Rules)
{
    public List<string> Validate(FallbackBonusPoolProfile item)
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

    public void RequireValid(FallbackBonusPoolProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBonusPoolProfile> items)
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

/// <summary>Functions of fallback bonus pool profile rules.</summary>
public static class FallbackBonusPoolProfileRules
{
    public static FallbackBonusPoolProfileValidator DefaultFallbackBonusPoolProfileValidator()
    {
        return new FallbackBonusPoolProfileValidator(
            new List<IFallbackBonusPoolProfileRule>
            {
                new FallbackBonusPoolProfileRequiredCodeRule("fallback bonus pool profile needs a value"),
                new FallbackBonusPoolProfileCodeLengthRule(1000),
                new FallbackBonusPoolProfileAllowedTitleRule(new List<string> { "pending" }),
                new FallbackBonusPoolProfileBatchSizeRangeRule(80, 1000),
                new FallbackBonusPoolProfileRequiredTitleRule("fallback bonus pool profile needs a value"),
            });
    }
}
