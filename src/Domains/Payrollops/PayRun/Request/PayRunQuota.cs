namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run quota is in its life cycle.</summary>
public enum PayRunQuotaStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pay run quota. Its code is unique within a tenant.</summary>
public sealed class PayRunQuota(string code, string name, int expiresAt, string channel, int batchSize, double rating, string checksum, int revision, PayRunQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public int BatchSize { get; set; } = batchSize;
    public double Rating { get; set; } = rating;
    public string Checksum { get; set; } = checksum;
    public int Revision { get; set; } = revision;
    public PayRunQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pay run quota in status requested.</summary>
    public static PayRunQuota NewPayRunQuota(string code, string name)
    {
        return new PayRunQuota(code, name, 0, "", 0, 0.0, "", 0, PayRunQuotaStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunQuotaStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunQuotaStatus next)
    {
        switch (Status)
        {
            case PayRunQuotaStatus.Requested:
                return next == PayRunQuotaStatus.Revoked;
            case PayRunQuotaStatus.Granted:
                return next == PayRunQuotaStatus.Expired;
            case PayRunQuotaStatus.Revoked:
                return next == PayRunQuotaStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayRunQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round(Rating - (double)Revision, 2);
    }

    public PayRunQuota Copy()
    {
        return new PayRunQuota(Code, Name, ExpiresAt, Channel, BatchSize, Rating, Checksum, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
