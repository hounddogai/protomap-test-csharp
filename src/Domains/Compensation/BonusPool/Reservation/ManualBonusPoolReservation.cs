namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual bonus pool reservation is in its life cycle.</summary>
public enum ManualBonusPoolReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a manual bonus pool reservation and its current status.</summary>
public sealed class ManualBonusPoolReservation(string code, string label, double maximum, string contactEmail, string category, string summary, Dictionary<string, string> attributes, string employeeId, double cost, string grade, ManualBonusPoolReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string ContactEmail { get; set; } = contactEmail;
    public string Category { get; set; } = category;
    public string Summary { get; set; } = summary;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string EmployeeId { get; set; } = employeeId;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public ManualBonusPoolReservationStatus Status { get; set; } = status;

    /// <summary>Starts a manual bonus pool reservation in status created.</summary>
    public static ManualBonusPoolReservation NewManualBonusPoolReservation(string code, string label)
    {
        return new ManualBonusPoolReservation(
            code,
            label,
            0.0,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            "",
            ManualBonusPoolReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBonusPoolReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBonusPoolReservationStatus next)
    {
        switch (Status)
        {
            case ManualBonusPoolReservationStatus.Created:
                return next == ManualBonusPoolReservationStatus.Published || next == ManualBonusPoolReservationStatus.Retired;
            case ManualBonusPoolReservationStatus.Validated:
                return next == ManualBonusPoolReservationStatus.Retired;
            case ManualBonusPoolReservationStatus.Published:
                return next == ManualBonusPoolReservationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualBonusPoolReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual bonus pool reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double NetVolume()
    {
        return Math.Round(Maximum * Cost, 2);
    }

    public ManualBonusPoolReservation Copy()
    {
        return new ManualBonusPoolReservation(
            Code,
            Label,
            Maximum,
            ContactEmail,
            Category,
            Summary,
            Attributes,
            EmployeeId,
            Cost,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
