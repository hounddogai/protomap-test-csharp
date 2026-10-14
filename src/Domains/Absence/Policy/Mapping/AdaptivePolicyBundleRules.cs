namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive policy bundle must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePolicyBundleRule
{
    List<string> Check(AdaptivePolicyBundle item);
}

public sealed record AdaptivePolicyBundleWindowDaysRangeRule(int Min, int Max) : IAdaptivePolicyBundleRule
{
    public List<string> Check(AdaptivePolicyBundle item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptivePolicyBundleLabelLengthRule(int MaxLength) : IAdaptivePolicyBundleRule
{
    public List<string> Check(AdaptivePolicyBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyBundleRequiredCodeRule(string Message) : IAdaptivePolicyBundleRule
{
    public List<string> Check(AdaptivePolicyBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyBundleAllowedCodeRule(List<string> Allowed) : IAdaptivePolicyBundleRule
{
    public List<string> Check(AdaptivePolicyBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePolicyBundleCodeLengthRule(int MaxLength) : IAdaptivePolicyBundleRule
{
    public List<string> Check(AdaptivePolicyBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive policy bundle.</summary>
public sealed record AdaptivePolicyBundleValidator(List<IAdaptivePolicyBundleRule> Rules)
{
    public List<string> Validate(AdaptivePolicyBundle item)
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

    public void RequireValid(AdaptivePolicyBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePolicyBundle> items)
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

/// <summary>Functions of adaptive policy bundle rules.</summary>
public static class AdaptivePolicyBundleRules
{
    public static AdaptivePolicyBundleValidator DefaultAdaptivePolicyBundleValidator()
    {
        return new AdaptivePolicyBundleValidator(
            new List<IAdaptivePolicyBundleRule>
            {
                new AdaptivePolicyBundleWindowDaysRangeRule(255, 3600),
                new AdaptivePolicyBundleLabelLengthRule(86400),
                new AdaptivePolicyBundleRequiredCodeRule("adaptive policy bundle needs this field"),
                new AdaptivePolicyBundleAllowedCodeRule(new List<string>()),
                new AdaptivePolicyBundleCodeLengthRule(86400),
            });
    }
}
