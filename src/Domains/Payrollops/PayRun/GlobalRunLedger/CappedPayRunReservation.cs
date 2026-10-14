namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pay run reservation is in its life cycle.</summary>
public enum CappedPayRunReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One capped pay run reservation, with what the planners need to act on it.</summary>
public sealed class CappedPayRunReservation(string code, string title, double maximum, Dictionary<string, string> attributes, int batchSize, string summary, List<string> tags, int quantity, string region, double floorValue, int graceDays, CappedPayRunReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int BatchSize { get; set; } = batchSize;
    public string Summary { get; set; } = summary;
    public List<string> Tags { get; set; } = tags;
    public int Quantity { get; set; } = quantity;
    public string Region { get; set; } = region;
    public double FloorValue { get; set; } = floorValue;
    public int GraceDays { get; set; } = graceDays;
    public CappedPayRunReservationStatus Status { get; set; } = status;

    /// <summary>Starts a capped pay run reservation in status created.</summary>
    public static CappedPayRunReservation NewCappedPayRunReservation(string code, string title)
    {
        return new CappedPayRunReservation(
            code,
            title,
            0.0,
            new Dictionary<string, string>(),
            0,
            "",
            new List<string>(),
            0,
            "",
            0.0,
            0,
            CappedPayRunReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPayRunReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPayRunReservationStatus next)
    {
        switch (Status)
        {
            case CappedPayRunReservationStatus.Created:
                return next == CappedPayRunReservationStatus.Validated;
            case CappedPayRunReservationStatus.Validated:
                return next == CappedPayRunReservationStatus.Published;
            case CappedPayRunReservationStatus.Published:
                switch (next)
                {
                    case CappedPayRunReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPayRunReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pay run reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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

    public double AdjustedExposure()
    {
        return Math.Round((double)GraceDays - (double)BatchSize, 2);
    }

    public CappedPayRunReservation Copy()
    {
        return new CappedPayRunReservation(
            Code,
            Title,
            Maximum,
            Attributes,
            BatchSize,
            Summary,
            Tags,
            Quantity,
            Region,
            FloorValue,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
