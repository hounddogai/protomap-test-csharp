namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual warranty reservation is in its life cycle.</summary>
public enum ManualWarrantyReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual warranty reservation. Its code is unique within a tenant.</summary>
public sealed class ManualWarrantyReservation(string code, string label, double maximum, string reviewer, int version, int expiresAt, string phone, int createdAt, string managerId, double weight, string locale, ManualWarrantyReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public string Reviewer { get; set; } = reviewer;
    public int Version { get; set; } = version;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Phone { get; set; } = phone;
    public int CreatedAt { get; set; } = createdAt;
    public string ManagerId { get; set; } = managerId;
    public double Weight { get; set; } = weight;
    public string Locale { get; set; } = locale;
    public ManualWarrantyReservationStatus Status { get; set; } = status;

    /// <summary>Starts a manual warranty reservation in status created.</summary>
    public static ManualWarrantyReservation NewManualWarrantyReservation(string code, string label)
    {
        return new ManualWarrantyReservation(
            code,
            label,
            0.0,
            "",
            0,
            0,
            "",
            0,
            "",
            0.0,
            "",
            ManualWarrantyReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualWarrantyReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualWarrantyReservationStatus next)
    {
        switch (Status)
        {
            case ManualWarrantyReservationStatus.Created:
                return next == ManualWarrantyReservationStatus.Validated;
            case ManualWarrantyReservationStatus.Validated:
                return next == ManualWarrantyReservationStatus.Published || next == ManualWarrantyReservationStatus.Retired;
            case ManualWarrantyReservationStatus.Published:
                switch (next)
                {
                    case ManualWarrantyReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualWarrantyReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual warranty reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetValue()
    {
        return Math.Round((double)Version - (double)CreatedAt, 2);
    }

    public ManualWarrantyReservation Copy()
    {
        return new ManualWarrantyReservation(
            Code,
            Label,
            Maximum,
            Reviewer,
            Version,
            ExpiresAt,
            Phone,
            CreatedAt,
            ManagerId,
            Weight,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
