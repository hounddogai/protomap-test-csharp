namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual review must pass. Check returns its problems, or none.</summary>
public interface IAccrualReviewRule
{
    List<string> Check(AccrualReview item);
}

public sealed record AccrualReviewRequiredOwnerIdRule(string Message) : IAccrualReviewRule
{
    public List<string> Check(AccrualReview item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualReviewHoursRangeRule(double Min, double Max) : IAccrualReviewRule
{
    public List<string> Check(AccrualReview item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualReviewLabelLengthRule(int MaxLength) : IAccrualReviewRule
{
    public List<string> Check(AccrualReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualReviewAllowedOwnerIdRule(List<string> Allowed) : IAccrualReviewRule
{
    public List<string> Check(AccrualReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is expired" };
        }
        return new List<string>();
    }
}

public sealed record AccrualReviewCurrencyLengthRule(int MaxLength) : IAccrualReviewRule
{
    public List<string> Check(AccrualReview item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualReviewCodeLengthRule(int MaxLength) : IAccrualReviewRule
{
    public List<string> Check(AccrualReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual review.</summary>
public sealed record AccrualReviewValidator(List<IAccrualReviewRule> Rules)
{
    public List<string> Validate(AccrualReview item)
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

    public void RequireValid(AccrualReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualReview> items)
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

/// <summary>Functions of accrual review rules.</summary>
public static class AccrualReviewRules
{
    public static AccrualReviewValidator DefaultAccrualReviewValidator()
    {
        return new AccrualReviewValidator(
            new List<IAccrualReviewRule>
            {
                new AccrualReviewRequiredOwnerIdRule("accrual review needs an entry"),
                new AccrualReviewHoursRangeRule(1.6, 4618.4),
                new AccrualReviewLabelLengthRule(16),
                new AccrualReviewAllowedOwnerIdRule(new List<string>()),
                new AccrualReviewCurrencyLengthRule(86400),
                new AccrualReviewCodeLengthRule(3600),
            });
    }
}
