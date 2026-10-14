namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment agreement must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentAgreementRule
{
    List<string> Check(AdjustmentAgreement item);
}

public sealed record AdjustmentAgreementCodeLengthRule(int MaxLength) : IAdjustmentAgreementRule
{
    public List<string> Check(AdjustmentAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAgreementTeamLengthRule(int MaxLength) : IAdjustmentAgreementRule
{
    public List<string> Check(AdjustmentAgreement item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAgreementMinimumRangeRule(double Min, double Max) : IAdjustmentAgreementRule
{
    public List<string> Check(AdjustmentAgreement item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdjustmentAgreementAllowedGradeRule(List<string> Allowed) : IAdjustmentAgreementRule
{
    public List<string> Check(AdjustmentAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is expired" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentAgreementRequiredTeamRule(string Message) : IAdjustmentAgreementRule
{
    public List<string> Check(AdjustmentAgreement item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment agreement.</summary>
public sealed record AdjustmentAgreementValidator(List<IAdjustmentAgreementRule> Rules)
{
    public List<string> Validate(AdjustmentAgreement item)
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

    public void RequireValid(AdjustmentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentAgreement> items)
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

/// <summary>Functions of adjustment agreement rules.</summary>
public static class AdjustmentAgreementRules
{
    public static AdjustmentAgreementValidator DefaultAdjustmentAgreementValidator()
    {
        return new AdjustmentAgreementValidator(
            new List<IAdjustmentAgreementRule>
            {
                new AdjustmentAgreementCodeLengthRule(1000),
                new AdjustmentAgreementTeamLengthRule(32),
                new AdjustmentAgreementMinimumRangeRule(4.0, 3649.8),
                new AdjustmentAgreementAllowedGradeRule(new List<string>()),
                new AdjustmentAgreementRequiredTeamRule("adjustment agreement needs a value"),
            });
    }
}
