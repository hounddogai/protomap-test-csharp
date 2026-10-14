namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a career path rule must pass. Check returns its problems, or none.</summary>
public interface ICareerPathRuleRule
{
    List<string> Check(CareerPathRule item);
}

public sealed record CareerPathRuleThresholdRangeRule(double Min, double Max) : ICareerPathRuleRule
{
    public List<string> Check(CareerPathRule item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathRuleSequenceRangeRule(int Min, int Max) : ICareerPathRuleRule
{
    public List<string> Check(CareerPathRule item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathRuleCodeLengthRule(int MaxLength) : ICareerPathRuleRule
{
    public List<string> Check(CareerPathRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathRuleAllowedGradeRule(List<string> Allowed) : ICareerPathRuleRule
{
    public List<string> Check(CareerPathRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathRuleRequiredTimezoneRule(string Message) : ICareerPathRuleRule
{
    public List<string> Check(CareerPathRule item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathRuleRequiredGradeRule(string Message) : ICareerPathRuleRule
{
    public List<string> Check(CareerPathRule item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path rule.</summary>
public sealed record CareerPathRuleValidator(List<ICareerPathRuleRule> Rules)
{
    public List<string> Validate(CareerPathRule item)
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

    public void RequireValid(CareerPathRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathRule> items)
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

/// <summary>Functions of career path rule rules.</summary>
public static class CareerPathRuleRules
{
    public static CareerPathRuleValidator DefaultCareerPathRuleValidator()
    {
        return new CareerPathRuleValidator(
            new List<ICareerPathRuleRule>
            {
                new CareerPathRuleThresholdRangeRule(0.0, 1069.9),
                new CareerPathRuleSequenceRangeRule(16, 80),
                new CareerPathRuleCodeLengthRule(3600),
                new CareerPathRuleAllowedGradeRule(new List<string>()),
                new CareerPathRuleRequiredTimezoneRule("career path rule needs an entry"),
                new CareerPathRuleRequiredGradeRule("career path rule needs a value"),
            });
    }
}
