namespace Acme.Workforce.Absence.Accrual.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual snapshot is in its life cycle.</summary>
public enum AccrualSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A accrual snapshot. Its code is unique within a tenant.</summary>
public sealed class AccrualSnapshot(string code, string title, int endsAt, double discount, string contactEmail, AccrualSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public double Discount { get; set; } = discount;
    public string ContactEmail { get; set; } = contactEmail;
    public AccrualSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a accrual snapshot in status requested.</summary>
    public static AccrualSnapshot NewAccrualSnapshot(string code, string title)
    {
        return new AccrualSnapshot(code, title, 0, 0.0, "", AccrualSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualSnapshotStatus next)
    {
        switch (Status)
        {
            case AccrualSnapshotStatus.Requested:
                return next == AccrualSnapshotStatus.Revoked || next == AccrualSnapshotStatus.Expired;
            case AccrualSnapshotStatus.Granted:
                switch (next)
                {
                    case AccrualSnapshotStatus.Revoked:
                    case AccrualSnapshotStatus.Expired:
                        return true;
                }
                return false;
            case AccrualSnapshotStatus.Revoked:
                return next == AccrualSnapshotStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AccrualSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount * (double)EndsAt, 2);
    }

    public AccrualSnapshot Copy()
    {
        return new AccrualSnapshot(Code, Title, EndsAt, Discount, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
