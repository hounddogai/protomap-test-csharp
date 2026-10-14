namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance review is in its life cycle.</summary>
public enum BalanceReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A balance review as the balance team keeps it.</summary>
public sealed class BalanceReview(string code, string label, string salaryBand, int version, int tenureMonths, double margin, BalanceReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public int Version { get; set; } = version;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Margin { get; set; } = margin;
    public BalanceReviewStatus Status { get; set; } = status;

    /// <summary>Starts a balance review in status open.</summary>
    public static BalanceReview NewBalanceReview(string code, string label)
    {
        return new BalanceReview(code, label, "", 0, 0, 0.0, BalanceReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceReviewStatus next)
    {
        switch (Status)
        {
            case BalanceReviewStatus.Open:
                switch (next)
                {
                    case BalanceReviewStatus.Reopened:
                    case BalanceReviewStatus.Archived:
                        return true;
                }
                return false;
            case BalanceReviewStatus.InReview:
                switch (next)
                {
                    case BalanceReviewStatus.Resolved:
                    case BalanceReviewStatus.Reopened:
                        return true;
                }
                return false;
            case BalanceReviewStatus.Resolved:
                return next == BalanceReviewStatus.Reopened || next == BalanceReviewStatus.Archived;
            case BalanceReviewStatus.Reopened:
                switch (next)
                {
                    case BalanceReviewStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Version + Margin, 2);
    }

    public BalanceReview Copy()
    {
        return new BalanceReview(Code, Label, SalaryBand, Version, TenureMonths, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
