namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default laptop digest must pass. Check returns its problems, or none.</summary>
public interface IDefaultLaptopDigestRule
{
    List<string> Check(DefaultLaptopDigest item);
}

public sealed record DefaultLaptopDigestTitleLengthRule(int MaxLength) : IDefaultLaptopDigestRule
{
    public List<string> Check(DefaultLaptopDigest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopDigestRequiredCodeRule(string Message) : IDefaultLaptopDigestRule
{
    public List<string> Check(DefaultLaptopDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopDigestRequiredDepartmentRule(string Message) : IDefaultLaptopDigestRule
{
    public List<string> Check(DefaultLaptopDigest item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopDigestVersionRangeRule(int Min, int Max) : IDefaultLaptopDigestRule
{
    public List<string> Check(DefaultLaptopDigest item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultLaptopDigestAllowedDepartmentRule(List<string> Allowed) : IDefaultLaptopDigestRule
{
    public List<string> Check(DefaultLaptopDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default laptop digest.</summary>
public sealed record DefaultLaptopDigestValidator(List<IDefaultLaptopDigestRule> Rules)
{
    public List<string> Validate(DefaultLaptopDigest item)
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

    public void RequireValid(DefaultLaptopDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLaptopDigest> items)
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

/// <summary>Functions of default laptop digest rules.</summary>
public static class DefaultLaptopDigestRules
{
    public static DefaultLaptopDigestValidator DefaultDefaultLaptopDigestValidator()
    {
        return new DefaultLaptopDigestValidator(
            new List<IDefaultLaptopDigestRule>
            {
                new DefaultLaptopDigestTitleLengthRule(1000),
                new DefaultLaptopDigestRequiredCodeRule("default laptop digest needs this field"),
                new DefaultLaptopDigestRequiredDepartmentRule("default laptop digest needs this field"),
                new DefaultLaptopDigestVersionRangeRule(80, 3600),
                new DefaultLaptopDigestAllowedDepartmentRule(new List<string> { "lenient" }),
            });
    }
}
