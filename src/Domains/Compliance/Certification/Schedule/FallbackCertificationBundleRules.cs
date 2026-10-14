namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback certification bundle must pass. Check returns its problems, or none.</summary>
public interface IFallbackCertificationBundleRule
{
    List<string> Check(FallbackCertificationBundle item);
}

public sealed record FallbackCertificationBundleNameLengthRule(int MaxLength) : IFallbackCertificationBundleRule
{
    public List<string> Check(FallbackCertificationBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCertificationBundleTeamLengthRule(int MaxLength) : IFallbackCertificationBundleRule
{
    public List<string> Check(FallbackCertificationBundle item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCertificationBundleRequiredGradeRule(string Message) : IFallbackCertificationBundleRule
{
    public List<string> Check(FallbackCertificationBundle item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackCertificationBundleRequiredTeamRule(string Message) : IFallbackCertificationBundleRule
{
    public List<string> Check(FallbackCertificationBundle item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackCertificationBundleAllowedNameRule(List<string> Allowed) : IFallbackCertificationBundleRule
{
    public List<string> Check(FallbackCertificationBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record FallbackCertificationBundleQuantityRangeRule(int Min, int Max) : IFallbackCertificationBundleRule
{
    public List<string> Check(FallbackCertificationBundle item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback certification bundle.</summary>
public sealed record FallbackCertificationBundleValidator(List<IFallbackCertificationBundleRule> Rules)
{
    public List<string> Validate(FallbackCertificationBundle item)
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

    public void RequireValid(FallbackCertificationBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackCertificationBundle> items)
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

/// <summary>Functions of fallback certification bundle rules.</summary>
public static class FallbackCertificationBundleRules
{
    public static FallbackCertificationBundleValidator DefaultFallbackCertificationBundleValidator()
    {
        return new FallbackCertificationBundleValidator(
            new List<IFallbackCertificationBundleRule>
            {
                new FallbackCertificationBundleNameLengthRule(16),
                new FallbackCertificationBundleTeamLengthRule(1000),
                new FallbackCertificationBundleRequiredGradeRule("fallback certification bundle needs a value"),
                new FallbackCertificationBundleRequiredTeamRule("fallback certification bundle needs this field"),
                new FallbackCertificationBundleAllowedNameRule(new List<string>()),
                new FallbackCertificationBundleQuantityRangeRule(32, 32),
            });
    }
}
