namespace Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>A rule a strict career path override must pass. Check returns its problems, or none.</summary>
public interface IStrictCareerPathOverrideRule
{
    List<string> Check(StrictCareerPathOverride item);
}

public sealed record StrictCareerPathOverrideRequiredTitleRule(string Message) : IStrictCareerPathOverrideRule
{
    public List<string> Check(StrictCareerPathOverride item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathOverrideRequiredCodeRule(string Message) : IStrictCareerPathOverrideRule
{
    public List<string> Check(StrictCareerPathOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathOverrideDiscountRangeRule(double Min, double Max) : IStrictCareerPathOverrideRule
{
    public List<string> Check(StrictCareerPathOverride item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictCareerPathOverrideUnitLengthRule(int MaxLength) : IStrictCareerPathOverrideRule
{
    public List<string> Check(StrictCareerPathOverride item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict career path override.</summary>
public sealed record StrictCareerPathOverrideValidator(List<IStrictCareerPathOverrideRule> Rules)
{
    public List<string> Validate(StrictCareerPathOverride item)
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

    public void RequireValid(StrictCareerPathOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictCareerPathOverride> items)
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

/// <summary>Functions of strict career path override rules.</summary>
public static class StrictCareerPathOverrideRules
{
    public static StrictCareerPathOverrideValidator DefaultStrictCareerPathOverrideValidator()
    {
        return new StrictCareerPathOverrideValidator(
            new List<IStrictCareerPathOverrideRule>
            {
                new StrictCareerPathOverrideRequiredTitleRule("strict career path override needs a value"),
                new StrictCareerPathOverrideRequiredCodeRule("strict career path override needs this field"),
                new StrictCareerPathOverrideDiscountRangeRule(7.7, 597.6),
                new StrictCareerPathOverrideUnitLengthRule(255),
            });
    }
}
