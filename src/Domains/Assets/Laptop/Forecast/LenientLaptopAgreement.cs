namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient laptop agreement is in its life cycle.</summary>
public enum LenientLaptopAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a lenient laptop agreement and its current status.</summary>
public sealed class LenientLaptopAgreement(string code, string name, int quantity, double rate, double margin, int leaveDays, int limit, string checksum, int attempts, LenientLaptopAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Rate { get; set; } = rate;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public int Attempts { get; set; } = attempts;
    public LenientLaptopAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a lenient laptop agreement in status new.</summary>
    public static LenientLaptopAgreement NewLenientLaptopAgreement(string code, string name)
    {
        return new LenientLaptopAgreement(code, name, 0, 0.0, 0.0, 0, 0, "", 0, LenientLaptopAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLaptopAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLaptopAgreementStatus next)
    {
        switch (Status)
        {
            case LenientLaptopAgreementStatus.New:
                switch (next)
                {
                    case LenientLaptopAgreementStatus.Done:
                    case LenientLaptopAgreementStatus.Failed:
                        return true;
                }
                return false;
            case LenientLaptopAgreementStatus.Queued:
                return next == LenientLaptopAgreementStatus.Done;
            case LenientLaptopAgreementStatus.Running:
                switch (next)
                {
                    case LenientLaptopAgreementStatus.Done:
                    case LenientLaptopAgreementStatus.Failed:
                        return true;
                }
                return false;
            case LenientLaptopAgreementStatus.Done:
                return next == LenientLaptopAgreementStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LenientLaptopAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient laptop agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)LeaveDays + (double)Limit, 2);
    }

    public LenientLaptopAgreement Copy()
    {
        return new LenientLaptopAgreement(
            Code,
            Name,
            Quantity,
            Rate,
            Margin,
            LeaveDays,
            Limit,
            Checksum,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
