namespace Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>A rule a bonus pool window must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolWindowRule
{
    List<string> Check(BonusPoolWindow item);
}

public sealed record BonusPoolWindowUpdatedAtRangeRule(int Min, int Max) : IBonusPoolWindowRule
{
    public List<string> Check(BonusPoolWindow item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolWindowRequiredContactEmailRule(string Message) : IBonusPoolWindowRule
{
    public List<string> Check(BonusPoolWindow item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolWindowWeightRangeRule(double Min, double Max) : IBonusPoolWindowRule
{
    public List<string> Check(BonusPoolWindow item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolWindowContactEmailLengthRule(int MaxLength) : IBonusPoolWindowRule
{
    public List<string> Check(BonusPoolWindow item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolWindowCodePrefixRule(string Prefix) : IBonusPoolWindowRule
{
    public List<string> Check(BonusPoolWindow item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool window.</summary>
public sealed record BonusPoolWindowValidator(List<IBonusPoolWindowRule> Rules)
{
    public List<string> Validate(BonusPoolWindow item)
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

    public void RequireValid(BonusPoolWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolWindow> items)
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

/// <summary>Functions of bonus pool window rules.</summary>
public static class BonusPoolWindowRules
{
    public static BonusPoolWindowValidator DefaultBonusPoolWindowValidator()
    {
        return new BonusPoolWindowValidator(
            new List<IBonusPoolWindowRule>
            {
                new BonusPoolWindowUpdatedAtRangeRule(32, 120),
                new BonusPoolWindowRequiredContactEmailRule("bonus pool window needs an entry"),
                new BonusPoolWindowWeightRangeRule(3.1, 1641.1),
                new BonusPoolWindowContactEmailLengthRule(32),
                new BonusPoolWindowCodePrefixRule("OPS"),
            });
    }
}
