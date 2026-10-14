namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global mobility record is in its life cycle.</summary>
public enum GlobalMobilityRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a global mobility record and its current status.</summary>
public sealed class GlobalMobilityRecord(string code, string title, double amount, int batchSize, string reviewer, string managerId, int expiresAt, GlobalMobilityRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public int BatchSize { get; set; } = batchSize;
    public string Reviewer { get; set; } = reviewer;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public GlobalMobilityRecordStatus Status { get; set; } = status;

    /// <summary>Starts a global mobility record in status requested.</summary>
    public static GlobalMobilityRecord NewGlobalMobilityRecord(string code, string title)
    {
        return new GlobalMobilityRecord(code, title, 0.0, 0, "", "", 0, GlobalMobilityRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalMobilityRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalMobilityRecordStatus next)
    {
        switch (Status)
        {
            case GlobalMobilityRecordStatus.Requested:
                switch (next)
                {
                    case GlobalMobilityRecordStatus.Revoked:
                        return true;
                }
                return false;
            case GlobalMobilityRecordStatus.Granted:
                return next == GlobalMobilityRecordStatus.Revoked || next == GlobalMobilityRecordStatus.Expired;
            case GlobalMobilityRecordStatus.Revoked:
                return next == GlobalMobilityRecordStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalMobilityRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global mobility record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Amount + (double)ExpiresAt, 2);
    }

    public GlobalMobilityRecord Copy()
    {
        return new GlobalMobilityRecord(Code, Title, Amount, BatchSize, Reviewer, ManagerId, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
