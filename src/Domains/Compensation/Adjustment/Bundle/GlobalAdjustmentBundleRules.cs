namespace Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>A rule a global adjustment bundle must pass. Check returns its problems, or none.</summary>
public interface IGlobalAdjustmentBundleRule
{
    List<string> Check(GlobalAdjustmentBundle item);
}

public sealed record GlobalAdjustmentBundleRateRangeRule(double Min, double Max) : IGlobalAdjustmentBundleRule
{
    public List<string> Check(GlobalAdjustmentBundle item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentBundleRequiredNameRule(string Message) : IGlobalAdjustmentBundleRule
{
    public List<string> Check(GlobalAdjustmentBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAdjustmentBundleCodeLengthRule(int MaxLength) : IGlobalAdjustmentBundleRule
{
    public List<string> Check(GlobalAdjustmentBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global adjustment bundle.</summary>
public sealed record GlobalAdjustmentBundleValidator(List<IGlobalAdjustmentBundleRule> Rules)
{
    public List<string> Validate(GlobalAdjustmentBundle item)
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

    public void RequireValid(GlobalAdjustmentBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAdjustmentBundle> items)
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

/// <summary>Functions of global adjustment bundle rules.</summary>
public static class GlobalAdjustmentBundleRules
{
    public static GlobalAdjustmentBundleValidator DefaultGlobalAdjustmentBundleValidator()
    {
        return new GlobalAdjustmentBundleValidator(
            new List<IGlobalAdjustmentBundleRule>
            {
                new GlobalAdjustmentBundleRateRangeRule(2.5, 231.7),
                new GlobalAdjustmentBundleRequiredNameRule("global adjustment bundle needs a value"),
                new GlobalAdjustmentBundleCodeLengthRule(1000),
            });
    }
}
