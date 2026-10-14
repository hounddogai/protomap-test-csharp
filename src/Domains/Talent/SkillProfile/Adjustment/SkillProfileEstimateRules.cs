namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>A rule a skill profile estimate must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileEstimateRule
{
    List<string> Check(SkillProfileEstimate item);
}

public sealed record SkillProfileEstimateGraceDaysRangeRule(int Min, int Max) : ISkillProfileEstimateRule
{
    public List<string> Check(SkillProfileEstimate item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileEstimateRequiredCodeRule(string Message) : ISkillProfileEstimateRule
{
    public List<string> Check(SkillProfileEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileEstimateStartsAtRangeRule(int Min, int Max) : ISkillProfileEstimateRule
{
    public List<string> Check(SkillProfileEstimate item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileEstimateCodeLengthRule(int MaxLength) : ISkillProfileEstimateRule
{
    public List<string> Check(SkillProfileEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileEstimateMarginRangeRule(double Min, double Max) : ISkillProfileEstimateRule
{
    public List<string> Check(SkillProfileEstimate item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileEstimateCodePrefixRule(string Prefix) : ISkillProfileEstimateRule
{
    public List<string> Check(SkillProfileEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile estimate.</summary>
public sealed record SkillProfileEstimateValidator(List<ISkillProfileEstimateRule> Rules)
{
    public List<string> Validate(SkillProfileEstimate item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(SkillProfileEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileEstimate> items)
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

/// <summary>Functions of skill profile estimate rules.</summary>
public static class SkillProfileEstimateRules
{
    public static SkillProfileEstimateValidator DefaultSkillProfileEstimateValidator()
    {
        return new SkillProfileEstimateValidator(
            new List<ISkillProfileEstimateRule>
            {
                new SkillProfileEstimateGraceDaysRangeRule(80, 120),
                new SkillProfileEstimateRequiredCodeRule("skill profile estimate needs a value"),
                new SkillProfileEstimateStartsAtRangeRule(32, 255),
                new SkillProfileEstimateCodeLengthRule(80),
                new SkillProfileEstimateMarginRangeRule(1.9, 2417.1),
                new SkillProfileEstimateCodePrefixRule("OPS"),
            });
    }
}
