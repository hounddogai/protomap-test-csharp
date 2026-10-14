namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount slot is in its life cycle.</summary>
public enum HeadcountSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A headcount slot as the headcount team keeps it.</summary>
public sealed class HeadcountSlot(string code, string title, double amount, double maximum, Dictionary<string, string> attributes, int graceDays, int durationMinutes, HeadcountSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public double Maximum { get; set; } = maximum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int GraceDays { get; set; } = graceDays;
    public int DurationMinutes { get; set; } = durationMinutes;
    public HeadcountSlotStatus Status { get; set; } = status;

    /// <summary>Starts a headcount slot in status requested.</summary>
    public static HeadcountSlot NewHeadcountSlot(string code, string title)
    {
        return new HeadcountSlot(
            code,
            title,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            0,
            0,
            HeadcountSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountSlotStatus next)
    {
        switch (Status)
        {
            case HeadcountSlotStatus.Requested:
                return next == HeadcountSlotStatus.Granted || next == HeadcountSlotStatus.Revoked;
            case HeadcountSlotStatus.Granted:
                return next == HeadcountSlotStatus.Expired;
            case HeadcountSlotStatus.Revoked:
                switch (next)
                {
                    case HeadcountSlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)DurationMinutes + (double)GraceDays, 2);
    }

    public HeadcountSlot Copy()
    {
        return new HeadcountSlot(Code, Title, Amount, Maximum, Attributes, GraceDays, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
