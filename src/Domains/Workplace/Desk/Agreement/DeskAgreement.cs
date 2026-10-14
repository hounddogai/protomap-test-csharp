namespace Acme.Workforce.Workplace.Desk.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk agreement is in its life cycle.</summary>
public enum DeskAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A desk agreement. Its code is unique within a tenant.</summary>
public sealed class DeskAgreement(string code, string label, double ratio, string badge, string jobCode, string grade, int batchSize, bool pinned, double hours, string description, DeskAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public string JobCode { get; set; } = jobCode;
    public string Grade { get; set; } = grade;
    public int BatchSize { get; set; } = batchSize;
    public bool Pinned { get; set; } = pinned;
    public double Hours { get; set; } = hours;
    public string Description { get; set; } = description;
    public DeskAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a desk agreement in status requested.</summary>
    public static DeskAgreement NewDeskAgreement(string code, string label)
    {
        return new DeskAgreement(code, label, 0.0, "", "", "", 0, false, 0.0, "", DeskAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskAgreementStatus next)
    {
        switch (Status)
        {
            case DeskAgreementStatus.Requested:
                return next == DeskAgreementStatus.Revoked;
            case DeskAgreementStatus.Granted:
                return next == DeskAgreementStatus.Expired;
            case DeskAgreementStatus.Revoked:
                switch (next)
                {
                    case DeskAgreementStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round(Hours + Ratio, 2);
    }

    public DeskAgreement Copy()
    {
        return new DeskAgreement(
            Code,
            Label,
            Ratio,
            Badge,
            JobCode,
            Grade,
            BatchSize,
            Pinned,
            Hours,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
