namespace Acme.Workforce.Compensation.Adjustment.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback adjustment draft is in its life cycle.</summary>
public enum FallbackAdjustmentDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One fallback adjustment draft, with what the planners need to act on it.</summary>
public sealed class FallbackAdjustmentDraft(string code, string name, int version, string salaryBand, int retries, string employeeId, string managerId, FallbackAdjustmentDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string SalaryBand { get; set; } = salaryBand;
    public int Retries { get; set; } = retries;
    public string EmployeeId { get; set; } = employeeId;
    public string ManagerId { get; set; } = managerId;
    public FallbackAdjustmentDraftStatus Status { get; set; } = status;

    /// <summary>Starts a fallback adjustment draft in status open.</summary>
    public static FallbackAdjustmentDraft NewFallbackAdjustmentDraft(string code, string name)
    {
        return new FallbackAdjustmentDraft(code, name, 0, "", 0, "", "", FallbackAdjustmentDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAdjustmentDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAdjustmentDraftStatus next)
    {
        switch (Status)
        {
            case FallbackAdjustmentDraftStatus.Open:
                switch (next)
                {
                    case FallbackAdjustmentDraftStatus.Resolved:
                    case FallbackAdjustmentDraftStatus.Archived:
                        return true;
                }
                return false;
            case FallbackAdjustmentDraftStatus.InReview:
                switch (next)
                {
                    case FallbackAdjustmentDraftStatus.Reopened:
                    case FallbackAdjustmentDraftStatus.Archived:
                        return true;
                }
                return false;
            case FallbackAdjustmentDraftStatus.Resolved:
                switch (next)
                {
                    case FallbackAdjustmentDraftStatus.Reopened:
                    case FallbackAdjustmentDraftStatus.Archived:
                        return true;
                }
                return false;
            case FallbackAdjustmentDraftStatus.Reopened:
                return next == FallbackAdjustmentDraftStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackAdjustmentDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback adjustment draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Version - (double)Retries, 2);
    }

    public FallbackAdjustmentDraft Copy()
    {
        return new FallbackAdjustmentDraft(Code, Name, Version, SalaryBand, Retries, EmployeeId, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
