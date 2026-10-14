namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>A rule a reorg threshold must pass. Check returns its problems, or none.</summary>
public interface IReorgThresholdRule
{
    List<string> Check(ReorgThreshold item);
}

public sealed record ReorgThresholdReviewerLengthRule(int MaxLength) : IReorgThresholdRule
{
    public List<string> Check(ReorgThreshold item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgThresholdDiscountRangeRule(double Min, double Max) : IReorgThresholdRule
{
    public List<string> Check(ReorgThreshold item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgThresholdRequiredSalaryBandRule(string Message) : IReorgThresholdRule
{
    public List<string> Check(ReorgThreshold item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgThresholdRequiredReviewerRule(string Message) : IReorgThresholdRule
{
    public List<string> Check(ReorgThreshold item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg threshold.</summary>
public sealed record ReorgThresholdValidator(List<IReorgThresholdRule> Rules)
{
    public List<string> Validate(ReorgThreshold item)
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

    public void RequireValid(ReorgThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgThreshold> items)
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

/// <summary>Functions of reorg threshold rules.</summary>
public static class ReorgThresholdRules
{
    public static ReorgThresholdValidator DefaultReorgThresholdValidator()
    {
        return new ReorgThresholdValidator(
            new List<IReorgThresholdRule>
            {
                new ReorgThresholdReviewerLengthRule(120),
                new ReorgThresholdDiscountRangeRule(3.7, 4658.9),
                new ReorgThresholdRequiredSalaryBandRule("reorg threshold needs this field"),
                new ReorgThresholdRequiredReviewerRule("reorg threshold needs an entry"),
            });
    }
}
