namespace Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>A rule a default adjustment agreement must pass. Check returns its problems, or none.</summary>
public interface IDefaultAdjustmentAgreementRule
{
    List<string> Check(DefaultAdjustmentAgreement item);
}

public sealed record DefaultAdjustmentAgreementTitleLengthRule(int MaxLength) : IDefaultAdjustmentAgreementRule
{
    public List<string> Check(DefaultAdjustmentAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAdjustmentAgreementTeamLengthRule(int MaxLength) : IDefaultAdjustmentAgreementRule
{
    public List<string> Check(DefaultAdjustmentAgreement item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAdjustmentAgreementCostCenterLengthRule(int MaxLength) : IDefaultAdjustmentAgreementRule
{
    public List<string> Check(DefaultAdjustmentAgreement item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default adjustment agreement.</summary>
public sealed record DefaultAdjustmentAgreementValidator(List<IDefaultAdjustmentAgreementRule> Rules)
{
    public List<string> Validate(DefaultAdjustmentAgreement item)
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

    public void RequireValid(DefaultAdjustmentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAdjustmentAgreement> items)
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

/// <summary>Functions of default adjustment agreement rules.</summary>
public static class DefaultAdjustmentAgreementRules
{
    public static DefaultAdjustmentAgreementValidator DefaultDefaultAdjustmentAgreementValidator()
    {
        return new DefaultAdjustmentAgreementValidator(
            new List<IDefaultAdjustmentAgreementRule>
            {
                new DefaultAdjustmentAgreementTitleLengthRule(64),
                new DefaultAdjustmentAgreementTeamLengthRule(16),
                new DefaultAdjustmentAgreementCostCenterLengthRule(255),
            });
    }
}
