namespace Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>A rule a capped pay band threshold must pass. Check returns its problems, or none.</summary>
public interface ICappedPayBandThresholdRule
{
    List<string> Check(CappedPayBandThreshold item);
}

public sealed record CappedPayBandThresholdCodeLengthRule(int MaxLength) : ICappedPayBandThresholdRule
{
    public List<string> Check(CappedPayBandThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayBandThresholdCostCenterLengthRule(int MaxLength) : ICappedPayBandThresholdRule
{
    public List<string> Check(CappedPayBandThreshold item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayBandThresholdRequiredLabelRule(string Message) : ICappedPayBandThresholdRule
{
    public List<string> Check(CappedPayBandThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pay band threshold.</summary>
public sealed record CappedPayBandThresholdValidator(List<ICappedPayBandThresholdRule> Rules)
{
    public List<string> Validate(CappedPayBandThreshold item)
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

    public void RequireValid(CappedPayBandThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayBandThreshold> items)
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

/// <summary>Functions of capped pay band threshold rules.</summary>
public static class CappedPayBandThresholdRules
{
    public static CappedPayBandThresholdValidator DefaultCappedPayBandThresholdValidator()
    {
        return new CappedPayBandThresholdValidator(
            new List<ICappedPayBandThresholdRule>
            {
                new CappedPayBandThresholdCodeLengthRule(80),
                new CappedPayBandThresholdCostCenterLengthRule(16),
                new CappedPayBandThresholdRequiredLabelRule("capped pay band threshold needs an entry"),
            });
    }
}
