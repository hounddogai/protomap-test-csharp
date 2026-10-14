namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a payslip review is in its life cycle.</summary>
public enum PayslipReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One payslip review, with what the planners need to act on it.</summary>
public sealed class PayslipReview(string code, string title, int tenureMonths, double ceiling, int graceDays, double discount, string skill, string badge, bool approved, int quantity, PayslipReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ceiling { get; set; } = ceiling;
    public int GraceDays { get; set; } = graceDays;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public string Badge { get; set; } = badge;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public PayslipReviewStatus Status { get; set; } = status;

    /// <summary>Starts a payslip review in status idle.</summary>
    public static PayslipReview NewPayslipReview(string code, string title)
    {
        return new PayslipReview(code, title, 0, 0.0, 0, 0.0, "", "", false, 0, PayslipReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayslipReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayslipReviewStatus next)
    {
        switch (Status)
        {
            case PayslipReviewStatus.Idle:
                return next == PayslipReviewStatus.Warming || next == PayslipReviewStatus.Stopped;
            case PayslipReviewStatus.Warming:
                return next == PayslipReviewStatus.Ready || next == PayslipReviewStatus.Stopped;
            case PayslipReviewStatus.Ready:
                return next == PayslipReviewStatus.Draining || next == PayslipReviewStatus.Stopped;
            case PayslipReviewStatus.Draining:
                return next == PayslipReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayslipReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"payslip review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)TenureMonths * (double)GraceDays, 2);
    }

    public PayslipReview Copy()
    {
        return new PayslipReview(
            Code,
            Title,
            TenureMonths,
            Ceiling,
            GraceDays,
            Discount,
            Skill,
            Badge,
            Approved,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
