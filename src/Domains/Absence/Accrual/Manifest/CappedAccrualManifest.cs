namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped accrual manifest is in its life cycle.</summary>
public enum CappedAccrualManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped accrual manifest. Its code is unique within a tenant.</summary>
public sealed class CappedAccrualManifest(string code, string label, int graceDays, string timezone, double maximum, int createdAt, string title, bool pinned, double fte, CappedAccrualManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public string Timezone { get; set; } = timezone;
    public double Maximum { get; set; } = maximum;
    public int CreatedAt { get; set; } = createdAt;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public double Fte { get; set; } = fte;
    public CappedAccrualManifestStatus Status { get; set; } = status;

    /// <summary>Starts a capped accrual manifest in status new.</summary>
    public static CappedAccrualManifest NewCappedAccrualManifest(string code, string label)
    {
        return new CappedAccrualManifest(
            code,
            label,
            0,
            "",
            0.0,
            0,
            "",
            false,
            0.0,
            CappedAccrualManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAccrualManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAccrualManifestStatus next)
    {
        switch (Status)
        {
            case CappedAccrualManifestStatus.New:
                switch (next)
                {
                    case CappedAccrualManifestStatus.Running:
                    case CappedAccrualManifestStatus.Failed:
                        return true;
                }
                return false;
            case CappedAccrualManifestStatus.Queued:
                switch (next)
                {
                    case CappedAccrualManifestStatus.Failed:
                        return true;
                }
                return false;
            case CappedAccrualManifestStatus.Running:
                switch (next)
                {
                    case CappedAccrualManifestStatus.Done:
                    case CappedAccrualManifestStatus.Failed:
                        return true;
                }
                return false;
            case CappedAccrualManifestStatus.Done:
                return next == CappedAccrualManifestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedAccrualManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped accrual manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"capped accrual manifest"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Maximum - (double)GraceDays, 2);
    }

    public CappedAccrualManifest Copy()
    {
        return new CappedAccrualManifest(
            Code,
            Label,
            GraceDays,
            Timezone,
            Maximum,
            CreatedAt,
            Title,
            Pinned,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
