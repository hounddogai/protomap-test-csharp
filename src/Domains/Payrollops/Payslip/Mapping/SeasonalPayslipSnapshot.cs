namespace Acme.Workforce.Payrollops.Payslip.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal payslip snapshot is in its life cycle.</summary>
public enum SeasonalPayslipSnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A seasonal payslip snapshot. Its code is unique within a tenant.</summary>
public sealed class SeasonalPayslipSnapshot(string code, string label, int updatedAt, int limit, string timezone, SeasonalPayslipSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Limit { get; set; } = limit;
    public string Timezone { get; set; } = timezone;
    public SeasonalPayslipSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal payslip snapshot in status open.</summary>
    public static SeasonalPayslipSnapshot NewSeasonalPayslipSnapshot(string code, string label)
    {
        return new SeasonalPayslipSnapshot(code, label, 0, 0, "", SeasonalPayslipSnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPayslipSnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPayslipSnapshotStatus next)
    {
        switch (Status)
        {
            case SeasonalPayslipSnapshotStatus.Open:
                return next == SeasonalPayslipSnapshotStatus.InReview || next == SeasonalPayslipSnapshotStatus.Reopened;
            case SeasonalPayslipSnapshotStatus.InReview:
                return next == SeasonalPayslipSnapshotStatus.Resolved || next == SeasonalPayslipSnapshotStatus.Archived;
            case SeasonalPayslipSnapshotStatus.Resolved:
                return next == SeasonalPayslipSnapshotStatus.Reopened;
            case SeasonalPayslipSnapshotStatus.Reopened:
                switch (next)
                {
                    case SeasonalPayslipSnapshotStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPayslipSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal payslip snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Limit - (double)UpdatedAt, 2);
    }

    public SeasonalPayslipSnapshot Copy()
    {
        return new SeasonalPayslipSnapshot(Code, Label, UpdatedAt, Limit, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
