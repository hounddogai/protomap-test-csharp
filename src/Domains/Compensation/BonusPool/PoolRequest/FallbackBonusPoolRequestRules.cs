namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>A rule a fallback bonus pool request must pass. Check returns its problems, or none.</summary>
public interface IFallbackBonusPoolRequestRule
{
    List<string> Check(FallbackBonusPoolRequest item);
}

public sealed record FallbackBonusPoolRequestCodeLengthRule(int MaxLength) : IFallbackBonusPoolRequestRule
{
    public List<string> Check(FallbackBonusPoolRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolRequestAttemptsRangeRule(int Min, int Max) : IFallbackBonusPoolRequestRule
{
    public List<string> Check(FallbackBonusPoolRequest item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackBonusPoolRequestCodePrefixRule(string Prefix) : IFallbackBonusPoolRequestRule
{
    public List<string> Check(FallbackBonusPoolRequest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback bonus pool request.</summary>
public sealed record FallbackBonusPoolRequestValidator(List<IFallbackBonusPoolRequestRule> Rules)
{
    public List<string> Validate(FallbackBonusPoolRequest item)
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

    public void RequireValid(FallbackBonusPoolRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackBonusPoolRequest> items)
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

/// <summary>Functions of fallback bonus pool request rules.</summary>
public static class FallbackBonusPoolRequestRules
{
    public static FallbackBonusPoolRequestValidator DefaultFallbackBonusPoolRequestValidator()
    {
        return new FallbackBonusPoolRequestValidator(
            new List<IFallbackBonusPoolRequestRule>
            {
                new FallbackBonusPoolRequestCodeLengthRule(120),
                new FallbackBonusPoolRequestAttemptsRangeRule(1000, 3600),
                new FallbackBonusPoolRequestCodePrefixRule("OPS"),
            });
    }
}
