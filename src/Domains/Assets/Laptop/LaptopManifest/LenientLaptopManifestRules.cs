namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

/// <summary>A rule a lenient laptop manifest must pass. Check returns its problems, or none.</summary>
public interface ILenientLaptopManifestRule
{
    List<string> Check(LenientLaptopManifest item);
}

public sealed record LenientLaptopManifestPhoneLengthRule(int MaxLength) : ILenientLaptopManifestRule
{
    public List<string> Check(LenientLaptopManifest item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopManifestCodePrefixRule(string Prefix) : ILenientLaptopManifestRule
{
    public List<string> Check(LenientLaptopManifest item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopManifestScoreRangeRule(double Min, double Max) : ILenientLaptopManifestRule
{
    public List<string> Check(LenientLaptopManifest item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient laptop manifest.</summary>
public sealed record LenientLaptopManifestValidator(List<ILenientLaptopManifestRule> Rules)
{
    public List<string> Validate(LenientLaptopManifest item)
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

    public void RequireValid(LenientLaptopManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLaptopManifest> items)
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

/// <summary>Functions of lenient laptop manifest rules.</summary>
public static class LenientLaptopManifestRules
{
    public static LenientLaptopManifestValidator DefaultLenientLaptopManifestValidator()
    {
        return new LenientLaptopManifestValidator(
            new List<ILenientLaptopManifestRule>
            {
                new LenientLaptopManifestPhoneLengthRule(120),
                new LenientLaptopManifestCodePrefixRule("ACM"),
                new LenientLaptopManifestScoreRangeRule(7.5, 3913.3),
            });
    }
}
