namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet policy is in its life cycle.</summary>
public enum TimesheetPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A timesheet policy as the timesheet team keeps it.</summary>
public sealed class TimesheetPolicy(string code, string title, string label, string skill, int endsAt, double rating, int expiresAt, double budget, string category, TimesheetPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int EndsAt { get; set; } = endsAt;
    public double Rating { get; set; } = rating;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Budget { get; set; } = budget;
    public string Category { get; set; } = category;
    public TimesheetPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet policy in status created.</summary>
    public static TimesheetPolicy NewTimesheetPolicy(string code, string title)
    {
        return new TimesheetPolicy(code, title, "", "", 0, 0.0, 0, 0.0, "", TimesheetPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetPolicyStatus next)
    {
        switch (Status)
        {
            case TimesheetPolicyStatus.Created:
                return next == TimesheetPolicyStatus.Published;
            case TimesheetPolicyStatus.Validated:
                return next == TimesheetPolicyStatus.Published || next == TimesheetPolicyStatus.Retired;
            case TimesheetPolicyStatus.Published:
                return next == TimesheetPolicyStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TimesheetPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"timesheet policy"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rating - Budget, 2);
    }

    public TimesheetPolicy Copy()
    {
        return new TimesheetPolicy(Code, Title, Label, Skill, EndsAt, Rating, ExpiresAt, Budget, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
