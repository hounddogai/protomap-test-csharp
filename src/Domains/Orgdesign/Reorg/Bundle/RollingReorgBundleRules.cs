namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>A rule a rolling reorg bundle must pass. Check returns its problems, or none.</summary>
public interface IRollingReorgBundleRule
{
    List<string> Check(RollingReorgBundle item);
}

public sealed record RollingReorgBundleLabelLengthRule(int MaxLength) : IRollingReorgBundleRule
{
    public List<string> Check(RollingReorgBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingReorgBundleTenureMonthsRangeRule(int Min, int Max) : IRollingReorgBundleRule
{
    public List<string> Check(RollingReorgBundle item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingReorgBundleRatingRangeRule(double Min, double Max) : IRollingReorgBundleRule
{
    public List<string> Check(RollingReorgBundle item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingReorgBundleJobCodeLengthRule(int MaxLength) : IRollingReorgBundleRule
{
    public List<string> Check(RollingReorgBundle item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingReorgBundleRequiredJobCodeRule(string Message) : IRollingReorgBundleRule
{
    public List<string> Check(RollingReorgBundle item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling reorg bundle.</summary>
public sealed record RollingReorgBundleValidator(List<IRollingReorgBundleRule> Rules)
{
    public List<string> Validate(RollingReorgBundle item)
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

    public void RequireValid(RollingReorgBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingReorgBundle> items)
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

/// <summary>Functions of rolling reorg bundle rules.</summary>
public static class RollingReorgBundleRules
{
    public static RollingReorgBundleValidator DefaultRollingReorgBundleValidator()
    {
        return new RollingReorgBundleValidator(
            new List<IRollingReorgBundleRule>
            {
                new RollingReorgBundleLabelLengthRule(255),
                new RollingReorgBundleTenureMonthsRangeRule(32, 1000),
                new RollingReorgBundleRatingRangeRule(1.5, 2856.2),
                new RollingReorgBundleJobCodeLengthRule(1000),
                new RollingReorgBundleRequiredJobCodeRule("rolling reorg bundle needs a value"),
            });
    }
}
