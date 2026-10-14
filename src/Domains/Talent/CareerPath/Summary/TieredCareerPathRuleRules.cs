namespace Acme.Workforce.Talent.CareerPath.Summary;

/// <summary>A rule a tiered career path rule must pass. Check returns its problems, or none.</summary>
public interface ITieredCareerPathRuleRule
{
    List<string> Check(TieredCareerPathRule item);
}

public sealed record TieredCareerPathRuleChecksumLengthRule(int MaxLength) : ITieredCareerPathRuleRule
{
    public List<string> Check(TieredCareerPathRule item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCareerPathRuleRequiredCodeRule(string Message) : ITieredCareerPathRuleRule
{
    public List<string> Check(TieredCareerPathRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredCareerPathRuleEmployeeIdLengthRule(int MaxLength) : ITieredCareerPathRuleRule
{
    public List<string> Check(TieredCareerPathRule item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered career path rule.</summary>
public sealed record TieredCareerPathRuleValidator(List<ITieredCareerPathRuleRule> Rules)
{
    public List<string> Validate(TieredCareerPathRule item)
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

    public void RequireValid(TieredCareerPathRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredCareerPathRule> items)
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

/// <summary>Functions of tiered career path rule rules.</summary>
public static class TieredCareerPathRuleRules
{
    public static TieredCareerPathRuleValidator DefaultTieredCareerPathRuleValidator()
    {
        return new TieredCareerPathRuleValidator(
            new List<ITieredCareerPathRuleRule>
            {
                new TieredCareerPathRuleChecksumLengthRule(80),
                new TieredCareerPathRuleRequiredCodeRule("tiered career path rule needs a value"),
                new TieredCareerPathRuleEmployeeIdLengthRule(120),
            });
    }
}
