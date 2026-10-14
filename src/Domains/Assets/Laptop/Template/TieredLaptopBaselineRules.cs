namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered laptop baseline must pass. Check returns its problems, or none.</summary>
public interface ITieredLaptopBaselineRule
{
    List<string> Check(TieredLaptopBaseline item);
}

public sealed record TieredLaptopBaselineAllowedTitleRule(List<string> Allowed) : ITieredLaptopBaselineRule
{
    public List<string> Check(TieredLaptopBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopBaselineTagsLimitRule(int MaxCount, List<string> Banned) : ITieredLaptopBaselineRule
{
    public List<string> Check(TieredLaptopBaseline item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record TieredLaptopBaselineRequiredTitleRule(string Message) : ITieredLaptopBaselineRule
{
    public List<string> Check(TieredLaptopBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopBaselineExpiresAtRangeRule(int Min, int Max) : ITieredLaptopBaselineRule
{
    public List<string> Check(TieredLaptopBaseline item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredLaptopBaselineUnitLengthRule(int MaxLength) : ITieredLaptopBaselineRule
{
    public List<string> Check(TieredLaptopBaseline item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered laptop baseline.</summary>
public sealed record TieredLaptopBaselineValidator(List<ITieredLaptopBaselineRule> Rules)
{
    public List<string> Validate(TieredLaptopBaseline item)
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

    public void RequireValid(TieredLaptopBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLaptopBaseline> items)
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

/// <summary>Functions of tiered laptop baseline rules.</summary>
public static class TieredLaptopBaselineRules
{
    public static TieredLaptopBaselineValidator DefaultTieredLaptopBaselineValidator()
    {
        return new TieredLaptopBaselineValidator(
            new List<ITieredLaptopBaselineRule>
            {
                new TieredLaptopBaselineAllowedTitleRule(new List<string>()),
                new TieredLaptopBaselineTagsLimitRule(1000, new List<string> { "capped", "tiered" }),
                new TieredLaptopBaselineRequiredTitleRule("tiered laptop baseline needs an entry"),
                new TieredLaptopBaselineExpiresAtRangeRule(255, 1000),
                new TieredLaptopBaselineUnitLengthRule(1000),
            });
    }
}
