namespace Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>A rule a pending pay band quota must pass. Check returns its problems, or none.</summary>
public interface IPendingPayBandQuotaRule
{
    List<string> Check(PendingPayBandQuota item);
}

public sealed record PendingPayBandQuotaCodeLengthRule(int MaxLength) : IPendingPayBandQuotaRule
{
    public List<string> Check(PendingPayBandQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandQuotaWeightRangeRule(double Min, double Max) : IPendingPayBandQuotaRule
{
    public List<string> Check(PendingPayBandQuota item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingPayBandQuotaSourceLengthRule(int MaxLength) : IPendingPayBandQuotaRule
{
    public List<string> Check(PendingPayBandQuota item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pay band quota.</summary>
public sealed record PendingPayBandQuotaValidator(List<IPendingPayBandQuotaRule> Rules)
{
    public List<string> Validate(PendingPayBandQuota item)
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

    public void RequireValid(PendingPayBandQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPayBandQuota> items)
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

/// <summary>Functions of pending pay band quota rules.</summary>
public static class PendingPayBandQuotaRules
{
    public static PendingPayBandQuotaValidator DefaultPendingPayBandQuotaValidator()
    {
        return new PendingPayBandQuotaValidator(
            new List<IPendingPayBandQuotaRule>
            {
                new PendingPayBandQuotaCodeLengthRule(120),
                new PendingPayBandQuotaWeightRangeRule(4.5, 2647.1),
                new PendingPayBandQuotaSourceLengthRule(120),
            });
    }
}
