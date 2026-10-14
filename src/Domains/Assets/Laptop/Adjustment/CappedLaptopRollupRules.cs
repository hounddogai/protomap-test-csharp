namespace Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>A rule a capped laptop rollup must pass. Check returns its problems, or none.</summary>
public interface ICappedLaptopRollupRule
{
    List<string> Check(CappedLaptopRollup item);
}

public sealed record CappedLaptopRollupRequiredTitleRule(string Message) : ICappedLaptopRollupRule
{
    public List<string> Check(CappedLaptopRollup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopRollupBadgeLengthRule(int MaxLength) : ICappedLaptopRollupRule
{
    public List<string> Check(CappedLaptopRollup item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopRollupWeightRangeRule(double Min, double Max) : ICappedLaptopRollupRule
{
    public List<string> Check(CappedLaptopRollup item)
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

public sealed record CappedLaptopRollupRequiredBadgeRule(string Message) : ICappedLaptopRollupRule
{
    public List<string> Check(CappedLaptopRollup item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped laptop rollup.</summary>
public sealed record CappedLaptopRollupValidator(List<ICappedLaptopRollupRule> Rules)
{
    public List<string> Validate(CappedLaptopRollup item)
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

    public void RequireValid(CappedLaptopRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedLaptopRollup> items)
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

/// <summary>Functions of capped laptop rollup rules.</summary>
public static class CappedLaptopRollupRules
{
    public static CappedLaptopRollupValidator DefaultCappedLaptopRollupValidator()
    {
        return new CappedLaptopRollupValidator(
            new List<ICappedLaptopRollupRule>
            {
                new CappedLaptopRollupRequiredTitleRule("capped laptop rollup needs this field"),
                new CappedLaptopRollupBadgeLengthRule(16),
                new CappedLaptopRollupWeightRangeRule(3.3, 4768.8),
                new CappedLaptopRollupRequiredBadgeRule("capped laptop rollup needs this field"),
            });
    }
}
