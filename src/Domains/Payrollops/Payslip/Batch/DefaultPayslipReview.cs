namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default payslip review is in its life cycle.</summary>
public enum DefaultPayslipReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a default payslip review and its current status.</summary>
public sealed class DefaultPayslipReview(string code, string title, string salaryBand, string contactEmail, int startsAt, bool archived, double minimum, int revision, int endsAt, DefaultPayslipReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string ContactEmail { get; set; } = contactEmail;
    public int StartsAt { get; set; } = startsAt;
    public bool Archived { get; set; } = archived;
    public double Minimum { get; set; } = minimum;
    public int Revision { get; set; } = revision;
    public int EndsAt { get; set; } = endsAt;
    public DefaultPayslipReviewStatus Status { get; set; } = status;

    /// <summary>Starts a default payslip review in status proposed.</summary>
    public static DefaultPayslipReview NewDefaultPayslipReview(string code, string title)
    {
        return new DefaultPayslipReview(
            code,
            title,
            "",
            "",
            0,
            false,
            0.0,
            0,
            0,
            DefaultPayslipReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayslipReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayslipReviewStatus next)
    {
        switch (Status)
        {
            case DefaultPayslipReviewStatus.Proposed:
                switch (next)
                {
                    case DefaultPayslipReviewStatus.Rejected:
                    case DefaultPayslipReviewStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultPayslipReviewStatus.Approved:
                switch (next)
                {
                    case DefaultPayslipReviewStatus.Rejected:
                    case DefaultPayslipReviewStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultPayslipReviewStatus.Rejected:
                return next == DefaultPayslipReviewStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultPayslipReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default payslip review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"default payslip review"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Revision - (double)EndsAt, 2);
    }

    public DefaultPayslipReview Copy()
    {
        return new DefaultPayslipReview(
            Code,
            Title,
            SalaryBand,
            ContactEmail,
            StartsAt,
            Archived,
            Minimum,
            Revision,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
