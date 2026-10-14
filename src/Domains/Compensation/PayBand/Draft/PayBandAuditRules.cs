namespace Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>A rule a pay band audit must pass. Check returns its problems, or none.</summary>
public interface IPayBandAuditRule
{
    List<string> Check(PayBandAudit item);
}

public sealed record PayBandAuditRequiredDescriptionRule(string Message) : IPayBandAuditRule
{
    public List<string> Check(PayBandAudit item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandAuditRequiredLabelRule(string Message) : IPayBandAuditRule
{
    public List<string> Check(PayBandAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayBandAuditCodePrefixRule(string Prefix) : IPayBandAuditRule
{
    public List<string> Check(PayBandAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandAuditRetriesRangeRule(int Min, int Max) : IPayBandAuditRule
{
    public List<string> Check(PayBandAudit item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band audit.</summary>
public sealed record PayBandAuditValidator(List<IPayBandAuditRule> Rules)
{
    public List<string> Validate(PayBandAudit item)
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

    public void RequireValid(PayBandAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandAudit> items)
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

/// <summary>Functions of pay band audit rules.</summary>
public static class PayBandAuditRules
{
    public static PayBandAuditValidator DefaultPayBandAuditValidator()
    {
        return new PayBandAuditValidator(
            new List<IPayBandAuditRule>
            {
                new PayBandAuditRequiredDescriptionRule("pay band audit needs this field"),
                new PayBandAuditRequiredLabelRule("pay band audit needs this field"),
                new PayBandAuditCodePrefixRule("OPS"),
                new PayBandAuditRetriesRangeRule(80, 120),
            });
    }
}
