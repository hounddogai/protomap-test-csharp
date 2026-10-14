namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual manifest is in its life cycle.</summary>
public enum AccrualManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A accrual manifest as the accrual team keeps it.</summary>
public sealed class AccrualManifest(string code, string label, int retries, int limit, string timezone, string ownerId, AccrualManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public int Limit { get; set; } = limit;
    public string Timezone { get; set; } = timezone;
    public string OwnerId { get; set; } = ownerId;
    public AccrualManifestStatus Status { get; set; } = status;

    /// <summary>Starts a accrual manifest in status draft.</summary>
    public static AccrualManifest NewAccrualManifest(string code, string label)
    {
        return new AccrualManifest(code, label, 0, 0, "", "", AccrualManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualManifestStatus next)
    {
        switch (Status)
        {
            case AccrualManifestStatus.Draft:
                switch (next)
                {
                    case AccrualManifestStatus.Active:
                        return true;
                }
                return false;
            case AccrualManifestStatus.Pending:
                return next == AccrualManifestStatus.Active || next == AccrualManifestStatus.Suspended;
            case AccrualManifestStatus.Active:
                return next == AccrualManifestStatus.Suspended;
            case AccrualManifestStatus.Suspended:
                return next == AccrualManifestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AccrualManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Limit + (double)Retries, 2);
    }

    public AccrualManifest Copy()
    {
        return new AccrualManifest(Code, Label, Retries, Limit, Timezone, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
