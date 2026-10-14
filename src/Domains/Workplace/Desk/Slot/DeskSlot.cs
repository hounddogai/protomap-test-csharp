namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk slot is in its life cycle.</summary>
public enum DeskSlotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One desk slot, with what the planners need to act on it.</summary>
public sealed class DeskSlot(string code, string label, bool pinned, string country, string contactEmail, string channel, int updatedAt, string region, string managerId, double rate, bool active, DeskSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Country { get; set; } = country;
    public string ContactEmail { get; set; } = contactEmail;
    public string Channel { get; set; } = channel;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Region { get; set; } = region;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public bool Active { get; set; } = active;
    public DeskSlotStatus Status { get; set; } = status;

    /// <summary>Starts a desk slot in status idle.</summary>
    public static DeskSlot NewDeskSlot(string code, string label)
    {
        return new DeskSlot(code, label, false, "", "", "", 0, "", "", 0.0, false, DeskSlotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskSlotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskSlotStatus next)
    {
        switch (Status)
        {
            case DeskSlotStatus.Idle:
                switch (next)
                {
                    case DeskSlotStatus.Warming:
                    case DeskSlotStatus.Ready:
                        return true;
                }
                return false;
            case DeskSlotStatus.Warming:
                switch (next)
                {
                    case DeskSlotStatus.Ready:
                    case DeskSlotStatus.Draining:
                        return true;
                }
                return false;
            case DeskSlotStatus.Ready:
                switch (next)
                {
                    case DeskSlotStatus.Draining:
                    case DeskSlotStatus.Stopped:
                        return true;
                }
                return false;
            case DeskSlotStatus.Draining:
                switch (next)
                {
                    case DeskSlotStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"desk slot"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)UpdatedAt - Rate, 2);
    }

    public DeskSlot Copy()
    {
        return new DeskSlot(
            Code,
            Label,
            Pinned,
            Country,
            ContactEmail,
            Channel,
            UpdatedAt,
            Region,
            ManagerId,
            Rate,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
