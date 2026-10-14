namespace Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>A rule a capped floor plan agreement must pass. Check returns its problems, or none.</summary>
public interface ICappedFloorPlanAgreementRule
{
    List<string> Check(CappedFloorPlanAgreement item);
}

public sealed record CappedFloorPlanAgreementCodeLengthRule(int MaxLength) : ICappedFloorPlanAgreementRule
{
    public List<string> Check(CappedFloorPlanAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedFloorPlanAgreementBadgeLengthRule(int MaxLength) : ICappedFloorPlanAgreementRule
{
    public List<string> Check(CappedFloorPlanAgreement item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedFloorPlanAgreementQuantityRangeRule(int Min, int Max) : ICappedFloorPlanAgreementRule
{
    public List<string> Check(CappedFloorPlanAgreement item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedFloorPlanAgreementRatioRangeRule(double Min, double Max) : ICappedFloorPlanAgreementRule
{
    public List<string> Check(CappedFloorPlanAgreement item)
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

public sealed record CappedFloorPlanAgreementRequiredBadgeRule(string Message) : ICappedFloorPlanAgreementRule
{
    public List<string> Check(CappedFloorPlanAgreement item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped floor plan agreement.</summary>
public sealed record CappedFloorPlanAgreementValidator(List<ICappedFloorPlanAgreementRule> Rules)
{
    public List<string> Validate(CappedFloorPlanAgreement item)
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

    public void RequireValid(CappedFloorPlanAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedFloorPlanAgreement> items)
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

/// <summary>Functions of capped floor plan agreement rules.</summary>
public static class CappedFloorPlanAgreementRules
{
    public static CappedFloorPlanAgreementValidator DefaultCappedFloorPlanAgreementValidator()
    {
        return new CappedFloorPlanAgreementValidator(
            new List<ICappedFloorPlanAgreementRule>
            {
                new CappedFloorPlanAgreementCodeLengthRule(64),
                new CappedFloorPlanAgreementBadgeLengthRule(16),
                new CappedFloorPlanAgreementQuantityRangeRule(1000, 1000),
                new CappedFloorPlanAgreementRatioRangeRule(0.6, 3420.8),
                new CappedFloorPlanAgreementRequiredBadgeRule("capped floor plan agreement needs this field"),
            });
    }
}
