namespace Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>A rule a retro pay audit must pass. Check returns its problems, or none.</summary>
public interface IRetroPayAuditRule
{
    List<string> Check(RetroPayAudit item);
}

public sealed record RetroPayAuditRequiredNameRule(string Message) : IRetroPayAuditRule
{
    public List<string> Check(RetroPayAudit item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAuditStartsAtRangeRule(int Min, int Max) : IRetroPayAuditRule
{
    public List<string> Check(RetroPayAudit item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAuditNameLengthRule(int MaxLength) : IRetroPayAuditRule
{
    public List<string> Check(RetroPayAudit item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAuditRequiredSourceRule(string Message) : IRetroPayAuditRule
{
    public List<string> Check(RetroPayAudit item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAuditRequiredCodeRule(string Message) : IRetroPayAuditRule
{
    public List<string> Check(RetroPayAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay audit.</summary>
public sealed record RetroPayAuditValidator(List<IRetroPayAuditRule> Rules)
{
    public List<string> Validate(RetroPayAudit item)
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

    public void RequireValid(RetroPayAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayAudit> items)
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

/// <summary>Functions of retro pay audit rules.</summary>
public static class RetroPayAuditRules
{
    public static RetroPayAuditValidator DefaultRetroPayAuditValidator()
    {
        return new RetroPayAuditValidator(
            new List<IRetroPayAuditRule>
            {
                new RetroPayAuditRequiredNameRule("retro pay audit needs a value"),
                new RetroPayAuditStartsAtRangeRule(16, 255),
                new RetroPayAuditNameLengthRule(1000),
                new RetroPayAuditRequiredSourceRule("retro pay audit needs this field"),
                new RetroPayAuditRequiredCodeRule("retro pay audit needs this field"),
            });
    }
}
