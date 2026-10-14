namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario quota is in its life cycle.</summary>
public enum ScenarioQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scenario quota, with what the planners need to act on it.</summary>
public sealed class ScenarioQuota(string code, string title, string employeeId, string country, string currency, string skill, int batchSize, double minimum, string salaryBand, string category, int startsAt, ScenarioQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Country { get; set; } = country;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public int BatchSize { get; set; } = batchSize;
    public double Minimum { get; set; } = minimum;
    public string SalaryBand { get; set; } = salaryBand;
    public string Category { get; set; } = category;
    public int StartsAt { get; set; } = startsAt;
    public ScenarioQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a scenario quota in status planned.</summary>
    public static ScenarioQuota NewScenarioQuota(string code, string title)
    {
        return new ScenarioQuota(code, title, "", "", "", "", 0, 0.0, "", "", 0, ScenarioQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioQuotaStatus next)
    {
        switch (Status)
        {
            case ScenarioQuotaStatus.Planned:
                switch (next)
                {
                    case ScenarioQuotaStatus.Scheduled:
                    case ScenarioQuotaStatus.Completed:
                        return true;
                }
                return false;
            case ScenarioQuotaStatus.Scheduled:
                switch (next)
                {
                    case ScenarioQuotaStatus.InProgress:
                    case ScenarioQuotaStatus.Completed:
                        return true;
                }
                return false;
            case ScenarioQuotaStatus.InProgress:
                return next == ScenarioQuotaStatus.Cancelled;
            case ScenarioQuotaStatus.Completed:
                return next == ScenarioQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScenarioQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetValue()
    {
        return Math.Round((double)BatchSize - Minimum, 2);
    }

    public ScenarioQuota Copy()
    {
        return new ScenarioQuota(
            Code,
            Title,
            EmployeeId,
            Country,
            Currency,
            Skill,
            BatchSize,
            Minimum,
            SalaryBand,
            Category,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
