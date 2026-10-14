namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat audit is in its life cycle.</summary>
public enum LicenseSeatAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A license seat audit as the license seat team keeps it.</summary>
public sealed class LicenseSeatAudit(string code, string title, string externalRef, int capacity, bool active, string segment, string region, double discount, int retries, int quantity, LicenseSeatAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public int Capacity { get; set; } = capacity;
    public bool Active { get; set; } = active;
    public string Segment { get; set; } = segment;
    public string Region { get; set; } = region;
    public double Discount { get; set; } = discount;
    public int Retries { get; set; } = retries;
    public int Quantity { get; set; } = quantity;
    public LicenseSeatAuditStatus Status { get; set; } = status;

    /// <summary>Starts a license seat audit in status requested.</summary>
    public static LicenseSeatAudit NewLicenseSeatAudit(string code, string title)
    {
        return new LicenseSeatAudit(code, title, "", 0, false, "", "", 0.0, 0, 0, LicenseSeatAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatAuditStatus next)
    {
        switch (Status)
        {
            case LicenseSeatAuditStatus.Requested:
                return next == LicenseSeatAuditStatus.Expired;
            case LicenseSeatAuditStatus.Granted:
                switch (next)
                {
                    case LicenseSeatAuditStatus.Expired:
                        return true;
                }
                return false;
            case LicenseSeatAuditStatus.Revoked:
                switch (next)
                {
                    case LicenseSeatAuditStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Quantity * (double)Capacity, 2);
    }

    public LicenseSeatAudit Copy()
    {
        return new LicenseSeatAudit(
            Code,
            Title,
            ExternalRef,
            Capacity,
            Active,
            Segment,
            Region,
            Discount,
            Retries,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
