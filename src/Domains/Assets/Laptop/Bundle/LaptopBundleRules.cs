namespace Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>A rule a laptop bundle must pass. Check returns its problems, or none.</summary>
public interface ILaptopBundleRule
{
    List<string> Check(LaptopBundle item);
}

public sealed record LaptopBundleCodeLengthRule(int MaxLength) : ILaptopBundleRule
{
    public List<string> Check(LaptopBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBundleCodePrefixRule(string Prefix) : ILaptopBundleRule
{
    public List<string> Check(LaptopBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBundleRequiredBadgeRule(string Message) : ILaptopBundleRule
{
    public List<string> Check(LaptopBundle item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopBundleExternalRefLengthRule(int MaxLength) : ILaptopBundleRule
{
    public List<string> Check(LaptopBundle item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopBundleRequiredCodeRule(string Message) : ILaptopBundleRule
{
    public List<string> Check(LaptopBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop bundle.</summary>
public sealed record LaptopBundleValidator(List<ILaptopBundleRule> Rules)
{
    public List<string> Validate(LaptopBundle item)
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

    public void RequireValid(LaptopBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopBundle> items)
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

/// <summary>Functions of laptop bundle rules.</summary>
public static class LaptopBundleRules
{
    public static LaptopBundleValidator DefaultLaptopBundleValidator()
    {
        return new LaptopBundleValidator(
            new List<ILaptopBundleRule>
            {
                new LaptopBundleCodeLengthRule(120),
                new LaptopBundleCodePrefixRule("OPS"),
                new LaptopBundleRequiredBadgeRule("laptop bundle needs an entry"),
                new LaptopBundleExternalRefLengthRule(64),
                new LaptopBundleRequiredCodeRule("laptop bundle needs an entry"),
            });
    }
}
