namespace Acme.Workforce.Workplace.Desk.Entry;

/// <summary>A rule a desk quota must pass. Check returns its problems, or none.</summary>
public interface IDeskQuotaRule
{
    List<string> Check(DeskQuota item);
}

public sealed record DeskQuotaBudgetRangeRule(double Min, double Max) : IDeskQuotaRule
{
    public List<string> Check(DeskQuota item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskQuotaDurationMinutesRangeRule(int Min, int Max) : IDeskQuotaRule
{
    public List<string> Check(DeskQuota item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskQuotaRequiredCodeRule(string Message) : IDeskQuotaRule
{
    public List<string> Check(DeskQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskQuotaRequiredPhoneRule(string Message) : IDeskQuotaRule
{
    public List<string> Check(DeskQuota item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskQuotaCodePrefixRule(string Prefix) : IDeskQuotaRule
{
    public List<string> Check(DeskQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk quota.</summary>
public sealed record DeskQuotaValidator(List<IDeskQuotaRule> Rules)
{
    public List<string> Validate(DeskQuota item)
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

    public void RequireValid(DeskQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskQuota> items)
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

/// <summary>Functions of desk quota rules.</summary>
public static class DeskQuotaRules
{
    public static DeskQuotaValidator DefaultDeskQuotaValidator()
    {
        return new DeskQuotaValidator(
            new List<IDeskQuotaRule>
            {
                new DeskQuotaBudgetRangeRule(7.5, 4489.6),
                new DeskQuotaDurationMinutesRangeRule(16, 64),
                new DeskQuotaRequiredCodeRule("desk quota needs this field"),
                new DeskQuotaRequiredPhoneRule("desk quota needs an entry"),
                new DeskQuotaCodePrefixRule("REF"),
            });
    }
}
