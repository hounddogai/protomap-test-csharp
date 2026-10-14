namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered vendor schedule is in its life cycle.</summary>
public enum TieredVendorScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a tiered vendor schedule and its current status.</summary>
public sealed class TieredVendorSchedule(string code, string label, bool active, int endsAt, Dictionary<string, string> attributes, int revision, double amount, TieredVendorScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public int EndsAt { get; set; } = endsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Revision { get; set; } = revision;
    public double Amount { get; set; } = amount;
    public TieredVendorScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered vendor schedule in status planned.</summary>
    public static TieredVendorSchedule NewTieredVendorSchedule(string code, string label)
    {
        return new TieredVendorSchedule(
            code,
            label,
            false,
            0,
            new Dictionary<string, string>(),
            0,
            0.0,
            TieredVendorScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredVendorScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredVendorScheduleStatus next)
    {
        switch (Status)
        {
            case TieredVendorScheduleStatus.Planned:
                return next == TieredVendorScheduleStatus.Cancelled;
            case TieredVendorScheduleStatus.Scheduled:
                switch (next)
                {
                    case TieredVendorScheduleStatus.InProgress:
                    case TieredVendorScheduleStatus.Completed:
                        return true;
                }
                return false;
            case TieredVendorScheduleStatus.InProgress:
                return next == TieredVendorScheduleStatus.Completed;
            case TieredVendorScheduleStatus.Completed:
                switch (next)
                {
                    case TieredVendorScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredVendorScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered vendor schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double EffectiveVolume()
    {
        return Math.Round((double)Revision + Amount, 2);
    }

    public TieredVendorSchedule Copy()
    {
        return new TieredVendorSchedule(Code, Label, Active, EndsAt, Attributes, Revision, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
