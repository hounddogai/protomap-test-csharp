namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling recognition slot is in its life cycle.</summary>
public enum RollingRecognitionSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rolling recognition slot and its current status.</summary>
public sealed class RollingRecognitionSlot(string code, string label, string phone, int quantity, string currency, string description, bool pinned, int leaveDays, int version, string country, RollingRecognitionSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public int Quantity { get; set; } = quantity;
    public string Currency { get; set; } = currency;
    public string Description { get; set; } = description;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public int Version { get; set; } = version;
    public string Country { get; set; } = country;
    public RollingRecognitionSlotStatus Status { get; set; } = status;

    /// <summary>Starts a rolling recognition slot in status open.</summary>
    public static RollingRecognitionSlot NewRollingRecognitionSlot(string code, string label)
    {
        return new RollingRecognitionSlot(
            code,
            label,
            "",
            0,
            "",
            "",
            false,
            0,
            0,
            "",
            RollingRecognitionSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRecognitionSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRecognitionSlotStatus next)
    {
        switch (Status)
        {
            case RollingRecognitionSlotStatus.Open:
                switch (next)
                {
                    case RollingRecognitionSlotStatus.InReview:
                        return true;
                }
                return false;
            case RollingRecognitionSlotStatus.InReview:
                switch (next)
                {
                    case RollingRecognitionSlotStatus.Archived:
                        return true;
                }
                return false;
            case RollingRecognitionSlotStatus.Resolved:
                switch (next)
                {
                    case RollingRecognitionSlotStatus.Reopened:
                    case RollingRecognitionSlotStatus.Archived:
                        return true;
                }
                return false;
            case RollingRecognitionSlotStatus.Reopened:
                return next == RollingRecognitionSlotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingRecognitionSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling recognition slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Version + (double)Quantity, 2);
    }

    public RollingRecognitionSlot Copy()
    {
        return new RollingRecognitionSlot(
            Code,
            Label,
            Phone,
            Quantity,
            Currency,
            Description,
            Pinned,
            LeaveDays,
            Version,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
