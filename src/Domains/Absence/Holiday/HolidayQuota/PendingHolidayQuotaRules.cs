namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

/// <summary>A rule a pending holiday quota must pass. Check returns its problems, or none.</summary>
public interface IPendingHolidayQuotaRule
{
    List<string> Check(PendingHolidayQuota item);
}

public sealed record PendingHolidayQuotaLeaveDaysRangeRule(int Min, int Max) : IPendingHolidayQuotaRule
{
    public List<string> Check(PendingHolidayQuota item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingHolidayQuotaContactEmailLengthRule(int MaxLength) : IPendingHolidayQuotaRule
{
    public List<string> Check(PendingHolidayQuota item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingHolidayQuotaRequiredTimezoneRule(string Message) : IPendingHolidayQuotaRule
{
    public List<string> Check(PendingHolidayQuota item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingHolidayQuotaCodeLengthRule(int MaxLength) : IPendingHolidayQuotaRule
{
    public List<string> Check(PendingHolidayQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending holiday quota.</summary>
public sealed record PendingHolidayQuotaValidator(List<IPendingHolidayQuotaRule> Rules)
{
    public List<string> Validate(PendingHolidayQuota item)
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

    public void RequireValid(PendingHolidayQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingHolidayQuota> items)
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

/// <summary>Functions of pending holiday quota rules.</summary>
public static class PendingHolidayQuotaRules
{
    public static PendingHolidayQuotaValidator DefaultPendingHolidayQuotaValidator()
    {
        return new PendingHolidayQuotaValidator(
            new List<IPendingHolidayQuotaRule>
            {
                new PendingHolidayQuotaLeaveDaysRangeRule(32, 1000),
                new PendingHolidayQuotaContactEmailLengthRule(16),
                new PendingHolidayQuotaRequiredTimezoneRule("pending holiday quota needs this field"),
                new PendingHolidayQuotaCodeLengthRule(3600),
            });
    }
}
