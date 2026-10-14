namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual scenario window is in its life cycle.</summary>
public enum ManualScenarioWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A manual scenario window as the scenario team keeps it.</summary>
public sealed class ManualScenarioWindow(string code, string title, string jobCode, int graceDays, int endsAt, int windowDays, string timezone, string skill, string employeeId, string unit, ManualScenarioWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public int EndsAt { get; set; } = endsAt;
    public int WindowDays { get; set; } = windowDays;
    public string Timezone { get; set; } = timezone;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public string Unit { get; set; } = unit;
    public ManualScenarioWindowStatus Status { get; set; } = status;

    /// <summary>Starts a manual scenario window in status requested.</summary>
    public static ManualScenarioWindow NewManualScenarioWindow(string code, string title)
    {
        return new ManualScenarioWindow(
            code,
            title,
            "",
            0,
            0,
            0,
            "",
            "",
            "",
            "",
            ManualScenarioWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualScenarioWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualScenarioWindowStatus next)
    {
        switch (Status)
        {
            case ManualScenarioWindowStatus.Requested:
                switch (next)
                {
                    case ManualScenarioWindowStatus.Granted:
                        return true;
                }
                return false;
            case ManualScenarioWindowStatus.Granted:
                switch (next)
                {
                    case ManualScenarioWindowStatus.Revoked:
                    case ManualScenarioWindowStatus.Expired:
                        return true;
                }
                return false;
            case ManualScenarioWindowStatus.Revoked:
                return next == ManualScenarioWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualScenarioWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual scenario window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)GraceDays * (double)WindowDays, 2);
    }

    public ManualScenarioWindow Copy()
    {
        return new ManualScenarioWindow(
            Code,
            Title,
            JobCode,
            GraceDays,
            EndsAt,
            WindowDays,
            Timezone,
            Skill,
            EmployeeId,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
