namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band override is in its life cycle.</summary>
public enum PayBandOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pay band override as the pay band team keeps it.</summary>
public sealed class PayBandOverride(string code, string title, Dictionary<string, string> attributes, string country, int leaveDays, double rate, int capacity, double floorValue, int revision, PayBandOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Country { get; set; } = country;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rate { get; set; } = rate;
    public int Capacity { get; set; } = capacity;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public PayBandOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a pay band override in status planned.</summary>
    public static PayBandOverride NewPayBandOverride(string code, string title)
    {
        return new PayBandOverride(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            0,
            0.0,
            0,
            PayBandOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandOverrideStatus next)
    {
        switch (Status)
        {
            case PayBandOverrideStatus.Planned:
                switch (next)
                {
                    case PayBandOverrideStatus.InProgress:
                    case PayBandOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case PayBandOverrideStatus.Scheduled:
                return next == PayBandOverrideStatus.InProgress;
            case PayBandOverrideStatus.InProgress:
                switch (next)
                {
                    case PayBandOverrideStatus.Completed:
                    case PayBandOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case PayBandOverrideStatus.Completed:
                return next == PayBandOverrideStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PayBandOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double AdjustedTotal()
    {
        return Math.Round((double)LeaveDays - FloorValue, 2);
    }

    public PayBandOverride Copy()
    {
        return new PayBandOverride(
            Code,
            Title,
            Attributes,
            Country,
            LeaveDays,
            Rate,
            Capacity,
            FloorValue,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
