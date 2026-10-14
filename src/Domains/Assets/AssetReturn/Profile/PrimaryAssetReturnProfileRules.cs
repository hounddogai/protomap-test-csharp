namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary asset return profile must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAssetReturnProfileRule
{
    List<string> Check(PrimaryAssetReturnProfile item);
}

public sealed record PrimaryAssetReturnProfileAllowedTitleRule(List<string> Allowed) : IPrimaryAssetReturnProfileRule
{
    public List<string> Check(PrimaryAssetReturnProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAssetReturnProfileTitleLengthRule(int MaxLength) : IPrimaryAssetReturnProfileRule
{
    public List<string> Check(PrimaryAssetReturnProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAssetReturnProfileRequiredCategoryRule(string Message) : IPrimaryAssetReturnProfileRule
{
    public List<string> Check(PrimaryAssetReturnProfile item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAssetReturnProfileRequiredCodeRule(string Message) : IPrimaryAssetReturnProfileRule
{
    public List<string> Check(PrimaryAssetReturnProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary asset return profile.</summary>
public sealed record PrimaryAssetReturnProfileValidator(List<IPrimaryAssetReturnProfileRule> Rules)
{
    public List<string> Validate(PrimaryAssetReturnProfile item)
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

    public void RequireValid(PrimaryAssetReturnProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAssetReturnProfile> items)
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

/// <summary>Functions of primary asset return profile rules.</summary>
public static class PrimaryAssetReturnProfileRules
{
    public static PrimaryAssetReturnProfileValidator DefaultPrimaryAssetReturnProfileValidator()
    {
        return new PrimaryAssetReturnProfileValidator(
            new List<IPrimaryAssetReturnProfileRule>
            {
                new PrimaryAssetReturnProfileAllowedTitleRule(new List<string> { "tiered", "pending", "strict" }),
                new PrimaryAssetReturnProfileTitleLengthRule(32),
                new PrimaryAssetReturnProfileRequiredCategoryRule("primary asset return profile needs an entry"),
                new PrimaryAssetReturnProfileRequiredCodeRule("primary asset return profile needs a value"),
            });
    }
}
