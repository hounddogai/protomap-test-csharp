namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual preference is in its life cycle.</summary>
public enum AccrualPreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A accrual preference as the accrual team keeps it.</summary>
public sealed class AccrualPreference(string code, string title, double floorValue, int updatedAt, double maximum, string name, string department, string contactEmail, double rating, string locale, AccrualPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rating { get; set; } = rating;
    public string Locale { get; set; } = locale;
    public AccrualPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a accrual preference in status requested.</summary>
    public static AccrualPreference NewAccrualPreference(string code, string title)
    {
        return new AccrualPreference(
            code,
            title,
            0.0,
            0,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            AccrualPreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualPreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualPreferenceStatus next)
    {
        switch (Status)
        {
            case AccrualPreferenceStatus.Requested:
                switch (next)
                {
                    case AccrualPreferenceStatus.Revoked:
                        return true;
                }
                return false;
            case AccrualPreferenceStatus.Granted:
                return next == AccrualPreferenceStatus.Revoked;
            case AccrualPreferenceStatus.Revoked:
                return next == AccrualPreferenceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AccrualPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Rating - Maximum, 2);
    }

    public AccrualPreference Copy()
    {
        return new AccrualPreference(
            Code,
            Title,
            FloorValue,
            UpdatedAt,
            Maximum,
            Name,
            Department,
            ContactEmail,
            Rating,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
