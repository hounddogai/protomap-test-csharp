namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat quota must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatQuotaRule
{
    List<string> Check(LicenseSeatQuota item);
}

public sealed record LicenseSeatQuotaRequiredNameRule(string Message) : ILicenseSeatQuotaRule
{
    public List<string> Check(LicenseSeatQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatQuotaGraceDaysRangeRule(int Min, int Max) : ILicenseSeatQuotaRule
{
    public List<string> Check(LicenseSeatQuota item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatQuotaAllowedTeamRule(List<string> Allowed) : ILicenseSeatQuotaRule
{
    public List<string> Check(LicenseSeatQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat quota.</summary>
public sealed record LicenseSeatQuotaValidator(List<ILicenseSeatQuotaRule> Rules)
{
    public List<string> Validate(LicenseSeatQuota item)
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

    public void RequireValid(LicenseSeatQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatQuota> items)
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

/// <summary>Functions of license seat quota rules.</summary>
public static class LicenseSeatQuotaRules
{
    public static LicenseSeatQuotaValidator DefaultLicenseSeatQuotaValidator()
    {
        return new LicenseSeatQuotaValidator(
            new List<ILicenseSeatQuotaRule>
            {
                new LicenseSeatQuotaRequiredNameRule("license seat quota needs this field"),
                new LicenseSeatQuotaGraceDaysRangeRule(1000, 3600),
                new LicenseSeatQuotaAllowedTeamRule(new List<string> { "pending", "regional" }),
            });
    }
}
