namespace Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>A rule a capped pay band quota must pass. Check returns its problems, or none.</summary>
public interface ICappedPayBandQuotaRule
{
    List<string> Check(CappedPayBandQuota item);
}

public sealed record CappedPayBandQuotaCodeLengthRule(int MaxLength) : ICappedPayBandQuotaRule
{
    public List<string> Check(CappedPayBandQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPayBandQuotaRequiredCodeRule(string Message) : ICappedPayBandQuotaRule
{
    public List<string> Check(CappedPayBandQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayBandQuotaRequiredEmployeeIdRule(string Message) : ICappedPayBandQuotaRule
{
    public List<string> Check(CappedPayBandQuota item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pay band quota.</summary>
public sealed record CappedPayBandQuotaValidator(List<ICappedPayBandQuotaRule> Rules)
{
    public List<string> Validate(CappedPayBandQuota item)
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

    public void RequireValid(CappedPayBandQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayBandQuota> items)
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

/// <summary>Functions of capped pay band quota rules.</summary>
public static class CappedPayBandQuotaRules
{
    public static CappedPayBandQuotaValidator DefaultCappedPayBandQuotaValidator()
    {
        return new CappedPayBandQuotaValidator(
            new List<ICappedPayBandQuotaRule>
            {
                new CappedPayBandQuotaCodeLengthRule(120),
                new CappedPayBandQuotaRequiredCodeRule("capped pay band quota needs a value"),
                new CappedPayBandQuotaRequiredEmployeeIdRule("capped pay band quota needs a value"),
            });
    }
}
