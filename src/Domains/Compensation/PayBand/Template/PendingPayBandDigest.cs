namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pay band digest is in its life cycle.</summary>
public enum PendingPayBandDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending pay band digest as the pay band team keeps it.</summary>
public sealed class PendingPayBandDigest(string code, string name, string label, string ownerId, int version, string channel, string checksum, PendingPayBandDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public int Version { get; set; } = version;
    public string Channel { get; set; } = channel;
    public string Checksum { get; set; } = checksum;
    public PendingPayBandDigestStatus Status { get; set; } = status;

    /// <summary>Starts a pending pay band digest in status requested.</summary>
    public static PendingPayBandDigest NewPendingPayBandDigest(string code, string name)
    {
        return new PendingPayBandDigest(code, name, "", "", 0, "", "", PendingPayBandDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPayBandDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPayBandDigestStatus next)
    {
        switch (Status)
        {
            case PendingPayBandDigestStatus.Requested:
                switch (next)
                {
                    case PendingPayBandDigestStatus.Granted:
                        return true;
                }
                return false;
            case PendingPayBandDigestStatus.Granted:
                return next == PendingPayBandDigestStatus.Expired;
            case PendingPayBandDigestStatus.Revoked:
                return next == PendingPayBandDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingPayBandDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pay band digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public PendingPayBandDigest Copy()
    {
        return new PendingPayBandDigest(Code, Name, Label, OwnerId, Version, Channel, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
