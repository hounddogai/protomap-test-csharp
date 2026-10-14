namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pay band agreement is in its life cycle.</summary>
public enum GlobalPayBandAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global pay band agreement. Its code is unique within a tenant.</summary>
public sealed class GlobalPayBandAgreement(string code, string label, bool archived, int limit, double discount, string ownerId, double threshold, GlobalPayBandAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public int Limit { get; set; } = limit;
    public double Discount { get; set; } = discount;
    public string OwnerId { get; set; } = ownerId;
    public double Threshold { get; set; } = threshold;
    public GlobalPayBandAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a global pay band agreement in status planned.</summary>
    public static GlobalPayBandAgreement NewGlobalPayBandAgreement(string code, string label)
    {
        return new GlobalPayBandAgreement(code, label, false, 0, 0.0, "", 0.0, GlobalPayBandAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPayBandAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPayBandAgreementStatus next)
    {
        switch (Status)
        {
            case GlobalPayBandAgreementStatus.Planned:
                return next == GlobalPayBandAgreementStatus.Cancelled;
            case GlobalPayBandAgreementStatus.Scheduled:
                switch (next)
                {
                    case GlobalPayBandAgreementStatus.InProgress:
                    case GlobalPayBandAgreementStatus.Completed:
                        return true;
                }
                return false;
            case GlobalPayBandAgreementStatus.InProgress:
                switch (next)
                {
                    case GlobalPayBandAgreementStatus.Completed:
                        return true;
                }
                return false;
            case GlobalPayBandAgreementStatus.Completed:
                return next == GlobalPayBandAgreementStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalPayBandAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pay band agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"global pay band agreement"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Discount * Threshold, 2);
    }

    public GlobalPayBandAgreement Copy()
    {
        return new GlobalPayBandAgreement(Code, Label, Archived, Limit, Discount, OwnerId, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
