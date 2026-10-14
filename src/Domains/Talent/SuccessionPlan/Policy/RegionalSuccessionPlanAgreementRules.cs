namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

/// <summary>A rule a regional succession plan agreement must pass. Check returns its problems, or none.</summary>
public interface IRegionalSuccessionPlanAgreementRule
{
    List<string> Check(RegionalSuccessionPlanAgreement item);
}

public sealed record RegionalSuccessionPlanAgreementRequiredLocationRule(string Message) : IRegionalSuccessionPlanAgreementRule
{
    public List<string> Check(RegionalSuccessionPlanAgreement item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAgreementCostCenterLengthRule(int MaxLength) : IRegionalSuccessionPlanAgreementRule
{
    public List<string> Check(RegionalSuccessionPlanAgreement item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAgreementRequiredCostCenterRule(string Message) : IRegionalSuccessionPlanAgreementRule
{
    public List<string> Check(RegionalSuccessionPlanAgreement item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanAgreementRatioRangeRule(double Min, double Max) : IRegionalSuccessionPlanAgreementRule
{
    public List<string> Check(RegionalSuccessionPlanAgreement item)
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

public sealed record RegionalSuccessionPlanAgreementSequenceRangeRule(int Min, int Max) : IRegionalSuccessionPlanAgreementRule
{
    public List<string> Check(RegionalSuccessionPlanAgreement item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional succession plan agreement.</summary>
public sealed record RegionalSuccessionPlanAgreementValidator(List<IRegionalSuccessionPlanAgreementRule> Rules)
{
    public List<string> Validate(RegionalSuccessionPlanAgreement item)
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

    public void RequireValid(RegionalSuccessionPlanAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSuccessionPlanAgreement> items)
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

/// <summary>Functions of regional succession plan agreement rules.</summary>
public static class RegionalSuccessionPlanAgreementRules
{
    public static RegionalSuccessionPlanAgreementValidator DefaultRegionalSuccessionPlanAgreementValidator()
    {
        return new RegionalSuccessionPlanAgreementValidator(
            new List<IRegionalSuccessionPlanAgreementRule>
            {
                new RegionalSuccessionPlanAgreementRequiredLocationRule(
                    "regional succession plan agreement needs a value"),
                new RegionalSuccessionPlanAgreementCostCenterLengthRule(255),
                new RegionalSuccessionPlanAgreementRequiredCostCenterRule(
                    "regional succession plan agreement needs a value"),
                new RegionalSuccessionPlanAgreementRatioRangeRule(6.3, 3605.2),
                new RegionalSuccessionPlanAgreementSequenceRangeRule(80, 86400),
            });
    }
}
