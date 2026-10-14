namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment notice is in its life cycle.</summary>
public enum GarnishmentNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A garnishment notice as the garnishment team keeps it.</summary>
public sealed class GarnishmentNotice(string code, string name, double budget, double discount, string grade, int tenureMonths, GarnishmentNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public string Grade { get; set; } = grade;
    public int TenureMonths { get; set; } = tenureMonths;
    public GarnishmentNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment notice in status draft.</summary>
    public static GarnishmentNotice NewGarnishmentNotice(string code, string name)
    {
        return new GarnishmentNotice(code, name, 0.0, 0.0, "", 0, GarnishmentNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentNoticeStatus next)
    {
        switch (Status)
        {
            case GarnishmentNoticeStatus.Draft:
                switch (next)
                {
                    case GarnishmentNoticeStatus.Suspended:
                        return true;
                }
                return false;
            case GarnishmentNoticeStatus.Pending:
                return next == GarnishmentNoticeStatus.Closed;
            case GarnishmentNoticeStatus.Active:
                return next == GarnishmentNoticeStatus.Suspended || next == GarnishmentNoticeStatus.Closed;
            case GarnishmentNoticeStatus.Suspended:
                return next == GarnishmentNoticeStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GarnishmentNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Budget * (double)TenureMonths, 2);
    }

    public GarnishmentNotice Copy()
    {
        return new GarnishmentNotice(Code, Name, Budget, Discount, Grade, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
