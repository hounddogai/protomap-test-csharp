namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback recognition slot is in its life cycle.</summary>
public enum FallbackRecognitionSlotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback recognition slot as the recognition team keeps it.</summary>
public sealed class FallbackRecognitionSlot(string code, string name, string locale, string ownerId, double rate, string managerId, string channel, int tenureMonths, string skill, FallbackRecognitionSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string OwnerId { get; set; } = ownerId;
    public double Rate { get; set; } = rate;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Skill { get; set; } = skill;
    public FallbackRecognitionSlotStatus Status { get; set; } = status;

    /// <summary>Starts a fallback recognition slot in status planned.</summary>
    public static FallbackRecognitionSlot NewFallbackRecognitionSlot(string code, string name)
    {
        return new FallbackRecognitionSlot(
            code,
            name,
            "",
            "",
            0.0,
            "",
            "",
            0,
            "",
            FallbackRecognitionSlotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRecognitionSlotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRecognitionSlotStatus next)
    {
        switch (Status)
        {
            case FallbackRecognitionSlotStatus.Planned:
                return next == FallbackRecognitionSlotStatus.Completed;
            case FallbackRecognitionSlotStatus.Scheduled:
                switch (next)
                {
                    case FallbackRecognitionSlotStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackRecognitionSlotStatus.InProgress:
                return next == FallbackRecognitionSlotStatus.Completed || next == FallbackRecognitionSlotStatus.Cancelled;
            case FallbackRecognitionSlotStatus.Completed:
                return next == FallbackRecognitionSlotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackRecognitionSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback recognition slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Rate - (double)TenureMonths, 2);
    }

    public FallbackRecognitionSlot Copy()
    {
        return new FallbackRecognitionSlot(
            Code,
            Name,
            Locale,
            OwnerId,
            Rate,
            ManagerId,
            Channel,
            TenureMonths,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
