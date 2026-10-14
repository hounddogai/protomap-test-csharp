namespace Acme.Workforce.Absence.Accrual.Notice;

/// <summary>A rule a primary accrual notice must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAccrualNoticeRule
{
    List<string> Check(PrimaryAccrualNotice item);
}

public sealed record PrimaryAccrualNoticeRequiredCountryRule(string Message) : IPrimaryAccrualNoticeRule
{
    public List<string> Check(PrimaryAccrualNotice item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualNoticeRequiredCodeRule(string Message) : IPrimaryAccrualNoticeRule
{
    public List<string> Check(PrimaryAccrualNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualNoticeCountryLengthRule(int MaxLength) : IPrimaryAccrualNoticeRule
{
    public List<string> Check(PrimaryAccrualNotice item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary accrual notice.</summary>
public sealed record PrimaryAccrualNoticeValidator(List<IPrimaryAccrualNoticeRule> Rules)
{
    public List<string> Validate(PrimaryAccrualNotice item)
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

    public void RequireValid(PrimaryAccrualNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAccrualNotice> items)
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

/// <summary>Functions of primary accrual notice rules.</summary>
public static class PrimaryAccrualNoticeRules
{
    public static PrimaryAccrualNoticeValidator DefaultPrimaryAccrualNoticeValidator()
    {
        return new PrimaryAccrualNoticeValidator(
            new List<IPrimaryAccrualNoticeRule>
            {
                new PrimaryAccrualNoticeRequiredCountryRule("primary accrual notice needs an entry"),
                new PrimaryAccrualNoticeRequiredCodeRule("primary accrual notice needs a value"),
                new PrimaryAccrualNoticeCountryLengthRule(16),
            });
    }
}
