namespace Acme.Workforce.Assets.Laptop.Review;

/// <summary>A rule a capped laptop manifest must pass. Check returns its problems, or none.</summary>
public interface ICappedLaptopManifestRule
{
    List<string> Check(CappedLaptopManifest item);
}

public sealed record CappedLaptopManifestRequiredReviewerRule(string Message) : ICappedLaptopManifestRule
{
    public List<string> Check(CappedLaptopManifest item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopManifestCodeLengthRule(int MaxLength) : ICappedLaptopManifestRule
{
    public List<string> Check(CappedLaptopManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLaptopManifestNameLengthRule(int MaxLength) : ICappedLaptopManifestRule
{
    public List<string> Check(CappedLaptopManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped laptop manifest.</summary>
public sealed record CappedLaptopManifestValidator(List<ICappedLaptopManifestRule> Rules)
{
    public List<string> Validate(CappedLaptopManifest item)
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

    public void RequireValid(CappedLaptopManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedLaptopManifest> items)
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

/// <summary>Functions of capped laptop manifest rules.</summary>
public static class CappedLaptopManifestRules
{
    public static CappedLaptopManifestValidator DefaultCappedLaptopManifestValidator()
    {
        return new CappedLaptopManifestValidator(
            new List<ICappedLaptopManifestRule>
            {
                new CappedLaptopManifestRequiredReviewerRule("capped laptop manifest needs an entry"),
                new CappedLaptopManifestCodeLengthRule(64),
                new CappedLaptopManifestNameLengthRule(80),
            });
    }
}
