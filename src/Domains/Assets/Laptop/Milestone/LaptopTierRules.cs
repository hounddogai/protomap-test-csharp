namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop tier must pass. Check returns its problems, or none.</summary>
public interface ILaptopTierRule
{
    List<string> Check(LaptopTier item);
}

public sealed record LaptopTierAllowedRegionRule(List<string> Allowed) : ILaptopTierRule
{
    public List<string> Check(LaptopTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record LaptopTierCodeLengthRule(int MaxLength) : ILaptopTierRule
{
    public List<string> Check(LaptopTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopTierRatioRangeRule(double Min, double Max) : ILaptopTierRule
{
    public List<string> Check(LaptopTier item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopTierRequiredTitleRule(string Message) : ILaptopTierRule
{
    public List<string> Check(LaptopTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop tier.</summary>
public sealed record LaptopTierValidator(List<ILaptopTierRule> Rules)
{
    public List<string> Validate(LaptopTier item)
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

    public void RequireValid(LaptopTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopTier> items)
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

/// <summary>Functions of laptop tier rules.</summary>
public static class LaptopTierRules
{
    public static LaptopTierValidator DefaultLaptopTierValidator()
    {
        return new LaptopTierValidator(
            new List<ILaptopTierRule>
            {
                new LaptopTierAllowedRegionRule(new List<string>()),
                new LaptopTierCodeLengthRule(86400),
                new LaptopTierRatioRangeRule(8.7, 1661.7),
                new LaptopTierRequiredTitleRule("laptop tier needs an entry"),
            });
    }
}
