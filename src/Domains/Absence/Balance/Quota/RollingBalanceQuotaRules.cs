namespace Acme.Workforce.Absence.Balance.Quota;

/// <summary>A rule a rolling balance quota must pass. Check returns its problems, or none.</summary>
public interface IRollingBalanceQuotaRule
{
    List<string> Check(RollingBalanceQuota item);
}

public sealed record RollingBalanceQuotaTenureMonthsRangeRule(int Min, int Max) : IRollingBalanceQuotaRule
{
    public List<string> Check(RollingBalanceQuota item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceQuotaCodeLengthRule(int MaxLength) : IRollingBalanceQuotaRule
{
    public List<string> Check(RollingBalanceQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBalanceQuotaFloorValueRangeRule(double Min, double Max) : IRollingBalanceQuotaRule
{
    public List<string> Check(RollingBalanceQuota item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling balance quota.</summary>
public sealed record RollingBalanceQuotaValidator(List<IRollingBalanceQuotaRule> Rules)
{
    public List<string> Validate(RollingBalanceQuota item)
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

    public void RequireValid(RollingBalanceQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBalanceQuota> items)
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

/// <summary>Functions of rolling balance quota rules.</summary>
public static class RollingBalanceQuotaRules
{
    public static RollingBalanceQuotaValidator DefaultRollingBalanceQuotaValidator()
    {
        return new RollingBalanceQuotaValidator(
            new List<IRollingBalanceQuotaRule>
            {
                new RollingBalanceQuotaTenureMonthsRangeRule(3600, 3600),
                new RollingBalanceQuotaCodeLengthRule(16),
                new RollingBalanceQuotaFloorValueRangeRule(9.7, 2043.4),
            });
    }
}
