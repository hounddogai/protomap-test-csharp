namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending holiday quota is in its life cycle.</summary>
public enum PendingHolidayQuotaStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pending holiday quota, with what the planners need to act on it.</summary>
public sealed class PendingHolidayQuota(string code, string title, string timezone, string contactEmail, int graceDays, int endsAt, double budget, int leaveDays, double fte, bool visible, double amount, PendingHolidayQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string ContactEmail { get; set; } = contactEmail;
    public int GraceDays { get; set; } = graceDays;
    public int EndsAt { get; set; } = endsAt;
    public double Budget { get; set; } = budget;
    public int LeaveDays { get; set; } = leaveDays;
    public double Fte { get; set; } = fte;
    public bool Visible { get; set; } = visible;
    public double Amount { get; set; } = amount;
    public PendingHolidayQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a pending holiday quota in status idle.</summary>
    public static PendingHolidayQuota NewPendingHolidayQuota(string code, string title)
    {
        return new PendingHolidayQuota(
            code,
            title,
            "",
            "",
            0,
            0,
            0.0,
            0,
            0.0,
            false,
            0.0,
            PendingHolidayQuotaStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingHolidayQuotaStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingHolidayQuotaStatus next)
    {
        switch (Status)
        {
            case PendingHolidayQuotaStatus.Idle:
                return next == PendingHolidayQuotaStatus.Draining || next == PendingHolidayQuotaStatus.Stopped;
            case PendingHolidayQuotaStatus.Warming:
                return next == PendingHolidayQuotaStatus.Draining;
            case PendingHolidayQuotaStatus.Ready:
                switch (next)
                {
                    case PendingHolidayQuotaStatus.Draining:
                        return true;
                }
                return false;
            case PendingHolidayQuotaStatus.Draining:
                return next == PendingHolidayQuotaStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PendingHolidayQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending holiday quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)LeaveDays + (double)GraceDays, 2);
    }

    public PendingHolidayQuota Copy()
    {
        return new PendingHolidayQuota(
            Code,
            Title,
            Timezone,
            ContactEmail,
            GraceDays,
            EndsAt,
            Budget,
            LeaveDays,
            Fte,
            Visible,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
