namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run group is in its life cycle.</summary>
public enum PayRunGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One pay run group, with what the planners need to act on it.</summary>
public sealed class PayRunGroup(string code, string label, string reviewer, string contactEmail, string currency, int limit, double maximum, PayRunGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string ContactEmail { get; set; } = contactEmail;
    public string Currency { get; set; } = currency;
    public int Limit { get; set; } = limit;
    public double Maximum { get; set; } = maximum;
    public PayRunGroupStatus Status { get; set; } = status;

    /// <summary>Starts a pay run group in status requested.</summary>
    public static PayRunGroup NewPayRunGroup(string code, string label)
    {
        return new PayRunGroup(code, label, "", "", "", 0, 0.0, PayRunGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunGroupStatus next)
    {
        switch (Status)
        {
            case PayRunGroupStatus.Requested:
                return next == PayRunGroupStatus.Revoked;
            case PayRunGroupStatus.Granted:
                switch (next)
                {
                    case PayRunGroupStatus.Expired:
                        return true;
                }
                return false;
            case PayRunGroupStatus.Revoked:
                return next == PayRunGroupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayRunGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Maximum * (double)Limit, 2);
    }

    public PayRunGroup Copy()
    {
        return new PayRunGroup(Code, Label, Reviewer, ContactEmail, Currency, Limit, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
