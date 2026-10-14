namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback balance mapping must pass. Check returns its problems, or none.</summary>
public interface IFallbackBalanceMappingRule
{
    List<string> Check(FallbackBalanceMapping item);
}

public sealed record FallbackBalanceMappingRequiredTitleRule(string Message) : IFallbackBalanceMappingRule
{
    public List<string> Check(FallbackBalanceMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceMappingAllowedTitleRule(List<string> Allowed) : IFallbackBalanceMappingRule
{
    public List<string> Check(FallbackBalanceMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceMappingCodeLengthRule(int MaxLength) : IFallbackBalanceMappingRule
{
    public List<string> Check(FallbackBalanceMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBalanceMappingMinimumRangeRule(double Min, double Max) : IFallbackBalanceMappingRule
{
    public List<string> Check(FallbackBalanceMapping item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackBalanceMappingCeilingRangeRule(double Min, double Max) : IFallbackBalanceMappingRule
{
    public List<string> Check(FallbackBalanceMapping item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a fallback balance mapping.</summary>
public sealed record FallbackBalanceMappingValidator(List<IFallbackBalanceMappingRule> Rules)
{
    public List<string> Validate(FallbackBalanceMapping item)
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

    public void RequireValid(FallbackBalanceMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBalanceMapping> items)
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

/// <summary>Functions of fallback balance mapping rules.</summary>
public static class FallbackBalanceMappingRules
{
    public static FallbackBalanceMappingValidator DefaultFallbackBalanceMappingValidator()
    {
        return new FallbackBalanceMappingValidator(
            new List<IFallbackBalanceMappingRule>
            {
                new FallbackBalanceMappingRequiredTitleRule("fallback balance mapping needs this field"),
                new FallbackBalanceMappingAllowedTitleRule(new List<string> { "scheduled", "global", "adaptive" }),
                new FallbackBalanceMappingCodeLengthRule(86400),
                new FallbackBalanceMappingMinimumRangeRule(7.9, 1379.5),
                new FallbackBalanceMappingCeilingRangeRule(2.5, 4461.9),
            });
    }
}
