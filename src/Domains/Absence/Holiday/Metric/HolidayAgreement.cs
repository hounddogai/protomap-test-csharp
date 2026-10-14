namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday agreement is in its life cycle.</summary>
public enum HolidayAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A holiday agreement as the holiday team keeps it.</summary>
public sealed class HolidayAgreement(string code, string name, double cost, int endsAt, string contactEmail, double ratio, int expiresAt, double threshold, HolidayAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public int EndsAt { get; set; } = endsAt;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ratio { get; set; } = ratio;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Threshold { get; set; } = threshold;
    public HolidayAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a holiday agreement in status proposed.</summary>
    public static HolidayAgreement NewHolidayAgreement(string code, string name)
    {
        return new HolidayAgreement(code, name, 0.0, 0, "", 0.0, 0, 0.0, HolidayAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayAgreementStatus next)
    {
        switch (Status)
        {
            case HolidayAgreementStatus.Proposed:
                switch (next)
                {
                    case HolidayAgreementStatus.Approved:
                    case HolidayAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
            case HolidayAgreementStatus.Approved:
                return next == HolidayAgreementStatus.Rejected || next == HolidayAgreementStatus.Withdrawn;
            case HolidayAgreementStatus.Rejected:
                return next == HolidayAgreementStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(HolidayAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedValue()
    {
        return Math.Round(Cost + Ratio, 2);
    }

    public HolidayAgreement Copy()
    {
        return new HolidayAgreement(Code, Name, Cost, EndsAt, ContactEmail, Ratio, ExpiresAt, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
