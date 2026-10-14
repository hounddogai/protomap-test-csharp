namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip template is in its life cycle.</summary>
public enum PayslipTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One payslip template, with what the planners need to act on it.</summary>
public sealed class PayslipTemplate(string code, string title, int expiresAt, string grade, int updatedAt, PayslipTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public int UpdatedAt { get; set; } = updatedAt;
    public PayslipTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a payslip template in status open.</summary>
    public static PayslipTemplate NewPayslipTemplate(string code, string title)
    {
        return new PayslipTemplate(code, title, 0, "", 0, PayslipTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipTemplateStatus next)
    {
        switch (Status)
        {
            case PayslipTemplateStatus.Open:
                switch (next)
                {
                    case PayslipTemplateStatus.InReview:
                    case PayslipTemplateStatus.Resolved:
                        return true;
                }
                return false;
            case PayslipTemplateStatus.InReview:
                return next == PayslipTemplateStatus.Resolved || next == PayslipTemplateStatus.Archived;
            case PayslipTemplateStatus.Resolved:
                switch (next)
                {
                    case PayslipTemplateStatus.Reopened:
                    case PayslipTemplateStatus.Archived:
                        return true;
                }
                return false;
            case PayslipTemplateStatus.Reopened:
                switch (next)
                {
                    case PayslipTemplateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayslipTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)ExpiresAt + (double)UpdatedAt, 2);
    }

    public PayslipTemplate Copy()
    {
        return new PayslipTemplate(Code, Title, ExpiresAt, Grade, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
