namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending merit cycle batch is in its life cycle.</summary>
public enum PendingMeritCycleBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending merit cycle batch. Its code is unique within a tenant.</summary>
public sealed class PendingMeritCycleBatch(string code, string title, bool pinned, double score, string description, int updatedAt, int startsAt, string region, PendingMeritCycleBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public double Score { get; set; } = score;
    public string Description { get; set; } = description;
    public int UpdatedAt { get; set; } = updatedAt;
    public int StartsAt { get; set; } = startsAt;
    public string Region { get; set; } = region;
    public PendingMeritCycleBatchStatus Status { get; set; } = status;

    /// <summary>Starts a pending merit cycle batch in status requested.</summary>
    public static PendingMeritCycleBatch NewPendingMeritCycleBatch(string code, string title)
    {
        return new PendingMeritCycleBatch(
            code,
            title,
            false,
            0.0,
            "",
            0,
            0,
            "",
            PendingMeritCycleBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingMeritCycleBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingMeritCycleBatchStatus next)
    {
        switch (Status)
        {
            case PendingMeritCycleBatchStatus.Requested:
                switch (next)
                {
                    case PendingMeritCycleBatchStatus.Expired:
                        return true;
                }
                return false;
            case PendingMeritCycleBatchStatus.Granted:
                return next == PendingMeritCycleBatchStatus.Revoked || next == PendingMeritCycleBatchStatus.Expired;
            case PendingMeritCycleBatchStatus.Revoked:
                return next == PendingMeritCycleBatchStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingMeritCycleBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending merit cycle batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetTotal()
    {
        return Math.Round((double)UpdatedAt + (double)StartsAt, 2);
    }

    public PendingMeritCycleBatch Copy()
    {
        return new PendingMeritCycleBatch(
            Code,
            Title,
            Pinned,
            Score,
            Description,
            UpdatedAt,
            StartsAt,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
