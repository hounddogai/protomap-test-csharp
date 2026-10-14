namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment bundle must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentBundleRule
{
    List<string> Check(AdjustmentBundle item);
}

public sealed record AdjustmentBundleRequiredSummaryRule(string Message) : IAdjustmentBundleRule
{
    public List<string> Check(AdjustmentBundle item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentBundleRequiredTitleRule(string Message) : IAdjustmentBundleRule
{
    public List<string> Check(AdjustmentBundle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentBundleRequiredCodeRule(string Message) : IAdjustmentBundleRule
{
    public List<string> Check(AdjustmentBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentBundleAllowedTitleRule(List<string> Allowed) : IAdjustmentBundleRule
{
    public List<string> Check(AdjustmentBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is too long" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentBundleCodeLengthRule(int MaxLength) : IAdjustmentBundleRule
{
    public List<string> Check(AdjustmentBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment bundle.</summary>
public sealed record AdjustmentBundleValidator(List<IAdjustmentBundleRule> Rules)
{
    public List<string> Validate(AdjustmentBundle item)
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

    public void RequireValid(AdjustmentBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentBundle> items)
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

/// <summary>Functions of adjustment bundle rules.</summary>
public static class AdjustmentBundleRules
{
    public static AdjustmentBundleValidator DefaultAdjustmentBundleValidator()
    {
        return new AdjustmentBundleValidator(
            new List<IAdjustmentBundleRule>
            {
                new AdjustmentBundleRequiredSummaryRule("adjustment bundle needs an entry"),
                new AdjustmentBundleRequiredTitleRule("adjustment bundle needs a value"),
                new AdjustmentBundleRequiredCodeRule("adjustment bundle needs an entry"),
                new AdjustmentBundleAllowedTitleRule(new List<string>()),
                new AdjustmentBundleCodeLengthRule(1000),
            });
    }
}
