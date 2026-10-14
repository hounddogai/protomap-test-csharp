namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pay run agreement is in its life cycle.</summary>
public enum PendingPayRunAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One pending pay run agreement, with what the planners need to act on it.</summary>
public sealed class PendingPayRunAgreement(string code, string title, string ownerId, int revision, double amount, double cost, bool approved, PendingPayRunAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public int Revision { get; set; } = revision;
    public double Amount { get; set; } = amount;
    public double Cost { get; set; } = cost;
    public bool Approved { get; set; } = approved;
    public PendingPayRunAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pending pay run agreement in status draft.</summary>
    public static PendingPayRunAgreement NewPendingPayRunAgreement(string code, string title)
    {
        return new PendingPayRunAgreement(code, title, "", 0, 0.0, 0.0, false, PendingPayRunAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPayRunAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPayRunAgreementStatus next)
    {
        switch (Status)
        {
            case PendingPayRunAgreementStatus.Draft:
                return next == PendingPayRunAgreementStatus.Active || next == PendingPayRunAgreementStatus.Suspended;
            case PendingPayRunAgreementStatus.Pending:
                return next == PendingPayRunAgreementStatus.Suspended;
            case PendingPayRunAgreementStatus.Active:
                switch (next)
                {
                    case PendingPayRunAgreementStatus.Suspended:
                    case PendingPayRunAgreementStatus.Closed:
                        return true;
                }
                return false;
            case PendingPayRunAgreementStatus.Suspended:
                switch (next)
                {
                    case PendingPayRunAgreementStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPayRunAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pay run agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Cost * (double)Revision, 2);
    }

    public PendingPayRunAgreement Copy()
    {
        return new PendingPayRunAgreement(Code, Title, OwnerId, Revision, Amount, Cost, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
