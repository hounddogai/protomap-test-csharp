namespace Acme.Workforce.Talent.SuccessionPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan agreement is in its life cycle.</summary>
public enum SuccessionPlanAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A succession plan agreement as the succession plan team keeps it.</summary>
public sealed class SuccessionPlanAgreement(string code, string name, int graceDays, string country, int revision, double ratio, double hours, int windowDays, string notes, double discount, SuccessionPlanAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public int Revision { get; set; } = revision;
    public double Ratio { get; set; } = ratio;
    public double Hours { get; set; } = hours;
    public int WindowDays { get; set; } = windowDays;
    public string Notes { get; set; } = notes;
    public double Discount { get; set; } = discount;
    public SuccessionPlanAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan agreement in status new.</summary>
    public static SuccessionPlanAgreement NewSuccessionPlanAgreement(string code, string name)
    {
        return new SuccessionPlanAgreement(
            code,
            name,
            0,
            "",
            0,
            0.0,
            0.0,
            0,
            "",
            0.0,
            SuccessionPlanAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanAgreementStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanAgreementStatus.New:
                return next == SuccessionPlanAgreementStatus.Done;
            case SuccessionPlanAgreementStatus.Queued:
                return next == SuccessionPlanAgreementStatus.Done;
            case SuccessionPlanAgreementStatus.Running:
                return next == SuccessionPlanAgreementStatus.Done;
            case SuccessionPlanAgreementStatus.Done:
                switch (next)
                {
                    case SuccessionPlanAgreementStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetTotal()
    {
        return Math.Round(Hours - Discount, 2);
    }

    public SuccessionPlanAgreement Copy()
    {
        return new SuccessionPlanAgreement(
            Code,
            Name,
            GraceDays,
            Country,
            Revision,
            Ratio,
            Hours,
            WindowDays,
            Notes,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
