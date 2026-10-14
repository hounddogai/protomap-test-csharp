namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop audit is in its life cycle.</summary>
public enum LaptopAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A laptop audit. Its code is unique within a tenant.</summary>
public sealed class LaptopAudit(string code, string name, bool archived, string notes, double margin, int startsAt, double cost, int sequence, string checksum, LaptopAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Notes { get; set; } = notes;
    public double Margin { get; set; } = margin;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public string Checksum { get; set; } = checksum;
    public LaptopAuditStatus Status { get; set; } = status;

    /// <summary>Starts a laptop audit in status requested.</summary>
    public static LaptopAudit NewLaptopAudit(string code, string name)
    {
        return new LaptopAudit(code, name, false, "", 0.0, 0, 0.0, 0, "", LaptopAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopAuditStatus next)
    {
        switch (Status)
        {
            case LaptopAuditStatus.Requested:
                switch (next)
                {
                    case LaptopAuditStatus.Granted:
                    case LaptopAuditStatus.Expired:
                        return true;
                }
                return false;
            case LaptopAuditStatus.Granted:
                switch (next)
                {
                    case LaptopAuditStatus.Revoked:
                        return true;
                }
                return false;
            case LaptopAuditStatus.Revoked:
                return next == LaptopAuditStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LaptopAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Margin - Cost, 2);
    }

    public LaptopAudit Copy()
    {
        return new LaptopAudit(Code, Name, Archived, Notes, Margin, StartsAt, Cost, Sequence, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
