namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg exception is in its life cycle.</summary>
public enum ReorgExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A reorg exception. Its code is unique within a tenant.</summary>
public sealed class ReorgException(string code, string label, string department, string notes, double maximum, string currency, double amount, int graceDays, ReorgExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public string Notes { get; set; } = notes;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public double Amount { get; set; } = amount;
    public int GraceDays { get; set; } = graceDays;
    public ReorgExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a reorg exception in status requested.</summary>
    public static ReorgException NewReorgException(string code, string label)
    {
        return new ReorgException(code, label, "", "", 0.0, "", 0.0, 0, ReorgExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgExceptionStatus next)
    {
        switch (Status)
        {
            case ReorgExceptionStatus.Requested:
                return next == ReorgExceptionStatus.Granted || next == ReorgExceptionStatus.Expired;
            case ReorgExceptionStatus.Granted:
                return next == ReorgExceptionStatus.Expired;
            case ReorgExceptionStatus.Revoked:
                return next == ReorgExceptionStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ReorgExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)GraceDays - Amount, 2);
    }

    public ReorgException Copy()
    {
        return new ReorgException(Code, Label, Department, Notes, Maximum, Currency, Amount, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
