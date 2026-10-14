namespace Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>A rule a pay run agreement must pass. Check returns its problems, or none.</summary>
public interface IPayRunAgreementRule
{
    List<string> Check(PayRunAgreement item);
}

public sealed record PayRunAgreementRequiredCategoryRule(string Message) : IPayRunAgreementRule
{
    public List<string> Check(PayRunAgreement item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunAgreementCategoryLengthRule(int MaxLength) : IPayRunAgreementRule
{
    public List<string> Check(PayRunAgreement item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAgreementRequiredNameRule(string Message) : IPayRunAgreementRule
{
    public List<string> Check(PayRunAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunAgreementRequiredOwnerIdRule(string Message) : IPayRunAgreementRule
{
    public List<string> Check(PayRunAgreement item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunAgreementOwnerIdLengthRule(int MaxLength) : IPayRunAgreementRule
{
    public List<string> Check(PayRunAgreement item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAgreementRatingRangeRule(double Min, double Max) : IPayRunAgreementRule
{
    public List<string> Check(PayRunAgreement item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run agreement.</summary>
public sealed record PayRunAgreementValidator(List<IPayRunAgreementRule> Rules)
{
    public List<string> Validate(PayRunAgreement item)
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

    public void RequireValid(PayRunAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunAgreement> items)
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

/// <summary>Functions of pay run agreement rules.</summary>
public static class PayRunAgreementRules
{
    public static PayRunAgreementValidator DefaultPayRunAgreementValidator()
    {
        return new PayRunAgreementValidator(
            new List<IPayRunAgreementRule>
            {
                new PayRunAgreementRequiredCategoryRule("pay run agreement needs a value"),
                new PayRunAgreementCategoryLengthRule(80),
                new PayRunAgreementRequiredNameRule("pay run agreement needs an entry"),
                new PayRunAgreementRequiredOwnerIdRule("pay run agreement needs a value"),
                new PayRunAgreementOwnerIdLengthRule(32),
                new PayRunAgreementRatingRangeRule(7.1, 1794.4),
            });
    }
}
