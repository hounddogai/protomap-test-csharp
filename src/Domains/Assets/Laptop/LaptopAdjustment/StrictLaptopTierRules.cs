namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>A rule a strict laptop tier must pass. Check returns its problems, or none.</summary>
public interface IStrictLaptopTierRule
{
    List<string> Check(StrictLaptopTier item);
}

public sealed record StrictLaptopTierRequiredTitleRule(string Message) : IStrictLaptopTierRule
{
    public List<string> Check(StrictLaptopTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopTierCodeLengthRule(int MaxLength) : IStrictLaptopTierRule
{
    public List<string> Check(StrictLaptopTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopTierRetriesRangeRule(int Min, int Max) : IStrictLaptopTierRule
{
    public List<string> Check(StrictLaptopTier item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopTierStartsAtRangeRule(int Min, int Max) : IStrictLaptopTierRule
{
    public List<string> Check(StrictLaptopTier item)
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

public sealed record StrictLaptopTierGradeLengthRule(int MaxLength) : IStrictLaptopTierRule
{
    public List<string> Check(StrictLaptopTier item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictLaptopTierRequiredGradeRule(string Message) : IStrictLaptopTierRule
{
    public List<string> Check(StrictLaptopTier item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict laptop tier.</summary>
public sealed record StrictLaptopTierValidator(List<IStrictLaptopTierRule> Rules)
{
    public List<string> Validate(StrictLaptopTier item)
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

    public void RequireValid(StrictLaptopTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictLaptopTier> items)
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

/// <summary>Functions of strict laptop tier rules.</summary>
public static class StrictLaptopTierRules
{
    public static StrictLaptopTierValidator DefaultStrictLaptopTierValidator()
    {
        return new StrictLaptopTierValidator(
            new List<IStrictLaptopTierRule>
            {
                new StrictLaptopTierRequiredTitleRule("strict laptop tier needs an entry"),
                new StrictLaptopTierCodeLengthRule(255),
                new StrictLaptopTierRetriesRangeRule(80, 3600),
                new StrictLaptopTierStartsAtRangeRule(80, 120),
                new StrictLaptopTierGradeLengthRule(86400),
                new StrictLaptopTierRequiredGradeRule("strict laptop tier needs an entry"),
            });
    }
}
