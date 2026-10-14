namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility slot is in its life cycle.</summary>
public enum MobilitySlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A mobility slot. Its code is unique within a tenant.</summary>
public sealed class MobilitySlot(string code, string title, int updatedAt, int tenureMonths, string skill, int startsAt, double weight, MobilitySlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Skill { get; set; } = skill;
    public int StartsAt { get; set; } = startsAt;
    public double Weight { get; set; } = weight;
    public MobilitySlotStatus Status { get; set; } = status;

    /// <summary>Starts a mobility slot in status draft.</summary>
    public static MobilitySlot NewMobilitySlot(string code, string title)
    {
        return new MobilitySlot(code, title, 0, 0, "", 0, 0.0, MobilitySlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilitySlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilitySlotStatus next)
    {
        switch (Status)
        {
            case MobilitySlotStatus.Draft:
                switch (next)
                {
                    case MobilitySlotStatus.Pending:
                        return true;
                }
                return false;
            case MobilitySlotStatus.Pending:
                return next == MobilitySlotStatus.Suspended;
            case MobilitySlotStatus.Active:
                switch (next)
                {
                    case MobilitySlotStatus.Suspended:
                        return true;
                }
                return false;
            case MobilitySlotStatus.Suspended:
                return next == MobilitySlotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(MobilitySlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)TenureMonths + (double)StartsAt, 2);
    }

    public MobilitySlot Copy()
    {
        return new MobilitySlot(Code, Title, UpdatedAt, TenureMonths, Skill, StartsAt, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
