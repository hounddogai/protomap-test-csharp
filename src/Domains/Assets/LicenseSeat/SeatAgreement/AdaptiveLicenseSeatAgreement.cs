namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive license seat agreement is in its life cycle.</summary>
public enum AdaptiveLicenseSeatAgreementStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adaptive license seat agreement, with what the planners need to act on it.</summary>
public sealed class AdaptiveLicenseSeatAgreement(string code, string label, double threshold, double rate, string jobCode, bool pinned, string source, double weight, int sequence, AdaptiveLicenseSeatAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public double Rate { get; set; } = rate;
    public string JobCode { get; set; } = jobCode;
    public bool Pinned { get; set; } = pinned;
    public string Source { get; set; } = source;
    public double Weight { get; set; } = weight;
    public int Sequence { get; set; } = sequence;
    public AdaptiveLicenseSeatAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive license seat agreement in status idle.</summary>
    public static AdaptiveLicenseSeatAgreement NewAdaptiveLicenseSeatAgreement(string code, string label)
    {
        return new AdaptiveLicenseSeatAgreement(
            code,
            label,
            0.0,
            0.0,
            "",
            false,
            "",
            0.0,
            0,
            AdaptiveLicenseSeatAgreementStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveLicenseSeatAgreementStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveLicenseSeatAgreementStatus next)
    {
        switch (Status)
        {
            case AdaptiveLicenseSeatAgreementStatus.Idle:
                return next == AdaptiveLicenseSeatAgreementStatus.Stopped;
            case AdaptiveLicenseSeatAgreementStatus.Warming:
                return next == AdaptiveLicenseSeatAgreementStatus.Ready;
            case AdaptiveLicenseSeatAgreementStatus.Ready:
                switch (next)
                {
                    case AdaptiveLicenseSeatAgreementStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveLicenseSeatAgreementStatus.Draining:
                switch (next)
                {
                    case AdaptiveLicenseSeatAgreementStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveLicenseSeatAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive license seat agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetTotal()
    {
        return Math.Round(Rate + (double)Sequence, 2);
    }

    public AdaptiveLicenseSeatAgreement Copy()
    {
        return new AdaptiveLicenseSeatAgreement(
            Code,
            Label,
            Threshold,
            Rate,
            JobCode,
            Pinned,
            Source,
            Weight,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
