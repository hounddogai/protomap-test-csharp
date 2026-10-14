namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual equity grant group is in its life cycle.</summary>
public enum ManualEquityGrantGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a manual equity grant group and its current status.</summary>
public sealed class ManualEquityGrantGroup(string code, string title, bool active, double discount, string grade, int limit, double hours, int graceDays, bool archived, ManualEquityGrantGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public double Discount { get; set; } = discount;
    public string Grade { get; set; } = grade;
    public int Limit { get; set; } = limit;
    public double Hours { get; set; } = hours;
    public int GraceDays { get; set; } = graceDays;
    public bool Archived { get; set; } = archived;
    public ManualEquityGrantGroupStatus Status { get; set; } = status;

    /// <summary>Starts a manual equity grant group in status open.</summary>
    public static ManualEquityGrantGroup NewManualEquityGrantGroup(string code, string title)
    {
        return new ManualEquityGrantGroup(
            code,
            title,
            false,
            0.0,
            "",
            0,
            0.0,
            0,
            false,
            ManualEquityGrantGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualEquityGrantGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualEquityGrantGroupStatus next)
    {
        switch (Status)
        {
            case ManualEquityGrantGroupStatus.Open:
                return next == ManualEquityGrantGroupStatus.Archived;
            case ManualEquityGrantGroupStatus.InReview:
                return next == ManualEquityGrantGroupStatus.Archived;
            case ManualEquityGrantGroupStatus.Resolved:
                return next == ManualEquityGrantGroupStatus.Reopened || next == ManualEquityGrantGroupStatus.Archived;
            case ManualEquityGrantGroupStatus.Reopened:
                switch (next)
                {
                    case ManualEquityGrantGroupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualEquityGrantGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual equity grant group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Limit * Discount, 2);
    }

    public ManualEquityGrantGroup Copy()
    {
        return new ManualEquityGrantGroup(
            Code,
            Title,
            Active,
            Discount,
            Grade,
            Limit,
            Hours,
            GraceDays,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
