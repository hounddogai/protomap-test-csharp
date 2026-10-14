namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict warranty manifest must pass. Check returns its problems, or none.</summary>
public interface IStrictWarrantyManifestRule
{
    List<string> Check(StrictWarrantyManifest item);
}

public sealed record StrictWarrantyManifestRatingRangeRule(double Min, double Max) : IStrictWarrantyManifestRule
{
    public List<string> Check(StrictWarrantyManifest item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyManifestAllowedChecksumRule(List<string> Allowed) : IStrictWarrantyManifestRule
{
    public List<string> Check(StrictWarrantyManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyManifestTitleLengthRule(int MaxLength) : IStrictWarrantyManifestRule
{
    public List<string> Check(StrictWarrantyManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictWarrantyManifestUpdatedAtRangeRule(int Min, int Max) : IStrictWarrantyManifestRule
{
    public List<string> Check(StrictWarrantyManifest item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictWarrantyManifestRequiredDescriptionRule(string Message) : IStrictWarrantyManifestRule
{
    public List<string> Check(StrictWarrantyManifest item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict warranty manifest.</summary>
public sealed record StrictWarrantyManifestValidator(List<IStrictWarrantyManifestRule> Rules)
{
    public List<string> Validate(StrictWarrantyManifest item)
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

    public void RequireValid(StrictWarrantyManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictWarrantyManifest> items)
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

/// <summary>Functions of strict warranty manifest rules.</summary>
public static class StrictWarrantyManifestRules
{
    public static StrictWarrantyManifestValidator DefaultStrictWarrantyManifestValidator()
    {
        return new StrictWarrantyManifestValidator(
            new List<IStrictWarrantyManifestRule>
            {
                new StrictWarrantyManifestRatingRangeRule(9.6, 927.6),
                new StrictWarrantyManifestAllowedChecksumRule(new List<string> { "primary" }),
                new StrictWarrantyManifestTitleLengthRule(64),
                new StrictWarrantyManifestUpdatedAtRangeRule(32, 255),
                new StrictWarrantyManifestRequiredDescriptionRule("strict warranty manifest needs this field"),
            });
    }
}
