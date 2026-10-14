namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped laptop record is in its life cycle.</summary>
public enum CappedLaptopRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped laptop record as the laptop team keeps it.</summary>
public sealed class CappedLaptopRecord(string code, string name, string badge, double fte, int expiresAt, double discount, bool visible, int leaveDays, CappedLaptopRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public double Fte { get; set; } = fte;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Discount { get; set; } = discount;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public CappedLaptopRecordStatus Status { get; set; } = status;

    /// <summary>Starts a capped laptop record in status draft.</summary>
    public static CappedLaptopRecord NewCappedLaptopRecord(string code, string name)
    {
        return new CappedLaptopRecord(code, name, "", 0.0, 0, 0.0, false, 0, CappedLaptopRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLaptopRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLaptopRecordStatus next)
    {
        switch (Status)
        {
            case CappedLaptopRecordStatus.Draft:
                switch (next)
                {
                    case CappedLaptopRecordStatus.Active:
                    case CappedLaptopRecordStatus.Closed:
                        return true;
                }
                return false;
            case CappedLaptopRecordStatus.Pending:
                switch (next)
                {
                    case CappedLaptopRecordStatus.Suspended:
                        return true;
                }
                return false;
            case CappedLaptopRecordStatus.Active:
                return next == CappedLaptopRecordStatus.Suspended || next == CappedLaptopRecordStatus.Closed;
            case CappedLaptopRecordStatus.Suspended:
                return next == CappedLaptopRecordStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedLaptopRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped laptop record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)ExpiresAt - Discount, 2);
    }

    public CappedLaptopRecord Copy()
    {
        return new CappedLaptopRecord(Code, Name, Badge, Fte, ExpiresAt, Discount, Visible, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
