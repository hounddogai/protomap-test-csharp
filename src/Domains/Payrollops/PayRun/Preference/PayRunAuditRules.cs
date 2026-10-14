namespace Acme.Workforce.Payrollops.PayRun.Preference;

/// <summary>A rule a pay run audit must pass. Check returns its problems, or none.</summary>
public interface IPayRunAuditRule
{
    List<string> Check(PayRunAudit item);
}

public sealed record PayRunAuditTitleLengthRule(int MaxLength) : IPayRunAuditRule
{
    public List<string> Check(PayRunAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAuditExternalRefLengthRule(int MaxLength) : IPayRunAuditRule
{
    public List<string> Check(PayRunAudit item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAuditCodeLengthRule(int MaxLength) : IPayRunAuditRule
{
    public List<string> Check(PayRunAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAuditCodePrefixRule(string Prefix) : IPayRunAuditRule
{
    public List<string> Check(PayRunAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAuditRequiredCodeRule(string Message) : IPayRunAuditRule
{
    public List<string> Check(PayRunAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunAuditScoreRangeRule(double Min, double Max) : IPayRunAuditRule
{
    public List<string> Check(PayRunAudit item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run audit.</summary>
public sealed record PayRunAuditValidator(List<IPayRunAuditRule> Rules)
{
    public List<string> Validate(PayRunAudit item)
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

    public void RequireValid(PayRunAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunAudit> items)
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

/// <summary>Functions of pay run audit rules.</summary>
public static class PayRunAuditRules
{
    public static PayRunAuditValidator DefaultPayRunAuditValidator()
    {
        return new PayRunAuditValidator(
            new List<IPayRunAuditRule>
            {
                new PayRunAuditTitleLengthRule(32),
                new PayRunAuditExternalRefLengthRule(86400),
                new PayRunAuditCodeLengthRule(3600),
                new PayRunAuditCodePrefixRule("OPS"),
                new PayRunAuditRequiredCodeRule("pay run audit needs an entry"),
                new PayRunAuditScoreRangeRule(1.9, 4972.1),
            });
    }
}
