namespace Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>A rule a bonus pool agreement must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolAgreementRule
{
    List<string> Check(BonusPoolAgreement item);
}

public sealed record BonusPoolAgreementUpdatedAtRangeRule(int Min, int Max) : IBonusPoolAgreementRule
{
    public List<string> Check(BonusPoolAgreement item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolAgreementGradeLengthRule(int MaxLength) : IBonusPoolAgreementRule
{
    public List<string> Check(BonusPoolAgreement item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAgreementCodeLengthRule(int MaxLength) : IBonusPoolAgreementRule
{
    public List<string> Check(BonusPoolAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolAgreementRequiredGradeRule(string Message) : IBonusPoolAgreementRule
{
    public List<string> Check(BonusPoolAgreement item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool agreement.</summary>
public sealed record BonusPoolAgreementValidator(List<IBonusPoolAgreementRule> Rules)
{
    public List<string> Validate(BonusPoolAgreement item)
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

    public void RequireValid(BonusPoolAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolAgreement> items)
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

/// <summary>Functions of bonus pool agreement rules.</summary>
public static class BonusPoolAgreementRules
{
    public static BonusPoolAgreementValidator DefaultBonusPoolAgreementValidator()
    {
        return new BonusPoolAgreementValidator(
            new List<IBonusPoolAgreementRule>
            {
                new BonusPoolAgreementUpdatedAtRangeRule(120, 120),
                new BonusPoolAgreementGradeLengthRule(3600),
                new BonusPoolAgreementCodeLengthRule(16),
                new BonusPoolAgreementRequiredGradeRule("bonus pool agreement needs an entry"),
            });
    }
}
