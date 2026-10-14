namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global span slot is in its life cycle.</summary>
public enum GlobalSpanSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A global span slot. Its code is unique within a tenant.</summary>
public sealed class GlobalSpanSlot(string code, string title, int revision, string location, string channel, double discount, string managerId, int endsAt, int startsAt, GlobalSpanSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Location { get; set; } = location;
    public string Channel { get; set; } = channel;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public int EndsAt { get; set; } = endsAt;
    public int StartsAt { get; set; } = startsAt;
    public GlobalSpanSlotStatus Status { get; set; } = status;

    /// <summary>Starts a global span slot in status proposed.</summary>
    public static GlobalSpanSlot NewGlobalSpanSlot(string code, string title)
    {
        return new GlobalSpanSlot(code, title, 0, "", "", 0.0, "", 0, 0, GlobalSpanSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalSpanSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalSpanSlotStatus next)
    {
        switch (Status)
        {
            case GlobalSpanSlotStatus.Proposed:
                return next == GlobalSpanSlotStatus.Approved || next == GlobalSpanSlotStatus.Withdrawn;
            case GlobalSpanSlotStatus.Approved:
                return next == GlobalSpanSlotStatus.Withdrawn;
            case GlobalSpanSlotStatus.Rejected:
                return next == GlobalSpanSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GlobalSpanSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global span slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"global span slot"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)StartsAt + (double)Revision, 2);
    }

    public GlobalSpanSlot Copy()
    {
        return new GlobalSpanSlot(
            Code,
            Title,
            Revision,
            Location,
            Channel,
            Discount,
            ManagerId,
            EndsAt,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
