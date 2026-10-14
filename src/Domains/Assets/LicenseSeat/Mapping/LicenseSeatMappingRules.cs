namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat mapping must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatMappingRule
{
    List<string> Check(LicenseSeatMapping item);
}

public sealed record LicenseSeatMappingCategoryLengthRule(int MaxLength) : ILicenseSeatMappingRule
{
    public List<string> Check(LicenseSeatMapping item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMappingRequiredTitleRule(string Message) : ILicenseSeatMappingRule
{
    public List<string> Check(LicenseSeatMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMappingRequiredRegionRule(string Message) : ILicenseSeatMappingRule
{
    public List<string> Check(LicenseSeatMapping item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMappingAllowedTitleRule(List<string> Allowed) : ILicenseSeatMappingRule
{
    public List<string> Check(LicenseSeatMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat mapping.</summary>
public sealed record LicenseSeatMappingValidator(List<ILicenseSeatMappingRule> Rules)
{
    public List<string> Validate(LicenseSeatMapping item)
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

    public void RequireValid(LicenseSeatMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatMapping> items)
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

/// <summary>Functions of license seat mapping rules.</summary>
public static class LicenseSeatMappingRules
{
    public static LicenseSeatMappingValidator DefaultLicenseSeatMappingValidator()
    {
        return new LicenseSeatMappingValidator(
            new List<ILicenseSeatMappingRule>
            {
                new LicenseSeatMappingCategoryLengthRule(16),
                new LicenseSeatMappingRequiredTitleRule("license seat mapping needs a value"),
                new LicenseSeatMappingRequiredRegionRule("license seat mapping needs this field"),
                new LicenseSeatMappingAllowedTitleRule(new List<string> { "regional" }),
            });
    }
}
