namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>A rule a bonus pool bundle must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolBundleRule
{
    List<string> Check(BonusPoolBundle item);
}

public sealed record BonusPoolBundleRequiredCodeRule(string Message) : IBonusPoolBundleRule
{
    public List<string> Check(BonusPoolBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolBundleRequiredLabelRule(string Message) : IBonusPoolBundleRule
{
    public List<string> Check(BonusPoolBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolBundleLocationLengthRule(int MaxLength) : IBonusPoolBundleRule
{
    public List<string> Check(BonusPoolBundle item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolBundleCodeLengthRule(int MaxLength) : IBonusPoolBundleRule
{
    public List<string> Check(BonusPoolBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolBundleRatioRangeRule(double Min, double Max) : IBonusPoolBundleRule
{
    public List<string> Check(BonusPoolBundle item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool bundle.</summary>
public sealed record BonusPoolBundleValidator(List<IBonusPoolBundleRule> Rules)
{
    public List<string> Validate(BonusPoolBundle item)
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

    public void RequireValid(BonusPoolBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolBundle> items)
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

/// <summary>Functions of bonus pool bundle rules.</summary>
public static class BonusPoolBundleRules
{
    public static BonusPoolBundleValidator DefaultBonusPoolBundleValidator()
    {
        return new BonusPoolBundleValidator(
            new List<IBonusPoolBundleRule>
            {
                new BonusPoolBundleRequiredCodeRule("bonus pool bundle needs this field"),
                new BonusPoolBundleRequiredLabelRule("bonus pool bundle needs a value"),
                new BonusPoolBundleLocationLengthRule(255),
                new BonusPoolBundleCodeLengthRule(32),
                new BonusPoolBundleRatioRangeRule(1.5, 286.0),
            });
    }
}
