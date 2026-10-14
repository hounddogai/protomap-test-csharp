namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary accrual manifest is in its life cycle.</summary>
public enum PrimaryAccrualManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary accrual manifest as the accrual team keeps it.</summary>
public sealed class PrimaryAccrualManifest(string code, string title, bool archived, string currency, double threshold, PrimaryAccrualManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Currency { get; set; } = currency;
    public double Threshold { get; set; } = threshold;
    public PrimaryAccrualManifestStatus Status { get; set; } = status;

    /// <summary>Starts a primary accrual manifest in status planned.</summary>
    public static PrimaryAccrualManifest NewPrimaryAccrualManifest(string code, string title)
    {
        return new PrimaryAccrualManifest(code, title, false, "", 0.0, PrimaryAccrualManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAccrualManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAccrualManifestStatus next)
    {
        switch (Status)
        {
            case PrimaryAccrualManifestStatus.Planned:
                return next == PrimaryAccrualManifestStatus.Cancelled;
            case PrimaryAccrualManifestStatus.Scheduled:
                return next == PrimaryAccrualManifestStatus.Completed;
            case PrimaryAccrualManifestStatus.InProgress:
                return next == PrimaryAccrualManifestStatus.Completed || next == PrimaryAccrualManifestStatus.Cancelled;
            case PrimaryAccrualManifestStatus.Completed:
                return next == PrimaryAccrualManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimaryAccrualManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary accrual manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public PrimaryAccrualManifest Copy()
    {
        return new PrimaryAccrualManifest(Code, Title, Archived, Currency, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
