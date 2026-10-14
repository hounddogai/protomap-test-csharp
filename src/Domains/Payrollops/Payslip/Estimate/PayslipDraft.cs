namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip draft is in its life cycle.</summary>
public enum PayslipDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a payslip draft and its current status.</summary>
public sealed class PayslipDraft(string code, string name, string skill, double rating, int endsAt, string reviewer, string grade, PayslipDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public double Rating { get; set; } = rating;
    public int EndsAt { get; set; } = endsAt;
    public string Reviewer { get; set; } = reviewer;
    public string Grade { get; set; } = grade;
    public PayslipDraftStatus Status { get; set; } = status;

    /// <summary>Starts a payslip draft in status draft.</summary>
    public static PayslipDraft NewPayslipDraft(string code, string name)
    {
        return new PayslipDraft(code, name, "", 0.0, 0, "", "", PayslipDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipDraftStatus next)
    {
        switch (Status)
        {
            case PayslipDraftStatus.Draft:
                switch (next)
                {
                    case PayslipDraftStatus.Active:
                        return true;
                }
                return false;
            case PayslipDraftStatus.Pending:
                switch (next)
                {
                    case PayslipDraftStatus.Active:
                    case PayslipDraftStatus.Closed:
                        return true;
                }
                return false;
            case PayslipDraftStatus.Active:
                switch (next)
                {
                    case PayslipDraftStatus.Suspended:
                        return true;
                }
                return false;
            case PayslipDraftStatus.Suspended:
                return next == PayslipDraftStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PayslipDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Rating - (double)EndsAt, 2);
    }

    public PayslipDraft Copy()
    {
        return new PayslipDraft(Code, Name, Skill, Rating, EndsAt, Reviewer, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
