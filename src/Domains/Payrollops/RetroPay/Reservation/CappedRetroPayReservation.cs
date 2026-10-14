namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped retro pay reservation is in its life cycle.</summary>
public enum CappedRetroPayReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped retro pay reservation as the retro pay team keeps it.</summary>
public sealed class CappedRetroPayReservation(string code, string name, bool active, double hours, Dictionary<string, string> attributes, List<string> tags, int expiresAt, string notes, double ratio, string employeeId, string jobCode, CappedRetroPayReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public double Hours { get; set; } = hours;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public List<string> Tags { get; set; } = tags;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public string EmployeeId { get; set; } = employeeId;
    public string JobCode { get; set; } = jobCode;
    public CappedRetroPayReservationStatus Status { get; set; } = status;

    /// <summary>Starts a capped retro pay reservation in status draft.</summary>
    public static CappedRetroPayReservation NewCappedRetroPayReservation(string code, string name)
    {
        return new CappedRetroPayReservation(
            code,
            name,
            false,
            0.0,
            new Dictionary<string, string>(),
            new List<string>(),
            0,
            "",
            0.0,
            "",
            "",
            CappedRetroPayReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRetroPayReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRetroPayReservationStatus next)
    {
        switch (Status)
        {
            case CappedRetroPayReservationStatus.Draft:
                return next == CappedRetroPayReservationStatus.Active || next == CappedRetroPayReservationStatus.Closed;
            case CappedRetroPayReservationStatus.Pending:
                switch (next)
                {
                    case CappedRetroPayReservationStatus.Closed:
                        return true;
                }
                return false;
            case CappedRetroPayReservationStatus.Active:
                return next == CappedRetroPayReservationStatus.Closed;
            case CappedRetroPayReservationStatus.Suspended:
                switch (next)
                {
                    case CappedRetroPayReservationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedRetroPayReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped retro pay reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double AdjustedVolume()
    {
        return Math.Round(Hours - Ratio, 2);
    }

    public CappedRetroPayReservation Copy()
    {
        return new CappedRetroPayReservation(
            Code,
            Name,
            Active,
            Hours,
            Attributes,
            Tags,
            ExpiresAt,
            Notes,
            Ratio,
            EmployeeId,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
