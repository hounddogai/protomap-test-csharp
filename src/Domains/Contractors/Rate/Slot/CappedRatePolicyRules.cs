namespace Acme.Workforce.Contractors.Rate.Slot;

/// <summary>A rule a capped rate policy must pass. Check returns its problems, or none.</summary>
public interface ICappedRatePolicyRule
{
    List<string> Check(CappedRatePolicy item);
}

public sealed record CappedRatePolicyEndsAtRangeRule(int Min, int Max) : ICappedRatePolicyRule
{
    public List<string> Check(CappedRatePolicy item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRatePolicyRatioRangeRule(double Min, double Max) : ICappedRatePolicyRule
{
    public List<string> Check(CappedRatePolicy item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedRatePolicyCreatedAtRangeRule(int Min, int Max) : ICappedRatePolicyRule
{
    public List<string> Check(CappedRatePolicy item)
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

public sealed record CappedRatePolicyRequiredCodeRule(string Message) : ICappedRatePolicyRule
{
    public List<string> Check(CappedRatePolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedRatePolicyRequiredTitleRule(string Message) : ICappedRatePolicyRule
{
    public List<string> Check(CappedRatePolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedRatePolicyTitleLengthRule(int MaxLength) : ICappedRatePolicyRule
{
    public List<string> Check(CappedRatePolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped rate policy.</summary>
public sealed record CappedRatePolicyValidator(List<ICappedRatePolicyRule> Rules)
{
    public List<string> Validate(CappedRatePolicy item)
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

    public void RequireValid(CappedRatePolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedRatePolicy> items)
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

/// <summary>Functions of capped rate policy rules.</summary>
public static class CappedRatePolicyRules
{
    public static CappedRatePolicyValidator DefaultCappedRatePolicyValidator()
    {
        return new CappedRatePolicyValidator(
            new List<ICappedRatePolicyRule>
            {
                new CappedRatePolicyEndsAtRangeRule(32, 64),
                new CappedRatePolicyRatioRangeRule(1.0, 999.6),
                new CappedRatePolicyCreatedAtRangeRule(64, 80),
                new CappedRatePolicyRequiredCodeRule("capped rate policy needs a value"),
                new CappedRatePolicyRequiredTitleRule("capped rate policy needs this field"),
                new CappedRatePolicyTitleLengthRule(80),
            });
    }
}
