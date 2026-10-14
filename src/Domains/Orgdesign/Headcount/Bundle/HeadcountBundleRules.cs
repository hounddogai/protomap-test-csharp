namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount bundle must pass. Check returns its problems, or none.</summary>
public interface IHeadcountBundleRule
{
    List<string> Check(HeadcountBundle item);
}

public sealed record HeadcountBundleAllowedCodeRule(List<string> Allowed) : IHeadcountBundleRule
{
    public List<string> Check(HeadcountBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBundleRatingRangeRule(double Min, double Max) : IHeadcountBundleRule
{
    public List<string> Check(HeadcountBundle item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBundleRequiredCodeRule(string Message) : IHeadcountBundleRule
{
    public List<string> Check(HeadcountBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBundleCreatedAtRangeRule(int Min, int Max) : IHeadcountBundleRule
{
    public List<string> Check(HeadcountBundle item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBundleLabelLengthRule(int MaxLength) : IHeadcountBundleRule
{
    public List<string> Check(HeadcountBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBundleCeilingRangeRule(double Min, double Max) : IHeadcountBundleRule
{
    public List<string> Check(HeadcountBundle item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount bundle.</summary>
public sealed record HeadcountBundleValidator(List<IHeadcountBundleRule> Rules)
{
    public List<string> Validate(HeadcountBundle item)
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

    public void RequireValid(HeadcountBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountBundle> items)
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

/// <summary>Functions of headcount bundle rules.</summary>
public static class HeadcountBundleRules
{
    public static HeadcountBundleValidator DefaultHeadcountBundleValidator()
    {
        return new HeadcountBundleValidator(
            new List<IHeadcountBundleRule>
            {
                new HeadcountBundleAllowedCodeRule(new List<string>()),
                new HeadcountBundleRatingRangeRule(3.5, 116.5),
                new HeadcountBundleRequiredCodeRule("headcount bundle needs an entry"),
                new HeadcountBundleCreatedAtRangeRule(120, 86400),
                new HeadcountBundleLabelLengthRule(32),
                new HeadcountBundleCeilingRangeRule(9.3, 3334.1),
            });
    }
}
