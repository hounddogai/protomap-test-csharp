namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy exception quota must pass. Check returns its problems, or none.</summary>
public interface ILegacyExceptionQuotaRule
{
    List<string> Check(LegacyExceptionQuota item);
}

public sealed record LegacyExceptionQuotaAllowedTeamRule(List<string> Allowed) : ILegacyExceptionQuotaRule
{
    public List<string> Check(LegacyExceptionQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is too long" };
        }
        return new List<string>();
    }
}

public sealed record LegacyExceptionQuotaExpiresAtRangeRule(int Min, int Max) : ILegacyExceptionQuotaRule
{
    public List<string> Check(LegacyExceptionQuota item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyExceptionQuotaEmployeeIdLengthRule(int MaxLength) : ILegacyExceptionQuotaRule
{
    public List<string> Check(LegacyExceptionQuota item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy exception quota.</summary>
public sealed record LegacyExceptionQuotaValidator(List<ILegacyExceptionQuotaRule> Rules)
{
    public List<string> Validate(LegacyExceptionQuota item)
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

    public void RequireValid(LegacyExceptionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyExceptionQuota> items)
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

/// <summary>Functions of legacy exception quota rules.</summary>
public static class LegacyExceptionQuotaRules
{
    public static LegacyExceptionQuotaValidator DefaultLegacyExceptionQuotaValidator()
    {
        return new LegacyExceptionQuotaValidator(
            new List<ILegacyExceptionQuotaRule>
            {
                new LegacyExceptionQuotaAllowedTeamRule(new List<string> { "default" }),
                new LegacyExceptionQuotaExpiresAtRangeRule(16, 3600),
                new LegacyExceptionQuotaEmployeeIdLengthRule(86400),
            });
    }
}
