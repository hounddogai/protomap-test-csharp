namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal assignment draft is in its life cycle.</summary>
public enum SeasonalAssignmentDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a seasonal assignment draft and its current status.</summary>
public sealed class SeasonalAssignmentDraft(string code, string name, string employeeId, string salaryBand, int expiresAt, SeasonalAssignmentDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string SalaryBand { get; set; } = salaryBand;
    public int ExpiresAt { get; set; } = expiresAt;
    public SeasonalAssignmentDraftStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal assignment draft in status planned.</summary>
    public static SeasonalAssignmentDraft NewSeasonalAssignmentDraft(string code, string name)
    {
        return new SeasonalAssignmentDraft(code, name, "", "", 0, SeasonalAssignmentDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAssignmentDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAssignmentDraftStatus next)
    {
        switch (Status)
        {
            case SeasonalAssignmentDraftStatus.Planned:
                return next == SeasonalAssignmentDraftStatus.InProgress || next == SeasonalAssignmentDraftStatus.Cancelled;
            case SeasonalAssignmentDraftStatus.Scheduled:
                switch (next)
                {
                    case SeasonalAssignmentDraftStatus.Completed:
                    case SeasonalAssignmentDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalAssignmentDraftStatus.InProgress:
                return next == SeasonalAssignmentDraftStatus.Completed || next == SeasonalAssignmentDraftStatus.Cancelled;
            case SeasonalAssignmentDraftStatus.Completed:
                switch (next)
                {
                    case SeasonalAssignmentDraftStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalAssignmentDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal assignment draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public SeasonalAssignmentDraft Copy()
    {
        return new SeasonalAssignmentDraft(Code, Name, EmployeeId, SalaryBand, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
