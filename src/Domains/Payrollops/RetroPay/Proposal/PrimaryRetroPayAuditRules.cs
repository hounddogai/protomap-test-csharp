namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary retro pay audit must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRetroPayAuditRule
{
    List<string> Check(PrimaryRetroPayAudit item);
}

public sealed record PrimaryRetroPayAuditRequiredLabelRule(string Message) : IPrimaryRetroPayAuditRule
{
    public List<string> Check(PrimaryRetroPayAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRetroPayAuditAllowedLabelRule(List<string> Allowed) : IPrimaryRetroPayAuditRule
{
    public List<string> Check(PrimaryRetroPayAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRetroPayAuditRatioRangeRule(double Min, double Max) : IPrimaryRetroPayAuditRule
{
    public List<string> Check(PrimaryRetroPayAudit item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRetroPayAuditRequiredCodeRule(string Message) : IPrimaryRetroPayAuditRule
{
    public List<string> Check(PrimaryRetroPayAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRetroPayAuditTimezoneLengthRule(int MaxLength) : IPrimaryRetroPayAuditRule
{
    public List<string> Check(PrimaryRetroPayAudit item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary retro pay audit.</summary>
public sealed record PrimaryRetroPayAuditValidator(List<IPrimaryRetroPayAuditRule> Rules)
{
    public List<string> Validate(PrimaryRetroPayAudit item)
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

    public void RequireValid(PrimaryRetroPayAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRetroPayAudit> items)
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

/// <summary>Functions of primary retro pay audit rules.</summary>
public static class PrimaryRetroPayAuditRules
{
    public static PrimaryRetroPayAuditValidator DefaultPrimaryRetroPayAuditValidator()
    {
        return new PrimaryRetroPayAuditValidator(
            new List<IPrimaryRetroPayAuditRule>
            {
                new PrimaryRetroPayAuditRequiredLabelRule("primary retro pay audit needs an entry"),
                new PrimaryRetroPayAuditAllowedLabelRule(new List<string> { "rolling", "fallback", "tiered" }),
                new PrimaryRetroPayAuditRatioRangeRule(7.4, 1405.5),
                new PrimaryRetroPayAuditRequiredCodeRule("primary retro pay audit needs an entry"),
                new PrimaryRetroPayAuditTimezoneLengthRule(255),
            });
    }
}
