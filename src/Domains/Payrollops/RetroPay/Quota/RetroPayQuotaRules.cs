namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay quota must pass. Check returns its problems, or none.</summary>
public interface IRetroPayQuotaRule
{
    List<string> Check(RetroPayQuota item);
}

public sealed record RetroPayQuotaCreatedAtRangeRule(int Min, int Max) : IRetroPayQuotaRule
{
    public List<string> Check(RetroPayQuota item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayQuotaRequiredCurrencyRule(string Message) : IRetroPayQuotaRule
{
    public List<string> Check(RetroPayQuota item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayQuotaAllowedCodeRule(List<string> Allowed) : IRetroPayQuotaRule
{
    public List<string> Check(RetroPayQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayQuotaTenureMonthsRangeRule(int Min, int Max) : IRetroPayQuotaRule
{
    public List<string> Check(RetroPayQuota item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayQuotaHoursRangeRule(double Min, double Max) : IRetroPayQuotaRule
{
    public List<string> Check(RetroPayQuota item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayQuotaCodePrefixRule(string Prefix) : IRetroPayQuotaRule
{
    public List<string> Check(RetroPayQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay quota.</summary>
public sealed record RetroPayQuotaValidator(List<IRetroPayQuotaRule> Rules)
{
    public List<string> Validate(RetroPayQuota item)
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

    public void RequireValid(RetroPayQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayQuota> items)
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

/// <summary>Functions of retro pay quota rules.</summary>
public static class RetroPayQuotaRules
{
    public static RetroPayQuotaValidator DefaultRetroPayQuotaValidator()
    {
        return new RetroPayQuotaValidator(
            new List<IRetroPayQuotaRule>
            {
                new RetroPayQuotaCreatedAtRangeRule(32, 86400),
                new RetroPayQuotaRequiredCurrencyRule("retro pay quota needs a value"),
                new RetroPayQuotaAllowedCodeRule(new List<string> { "scheduled", "scheduled" }),
                new RetroPayQuotaTenureMonthsRangeRule(255, 3600),
                new RetroPayQuotaHoursRangeRule(3.6, 1452.1),
                new RetroPayQuotaCodePrefixRule("REF"),
            });
    }
}
