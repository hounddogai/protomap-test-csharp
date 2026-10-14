namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped offboarding slot is in its life cycle.</summary>
public enum CappedOffboardingSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a capped offboarding slot and its current status.</summary>
public sealed class CappedOffboardingSlot(string code, string title, string notes, string phone, int batchSize, int expiresAt, string locale, string name, string location, string badge, CappedOffboardingSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public string Phone { get; set; } = phone;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Locale { get; set; } = locale;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public string Badge { get; set; } = badge;
    public CappedOffboardingSlotStatus Status { get; set; } = status;

    /// <summary>Starts a capped offboarding slot in status new.</summary>
    public static CappedOffboardingSlot NewCappedOffboardingSlot(string code, string title)
    {
        return new CappedOffboardingSlot(code, title, "", "", 0, 0, "", "", "", "", CappedOffboardingSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedOffboardingSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedOffboardingSlotStatus next)
    {
        switch (Status)
        {
            case CappedOffboardingSlotStatus.New:
                switch (next)
                {
                    case CappedOffboardingSlotStatus.Running:
                    case CappedOffboardingSlotStatus.Done:
                        return true;
                }
                return false;
            case CappedOffboardingSlotStatus.Queued:
                return next == CappedOffboardingSlotStatus.Running || next == CappedOffboardingSlotStatus.Done;
            case CappedOffboardingSlotStatus.Running:
                return next == CappedOffboardingSlotStatus.Done || next == CappedOffboardingSlotStatus.Failed;
            case CappedOffboardingSlotStatus.Done:
                switch (next)
                {
                    case CappedOffboardingSlotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedOffboardingSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped offboarding slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)BatchSize * (double)ExpiresAt, 2);
    }

    public CappedOffboardingSlot Copy()
    {
        return new CappedOffboardingSlot(
            Code,
            Title,
            Notes,
            Phone,
            BatchSize,
            ExpiresAt,
            Locale,
            Name,
            Location,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
