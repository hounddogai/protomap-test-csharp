namespace Acme.Workforce.Orgdesign.Position.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position tier must pass. Check returns its problems, or none.</summary>
public interface IPositionTierRule
{
    List<string> Check(PositionTier item);
}

public sealed record PositionTierTitleLengthRule(int MaxLength) : IPositionTierRule
{
    public List<string> Check(PositionTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionTierRequiredCodeRule(string Message) : IPositionTierRule
{
    public List<string> Check(PositionTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionTierStartsAtRangeRule(int Min, int Max) : IPositionTierRule
{
    public List<string> Check(PositionTier item)
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

public sealed record PositionTierAllowedCurrencyRule(List<string> Allowed) : IPositionTierRule
{
    public List<string> Check(PositionTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PositionTierRequiredCurrencyRule(string Message) : IPositionTierRule
{
    public List<string> Check(PositionTier item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position tier.</summary>
public sealed record PositionTierValidator(List<IPositionTierRule> Rules)
{
    public List<string> Validate(PositionTier item)
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

    public void RequireValid(PositionTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionTier> items)
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

/// <summary>Functions of position tier rules.</summary>
public static class PositionTierRules
{
    public static PositionTierValidator DefaultPositionTierValidator()
    {
        return new PositionTierValidator(
            new List<IPositionTierRule>
            {
                new PositionTierTitleLengthRule(120),
                new PositionTierRequiredCodeRule("position tier needs an entry"),
                new PositionTierStartsAtRangeRule(80, 120),
                new PositionTierAllowedCurrencyRule(new List<string>()),
                new PositionTierRequiredCurrencyRule("position tier needs an entry"),
            });
    }
}
