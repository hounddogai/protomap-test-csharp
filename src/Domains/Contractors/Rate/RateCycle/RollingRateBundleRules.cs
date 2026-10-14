namespace Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>A rule a rolling rate bundle must pass. Check returns its problems, or none.</summary>
public interface IRollingRateBundleRule
{
    List<string> Check(RollingRateBundle item);
}

public sealed record RollingRateBundleDiscountRangeRule(double Min, double Max) : IRollingRateBundleRule
{
    public List<string> Check(RollingRateBundle item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRateBundleRequiredNameRule(string Message) : IRollingRateBundleRule
{
    public List<string> Check(RollingRateBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRateBundleCodeLengthRule(int MaxLength) : IRollingRateBundleRule
{
    public List<string> Check(RollingRateBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling rate bundle.</summary>
public sealed record RollingRateBundleValidator(List<IRollingRateBundleRule> Rules)
{
    public List<string> Validate(RollingRateBundle item)
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

    public void RequireValid(RollingRateBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRateBundle> items)
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

/// <summary>Functions of rolling rate bundle rules.</summary>
public static class RollingRateBundleRules
{
    public static RollingRateBundleValidator DefaultRollingRateBundleValidator()
    {
        return new RollingRateBundleValidator(
            new List<IRollingRateBundleRule>
            {
                new RollingRateBundleDiscountRangeRule(5.5, 3436.6),
                new RollingRateBundleRequiredNameRule("rolling rate bundle needs this field"),
                new RollingRateBundleCodeLengthRule(16),
            });
    }
}
