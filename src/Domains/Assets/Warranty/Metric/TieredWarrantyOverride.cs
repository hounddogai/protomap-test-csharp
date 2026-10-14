namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered warranty override is in its life cycle.</summary>
public enum TieredWarrantyOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a tiered warranty override and its current status.</summary>
public sealed class TieredWarrantyOverride(string code, string title, bool pinned, int leaveDays, double rate, int version, int createdAt, string managerId, double score, string source, TieredWarrantyOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rate { get; set; } = rate;
    public int Version { get; set; } = version;
    public int CreatedAt { get; set; } = createdAt;
    public string ManagerId { get; set; } = managerId;
    public double Score { get; set; } = score;
    public string Source { get; set; } = source;
    public TieredWarrantyOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a tiered warranty override in status proposed.</summary>
    public static TieredWarrantyOverride NewTieredWarrantyOverride(string code, string title)
    {
        return new TieredWarrantyOverride(
            code,
            title,
            false,
            0,
            0.0,
            0,
            0,
            "",
            0.0,
            "",
            TieredWarrantyOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredWarrantyOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredWarrantyOverrideStatus next)
    {
        switch (Status)
        {
            case TieredWarrantyOverrideStatus.Proposed:
                return next == TieredWarrantyOverrideStatus.Approved || next == TieredWarrantyOverrideStatus.Rejected;
            case TieredWarrantyOverrideStatus.Approved:
                return next == TieredWarrantyOverrideStatus.Rejected || next == TieredWarrantyOverrideStatus.Withdrawn;
            case TieredWarrantyOverrideStatus.Rejected:
                return next == TieredWarrantyOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredWarrantyOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered warranty override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"tiered warranty override"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Score + (double)Version, 2);
    }

    public TieredWarrantyOverride Copy()
    {
        return new TieredWarrantyOverride(
            Code,
            Title,
            Pinned,
            LeaveDays,
            Rate,
            Version,
            CreatedAt,
            ManagerId,
            Score,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
