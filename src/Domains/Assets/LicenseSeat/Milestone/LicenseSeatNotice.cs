namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat notice is in its life cycle.</summary>
public enum LicenseSeatNoticeStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A license seat notice as the license seat team keeps it.</summary>
public sealed class LicenseSeatNotice(string code, string title, int windowDays, double weight, string category, string description, int tenureMonths, LicenseSeatNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public string Description { get; set; } = description;
    public int TenureMonths { get; set; } = tenureMonths;
    public LicenseSeatNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a license seat notice in status new.</summary>
    public static LicenseSeatNotice NewLicenseSeatNotice(string code, string title)
    {
        return new LicenseSeatNotice(code, title, 0, 0.0, "", "", 0, LicenseSeatNoticeStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatNoticeStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatNoticeStatus next)
    {
        switch (Status)
        {
            case LicenseSeatNoticeStatus.New:
                return next == LicenseSeatNoticeStatus.Done;
            case LicenseSeatNoticeStatus.Queued:
                switch (next)
                {
                    case LicenseSeatNoticeStatus.Running:
                    case LicenseSeatNoticeStatus.Failed:
                        return true;
                }
                return false;
            case LicenseSeatNoticeStatus.Running:
                return next == LicenseSeatNoticeStatus.Failed;
            case LicenseSeatNoticeStatus.Done:
                switch (next)
                {
                    case LicenseSeatNoticeStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"license seat notice"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)WindowDays - (double)TenureMonths, 2);
    }

    public LicenseSeatNotice Copy()
    {
        return new LicenseSeatNotice(Code, Title, WindowDays, Weight, Category, Description, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
