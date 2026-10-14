namespace Acme.Workforce.Payrollops.PayRun.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run schedule is in its life cycle.</summary>
public enum PayRunScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a pay run schedule and its current status.</summary>
public sealed class PayRunSchedule(string code, string name, double rating, string locale, double hours, string badge, double threshold, int expiresAt, PayRunScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string Locale { get; set; } = locale;
    public double Hours { get; set; } = hours;
    public string Badge { get; set; } = badge;
    public double Threshold { get; set; } = threshold;
    public int ExpiresAt { get; set; } = expiresAt;
    public PayRunScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a pay run schedule in status requested.</summary>
    public static PayRunSchedule NewPayRunSchedule(string code, string name)
    {
        return new PayRunSchedule(code, name, 0.0, "", 0.0, "", 0.0, 0, PayRunScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunScheduleStatus next)
    {
        switch (Status)
        {
            case PayRunScheduleStatus.Requested:
                switch (next)
                {
                    case PayRunScheduleStatus.Revoked:
                        return true;
                }
                return false;
            case PayRunScheduleStatus.Granted:
                return next == PayRunScheduleStatus.Revoked || next == PayRunScheduleStatus.Expired;
            case PayRunScheduleStatus.Revoked:
                return next == PayRunScheduleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayRunScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Hours - Threshold, 2);
    }

    public PayRunSchedule Copy()
    {
        return new PayRunSchedule(Code, Name, Rating, Locale, Hours, Badge, Threshold, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
