namespace Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>A rule a pay run quota must pass. Check returns its problems, or none.</summary>
public interface IPayRunQuotaRule
{
    List<string> Check(PayRunQuota item);
}

public sealed record PayRunQuotaRequiredNameRule(string Message) : IPayRunQuotaRule
{
    public List<string> Check(PayRunQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunQuotaNameLengthRule(int MaxLength) : IPayRunQuotaRule
{
    public List<string> Check(PayRunQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunQuotaChannelLengthRule(int MaxLength) : IPayRunQuotaRule
{
    public List<string> Check(PayRunQuota item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run quota.</summary>
public sealed record PayRunQuotaValidator(List<IPayRunQuotaRule> Rules)
{
    public List<string> Validate(PayRunQuota item)
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

    public void RequireValid(PayRunQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunQuota> items)
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

/// <summary>Functions of pay run quota rules.</summary>
public static class PayRunQuotaRules
{
    public static PayRunQuotaValidator DefaultPayRunQuotaValidator()
    {
        return new PayRunQuotaValidator(
            new List<IPayRunQuotaRule>
            {
                new PayRunQuotaRequiredNameRule("pay run quota needs a value"),
                new PayRunQuotaNameLengthRule(80),
                new PayRunQuotaChannelLengthRule(1000),
            });
    }
}
