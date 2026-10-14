namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment variance must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentVarianceRule
{
    List<string> Check(AdjustmentVariance item);
}

public sealed record AdjustmentVarianceCurrencyLengthRule(int MaxLength) : IAdjustmentVarianceRule
{
    public List<string> Check(AdjustmentVariance item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentVarianceRequiredCodeRule(string Message) : IAdjustmentVarianceRule
{
    public List<string> Check(AdjustmentVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentVarianceRequiredLabelRule(string Message) : IAdjustmentVarianceRule
{
    public List<string> Check(AdjustmentVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentVarianceAllowedSegmentRule(List<string> Allowed) : IAdjustmentVarianceRule
{
    public List<string> Check(AdjustmentVariance item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment variance.</summary>
public sealed record AdjustmentVarianceValidator(List<IAdjustmentVarianceRule> Rules)
{
    public List<string> Validate(AdjustmentVariance item)
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

    public void RequireValid(AdjustmentVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentVariance> items)
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

/// <summary>Functions of adjustment variance rules.</summary>
public static class AdjustmentVarianceRules
{
    public static AdjustmentVarianceValidator DefaultAdjustmentVarianceValidator()
    {
        return new AdjustmentVarianceValidator(
            new List<IAdjustmentVarianceRule>
            {
                new AdjustmentVarianceCurrencyLengthRule(16),
                new AdjustmentVarianceRequiredCodeRule("adjustment variance needs a value"),
                new AdjustmentVarianceRequiredLabelRule("adjustment variance needs an entry"),
                new AdjustmentVarianceAllowedSegmentRule(new List<string> { "scheduled", "primary" }),
            });
    }
}
