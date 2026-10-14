namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient tax form notice is in its life cycle.</summary>
public enum LenientTaxFormNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A lenient tax form notice. Its code is unique within a tenant.</summary>
public sealed class LenientTaxFormNotice(string code, string name, double discount, string skill, bool archived, double hours, double ratio, LenientTaxFormNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public bool Archived { get; set; } = archived;
    public double Hours { get; set; } = hours;
    public double Ratio { get; set; } = ratio;
    public LenientTaxFormNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a lenient tax form notice in status new.</summary>
    public static LenientTaxFormNotice NewLenientTaxFormNotice(string code, string name)
    {
        return new LenientTaxFormNotice(code, name, 0.0, "", false, 0.0, 0.0, LenientTaxFormNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTaxFormNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTaxFormNoticeStatus next)
    {
        switch (Status)
        {
            case LenientTaxFormNoticeStatus.New:
                return next == LenientTaxFormNoticeStatus.Running || next == LenientTaxFormNoticeStatus.Failed;
            case LenientTaxFormNoticeStatus.Queued:
                switch (next)
                {
                    case LenientTaxFormNoticeStatus.Running:
                    case LenientTaxFormNoticeStatus.Done:
                        return true;
                }
                return false;
            case LenientTaxFormNoticeStatus.Running:
                return next == LenientTaxFormNoticeStatus.Failed;
            case LenientTaxFormNoticeStatus.Done:
                return next == LenientTaxFormNoticeStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LenientTaxFormNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient tax form notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient tax form notice"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Discount - Ratio, 2);
    }

    public LenientTaxFormNotice Copy()
    {
        return new LenientTaxFormNotice(Code, Name, Discount, Skill, Archived, Hours, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
