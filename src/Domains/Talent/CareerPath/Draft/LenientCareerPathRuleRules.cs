namespace Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>A rule a lenient career path rule must pass. Check returns its problems, or none.</summary>
public interface ILenientCareerPathRuleRule
{
    List<string> Check(LenientCareerPathRule item);
}

public sealed record LenientCareerPathRuleNameLengthRule(int MaxLength) : ILenientCareerPathRuleRule
{
    public List<string> Check(LenientCareerPathRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientCareerPathRuleRequiredNameRule(string Message) : ILenientCareerPathRuleRule
{
    public List<string> Check(LenientCareerPathRule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCareerPathRuleRequiredSourceRule(string Message) : ILenientCareerPathRuleRule
{
    public List<string> Check(LenientCareerPathRule item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCareerPathRuleCreatedAtRangeRule(int Min, int Max) : ILenientCareerPathRuleRule
{
    public List<string> Check(LenientCareerPathRule item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientCareerPathRuleRateRangeRule(double Min, double Max) : ILenientCareerPathRuleRule
{
    public List<string> Check(LenientCareerPathRule item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient career path rule.</summary>
public sealed record LenientCareerPathRuleValidator(List<ILenientCareerPathRuleRule> Rules)
{
    public List<string> Validate(LenientCareerPathRule item)
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

    public void RequireValid(LenientCareerPathRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientCareerPathRule> items)
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

/// <summary>Functions of lenient career path rule rules.</summary>
public static class LenientCareerPathRuleRules
{
    public static LenientCareerPathRuleValidator DefaultLenientCareerPathRuleValidator()
    {
        return new LenientCareerPathRuleValidator(
            new List<ILenientCareerPathRuleRule>
            {
                new LenientCareerPathRuleNameLengthRule(1000),
                new LenientCareerPathRuleRequiredNameRule("lenient career path rule needs an entry"),
                new LenientCareerPathRuleRequiredSourceRule("lenient career path rule needs a value"),
                new LenientCareerPathRuleCreatedAtRangeRule(1000, 3600),
                new LenientCareerPathRuleRateRangeRule(1.9, 492.7),
            });
    }
}
