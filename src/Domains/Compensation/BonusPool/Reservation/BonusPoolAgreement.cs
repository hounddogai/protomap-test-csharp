namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool agreement is in its life cycle.</summary>
public enum BonusPoolAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A bonus pool agreement. Its code is unique within a tenant.</summary>
public sealed class BonusPoolAgreement(string code, string title, int updatedAt, double hours, string description, string grade, int capacity, string currency, string locale, string label, BonusPoolAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Hours { get; set; } = hours;
    public string Description { get; set; } = description;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public string Locale { get; set; } = locale;
    public string Label { get; set; } = label;
    public BonusPoolAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool agreement in status idle.</summary>
    public static BonusPoolAgreement NewBonusPoolAgreement(string code, string title)
    {
        return new BonusPoolAgreement(code, title, 0, 0.0, "", "", 0, "", "", "", BonusPoolAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolAgreementStatus next)
    {
        switch (Status)
        {
            case BonusPoolAgreementStatus.Idle:
                return next == BonusPoolAgreementStatus.Ready || next == BonusPoolAgreementStatus.Stopped;
            case BonusPoolAgreementStatus.Warming:
                return next == BonusPoolAgreementStatus.Ready || next == BonusPoolAgreementStatus.Draining;
            case BonusPoolAgreementStatus.Ready:
                return next == BonusPoolAgreementStatus.Draining || next == BonusPoolAgreementStatus.Stopped;
            case BonusPoolAgreementStatus.Draining:
                switch (next)
                {
                    case BonusPoolAgreementStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool agreement"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Hours - (double)UpdatedAt, 2);
    }

    public BonusPoolAgreement Copy()
    {
        return new BonusPoolAgreement(
            Code,
            Title,
            UpdatedAt,
            Hours,
            Description,
            Grade,
            Capacity,
            Currency,
            Locale,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
