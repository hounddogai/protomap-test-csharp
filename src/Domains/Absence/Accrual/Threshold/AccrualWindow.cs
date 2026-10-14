namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual window is in its life cycle.</summary>
public enum AccrualWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A accrual window. Its code is unique within a tenant.</summary>
public sealed class AccrualWindow(string code, string title, string locale, string reviewer, string managerId, double margin, string jobCode, int quantity, AccrualWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string Reviewer { get; set; } = reviewer;
    public string ManagerId { get; set; } = managerId;
    public double Margin { get; set; } = margin;
    public string JobCode { get; set; } = jobCode;
    public int Quantity { get; set; } = quantity;
    public AccrualWindowStatus Status { get; set; } = status;

    /// <summary>Starts a accrual window in status requested.</summary>
    public static AccrualWindow NewAccrualWindow(string code, string title)
    {
        return new AccrualWindow(code, title, "", "", "", 0.0, "", 0, AccrualWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualWindowStatus next)
    {
        switch (Status)
        {
            case AccrualWindowStatus.Requested:
                return next == AccrualWindowStatus.Revoked;
            case AccrualWindowStatus.Granted:
                return next == AccrualWindowStatus.Revoked || next == AccrualWindowStatus.Expired;
            case AccrualWindowStatus.Revoked:
                return next == AccrualWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AccrualWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetTotal()
    {
        return Math.Round(Margin + (double)Quantity, 2);
    }

    public AccrualWindow Copy()
    {
        return new AccrualWindow(Code, Title, Locale, Reviewer, ManagerId, Margin, JobCode, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
