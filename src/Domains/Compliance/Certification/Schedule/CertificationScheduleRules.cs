namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a certification schedule must pass. Check returns its problems, or none.</summary>
public interface ICertificationScheduleRule
{
    List<string> Check(CertificationSchedule item);
}

public sealed record CertificationScheduleRetriesRangeRule(int Min, int Max) : ICertificationScheduleRule
{
    public List<string> Check(CertificationSchedule item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationScheduleAllowedCodeRule(List<string> Allowed) : ICertificationScheduleRule
{
    public List<string> Check(CertificationSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record CertificationScheduleRequiredCodeRule(string Message) : ICertificationScheduleRule
{
    public List<string> Check(CertificationSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationScheduleVisibleGuardRule(int Limit) : ICertificationScheduleRule
{
    public List<string> Check(CertificationSchedule item)
    {
        if (item.Visible && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification schedule.</summary>
public sealed record CertificationScheduleValidator(List<ICertificationScheduleRule> Rules)
{
    public List<string> Validate(CertificationSchedule item)
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

    public void RequireValid(CertificationSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationSchedule> items)
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

/// <summary>Functions of certification schedule rules.</summary>
public static class CertificationScheduleRules
{
    public static CertificationScheduleValidator DefaultCertificationScheduleValidator()
    {
        return new CertificationScheduleValidator(
            new List<ICertificationScheduleRule>
            {
                new CertificationScheduleRetriesRangeRule(1000, 3600),
                new CertificationScheduleAllowedCodeRule(new List<string> { "manual", "seasonal", "regional" }),
                new CertificationScheduleRequiredCodeRule("certification schedule needs a value"),
                new CertificationScheduleVisibleGuardRule(80),
            });
    }
}
