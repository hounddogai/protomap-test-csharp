namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient warranty bundle must pass. Check returns its problems, or none.</summary>
public interface ILenientWarrantyBundleRule
{
    List<string> Check(LenientWarrantyBundle item);
}

public sealed record LenientWarrantyBundleAllowedLabelRule(List<string> Allowed) : ILenientWarrantyBundleRule
{
    public List<string> Check(LenientWarrantyBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyBundleCodePrefixRule(string Prefix) : ILenientWarrantyBundleRule
{
    public List<string> Check(LenientWarrantyBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyBundleCodeLengthRule(int MaxLength) : ILenientWarrantyBundleRule
{
    public List<string> Check(LenientWarrantyBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyBundleRequiredCodeRule(string Message) : ILenientWarrantyBundleRule
{
    public List<string> Check(LenientWarrantyBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyBundleSkillLengthRule(int MaxLength) : ILenientWarrantyBundleRule
{
    public List<string> Check(LenientWarrantyBundle item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientWarrantyBundleTenureMonthsRangeRule(int Min, int Max) : ILenientWarrantyBundleRule
{
    public List<string> Check(LenientWarrantyBundle item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient warranty bundle.</summary>
public sealed record LenientWarrantyBundleValidator(List<ILenientWarrantyBundleRule> Rules)
{
    public List<string> Validate(LenientWarrantyBundle item)
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

    public void RequireValid(LenientWarrantyBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientWarrantyBundle> items)
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

/// <summary>Functions of lenient warranty bundle rules.</summary>
public static class LenientWarrantyBundleRules
{
    public static LenientWarrantyBundleValidator DefaultLenientWarrantyBundleValidator()
    {
        return new LenientWarrantyBundleValidator(
            new List<ILenientWarrantyBundleRule>
            {
                new LenientWarrantyBundleAllowedLabelRule(new List<string> { "lenient" }),
                new LenientWarrantyBundleCodePrefixRule("REF"),
                new LenientWarrantyBundleCodeLengthRule(86400),
                new LenientWarrantyBundleRequiredCodeRule("lenient warranty bundle needs an entry"),
                new LenientWarrantyBundleSkillLengthRule(255),
                new LenientWarrantyBundleTenureMonthsRangeRule(16, 32),
            });
    }
}
