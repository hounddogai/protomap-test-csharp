namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending adjustment agreement is in its life cycle.</summary>
public enum PendingAdjustmentAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending adjustment agreement as the adjustment team keeps it.</summary>
public sealed class PendingAdjustmentAgreement(string code, string title, string externalRef, string reviewer, bool visible, int limit, double ratio, double fte, PendingAdjustmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string Reviewer { get; set; } = reviewer;
    public bool Visible { get; set; } = visible;
    public int Limit { get; set; } = limit;
    public double Ratio { get; set; } = ratio;
    public double Fte { get; set; } = fte;
    public PendingAdjustmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pending adjustment agreement in status new.</summary>
    public static PendingAdjustmentAgreement NewPendingAdjustmentAgreement(string code, string title)
    {
        return new PendingAdjustmentAgreement(
            code,
            title,
            "",
            "",
            false,
            0,
            0.0,
            0.0,
            PendingAdjustmentAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAdjustmentAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAdjustmentAgreementStatus next)
    {
        switch (Status)
        {
            case PendingAdjustmentAgreementStatus.New:
                return next == PendingAdjustmentAgreementStatus.Done || next == PendingAdjustmentAgreementStatus.Failed;
            case PendingAdjustmentAgreementStatus.Queued:
                switch (next)
                {
                    case PendingAdjustmentAgreementStatus.Done:
                        return true;
                }
                return false;
            case PendingAdjustmentAgreementStatus.Running:
                switch (next)
                {
                    case PendingAdjustmentAgreementStatus.Failed:
                        return true;
                }
                return false;
            case PendingAdjustmentAgreementStatus.Done:
                return next == PendingAdjustmentAgreementStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingAdjustmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending adjustment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio + (double)Limit, 2);
    }

    public PendingAdjustmentAgreement Copy()
    {
        return new PendingAdjustmentAgreement(
            Code,
            Title,
            ExternalRef,
            Reviewer,
            Visible,
            Limit,
            Ratio,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
