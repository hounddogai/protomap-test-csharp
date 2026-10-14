namespace Acme.Workforce.Assets.Laptop.Template;

/// <summary>A rule a capped laptop bundle must pass. Check returns its problems, or none.</summary>
public interface ICappedLaptopBundleRule
{
    List<string> Check(CappedLaptopBundle item);
}

public sealed record CappedLaptopBundleRequiredCodeRule(string Message) : ICappedLaptopBundleRule
{
    public List<string> Check(CappedLaptopBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopBundleRequiredChecksumRule(string Message) : ICappedLaptopBundleRule
{
    public List<string> Check(CappedLaptopBundle item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopBundleDiscountRangeRule(double Min, double Max) : ICappedLaptopBundleRule
{
    public List<string> Check(CappedLaptopBundle item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedLaptopBundleTagsLimitRule(int MaxCount, List<string> Banned) : ICappedLaptopBundleRule
{
    public List<string> Check(CappedLaptopBundle item)
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

public sealed record CappedLaptopBundleCategoryLengthRule(int MaxLength) : ICappedLaptopBundleRule
{
    public List<string> Check(CappedLaptopBundle item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped laptop bundle.</summary>
public sealed record CappedLaptopBundleValidator(List<ICappedLaptopBundleRule> Rules)
{
    public List<string> Validate(CappedLaptopBundle item)
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

    public void RequireValid(CappedLaptopBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedLaptopBundle> items)
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

/// <summary>Functions of capped laptop bundle rules.</summary>
public static class CappedLaptopBundleRules
{
    public static CappedLaptopBundleValidator DefaultCappedLaptopBundleValidator()
    {
        return new CappedLaptopBundleValidator(
            new List<ICappedLaptopBundleRule>
            {
                new CappedLaptopBundleRequiredCodeRule("capped laptop bundle needs this field"),
                new CappedLaptopBundleRequiredChecksumRule("capped laptop bundle needs a value"),
                new CappedLaptopBundleDiscountRangeRule(5.3, 2905.4),
                new CappedLaptopBundleTagsLimitRule(16, new List<string> { "primary" }),
                new CappedLaptopBundleCategoryLengthRule(64),
            });
    }
}
