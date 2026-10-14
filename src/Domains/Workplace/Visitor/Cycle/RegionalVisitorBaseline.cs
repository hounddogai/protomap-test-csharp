namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional visitor baseline is in its life cycle.</summary>
public enum RegionalVisitorBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional visitor baseline as the visitor team keeps it.</summary>
public sealed class RegionalVisitorBaseline(string code, string title, string managerId, int revision, string phone, string badge, int windowDays, string salaryBand, double discount, int sequence, RegionalVisitorBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public int Revision { get; set; } = revision;
    public string Phone { get; set; } = phone;
    public string Badge { get; set; } = badge;
    public int WindowDays { get; set; } = windowDays;
    public string SalaryBand { get; set; } = salaryBand;
    public double Discount { get; set; } = discount;
    public int Sequence { get; set; } = sequence;
    public RegionalVisitorBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a regional visitor baseline in status open.</summary>
    public static RegionalVisitorBaseline NewRegionalVisitorBaseline(string code, string title)
    {
        return new RegionalVisitorBaseline(
            code,
            title,
            "",
            0,
            "",
            "",
            0,
            "",
            0.0,
            0,
            RegionalVisitorBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalVisitorBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalVisitorBaselineStatus next)
    {
        switch (Status)
        {
            case RegionalVisitorBaselineStatus.Open:
                return next == RegionalVisitorBaselineStatus.Resolved;
            case RegionalVisitorBaselineStatus.InReview:
                return next == RegionalVisitorBaselineStatus.Archived;
            case RegionalVisitorBaselineStatus.Resolved:
                switch (next)
                {
                    case RegionalVisitorBaselineStatus.Reopened:
                    case RegionalVisitorBaselineStatus.Archived:
                        return true;
                }
                return false;
            case RegionalVisitorBaselineStatus.Reopened:
                switch (next)
                {
                    case RegionalVisitorBaselineStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalVisitorBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional visitor baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"regional visitor baseline"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Discount * (double)Revision, 2);
    }

    public RegionalVisitorBaseline Copy()
    {
        return new RegionalVisitorBaseline(
            Code,
            Title,
            ManagerId,
            Revision,
            Phone,
            Badge,
            WindowDays,
            SalaryBand,
            Discount,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
