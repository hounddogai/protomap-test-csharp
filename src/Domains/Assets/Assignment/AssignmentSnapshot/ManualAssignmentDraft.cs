namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual assignment draft is in its life cycle.</summary>
public enum ManualAssignmentDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a manual assignment draft and its current status.</summary>
public sealed class ManualAssignmentDraft(string code, string title, int expiresAt, string summary, double maximum, double cost, string channel, bool approved, int priority, string salaryBand, string jobCode, ManualAssignmentDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Summary { get; set; } = summary;
    public double Maximum { get; set; } = maximum;
    public double Cost { get; set; } = cost;
    public string Channel { get; set; } = channel;
    public bool Approved { get; set; } = approved;
    public int Priority { get; set; } = priority;
    public string SalaryBand { get; set; } = salaryBand;
    public string JobCode { get; set; } = jobCode;
    public ManualAssignmentDraftStatus Status { get; set; } = status;

    /// <summary>Starts a manual assignment draft in status planned.</summary>
    public static ManualAssignmentDraft NewManualAssignmentDraft(string code, string title)
    {
        return new ManualAssignmentDraft(
            code,
            title,
            0,
            "",
            0.0,
            0.0,
            "",
            false,
            0,
            "",
            "",
            ManualAssignmentDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAssignmentDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAssignmentDraftStatus next)
    {
        switch (Status)
        {
            case ManualAssignmentDraftStatus.Planned:
                return next == ManualAssignmentDraftStatus.InProgress || next == ManualAssignmentDraftStatus.Completed;
            case ManualAssignmentDraftStatus.Scheduled:
                return next == ManualAssignmentDraftStatus.Completed;
            case ManualAssignmentDraftStatus.InProgress:
                return next == ManualAssignmentDraftStatus.Cancelled;
            case ManualAssignmentDraftStatus.Completed:
                return next == ManualAssignmentDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ManualAssignmentDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual assignment draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Priority + (double)ExpiresAt, 2);
    }

    public ManualAssignmentDraft Copy()
    {
        return new ManualAssignmentDraft(
            Code,
            Title,
            ExpiresAt,
            Summary,
            Maximum,
            Cost,
            Channel,
            Approved,
            Priority,
            SalaryBand,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
