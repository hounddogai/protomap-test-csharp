namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip score is in its life cycle.</summary>
public enum PayslipScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One payslip score, with what the planners need to act on it.</summary>
public sealed class PayslipScore(string code, string label, double rating, double maximum, string location, int sequence, string name, string checksum, PayslipScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public double Maximum { get; set; } = maximum;
    public string Location { get; set; } = location;
    public int Sequence { get; set; } = sequence;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public PayslipScoreStatus Status { get; set; } = status;

    /// <summary>Starts a payslip score in status requested.</summary>
    public static PayslipScore NewPayslipScore(string code, string label)
    {
        return new PayslipScore(code, label, 0.0, 0.0, "", 0, "", "", PayslipScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipScoreStatus next)
    {
        switch (Status)
        {
            case PayslipScoreStatus.Requested:
                switch (next)
                {
                    case PayslipScoreStatus.Revoked:
                        return true;
                }
                return false;
            case PayslipScoreStatus.Granted:
                return next == PayslipScoreStatus.Revoked;
            case PayslipScoreStatus.Revoked:
                switch (next)
                {
                    case PayslipScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rating * (double)Sequence, 2);
    }

    public PayslipScore Copy()
    {
        return new PayslipScore(Code, Label, Rating, Maximum, Location, Sequence, Name, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
