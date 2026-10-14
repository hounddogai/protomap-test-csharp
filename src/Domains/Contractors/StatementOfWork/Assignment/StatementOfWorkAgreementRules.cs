namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>A rule a statement of work agreement must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkAgreementRule
{
    List<string> Check(StatementOfWorkAgreement item);
}

public sealed record StatementOfWorkAgreementDiscountRangeRule(double Min, double Max) : IStatementOfWorkAgreementRule
{
    public List<string> Check(StatementOfWorkAgreement item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAgreementCodeLengthRule(int MaxLength) : IStatementOfWorkAgreementRule
{
    public List<string> Check(StatementOfWorkAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAgreementGraceDaysRangeRule(int Min, int Max) : IStatementOfWorkAgreementRule
{
    public List<string> Check(StatementOfWorkAgreement item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAgreementRegionLengthRule(int MaxLength) : IStatementOfWorkAgreementRule
{
    public List<string> Check(StatementOfWorkAgreement item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkAgreementTitleLengthRule(int MaxLength) : IStatementOfWorkAgreementRule
{
    public List<string> Check(StatementOfWorkAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work agreement.</summary>
public sealed record StatementOfWorkAgreementValidator(List<IStatementOfWorkAgreementRule> Rules)
{
    public List<string> Validate(StatementOfWorkAgreement item)
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

    public void RequireValid(StatementOfWorkAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkAgreement> items)
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

/// <summary>Functions of statement of work agreement rules.</summary>
public static class StatementOfWorkAgreementRules
{
    public static StatementOfWorkAgreementValidator DefaultStatementOfWorkAgreementValidator()
    {
        return new StatementOfWorkAgreementValidator(
            new List<IStatementOfWorkAgreementRule>
            {
                new StatementOfWorkAgreementDiscountRangeRule(0.4, 1172.6),
                new StatementOfWorkAgreementCodeLengthRule(120),
                new StatementOfWorkAgreementGraceDaysRangeRule(120, 86400),
                new StatementOfWorkAgreementRegionLengthRule(80),
                new StatementOfWorkAgreementTitleLengthRule(32),
            });
    }
}
