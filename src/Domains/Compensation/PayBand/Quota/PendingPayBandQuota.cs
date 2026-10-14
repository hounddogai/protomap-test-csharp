namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pay band quota is in its life cycle.</summary>
public enum PendingPayBandQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending pay band quota. Its code is unique within a tenant.</summary>
public sealed class PendingPayBandQuota(string code, string name, double weight, double minimum, string source, PendingPayBandQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public double Minimum { get; set; } = minimum;
    public string Source { get; set; } = source;
    public PendingPayBandQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pending pay band quota in status requested.</summary>
    public static PendingPayBandQuota NewPendingPayBandQuota(string code, string name)
    {
        return new PendingPayBandQuota(code, name, 0.0, 0.0, "", PendingPayBandQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPayBandQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPayBandQuotaStatus next)
    {
        switch (Status)
        {
            case PendingPayBandQuotaStatus.Requested:
                switch (next)
                {
                    case PendingPayBandQuotaStatus.Revoked:
                        return true;
                }
                return false;
            case PendingPayBandQuotaStatus.Granted:
                switch (next)
                {
                    case PendingPayBandQuotaStatus.Revoked:
                    case PendingPayBandQuotaStatus.Expired:
                        return true;
                }
                return false;
            case PendingPayBandQuotaStatus.Revoked:
                return next == PendingPayBandQuotaStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingPayBandQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pay band quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetLoad()
    {
        return Math.Round(Minimum - Weight, 2);
    }

    public PendingPayBandQuota Copy()
    {
        return new PendingPayBandQuota(Code, Name, Weight, Minimum, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
