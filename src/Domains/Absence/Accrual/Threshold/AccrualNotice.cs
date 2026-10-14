namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual notice is in its life cycle.</summary>
public enum AccrualNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a accrual notice and its current status.</summary>
public sealed class AccrualNotice(string code, string label, int windowDays, int createdAt, string category, string title, AccrualNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public int CreatedAt { get; set; } = createdAt;
    public string Category { get; set; } = category;
    public string Title { get; set; } = title;
    public AccrualNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a accrual notice in status created.</summary>
    public static AccrualNotice NewAccrualNotice(string code, string label)
    {
        return new AccrualNotice(code, label, 0, 0, "", "", AccrualNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualNoticeStatus next)
    {
        switch (Status)
        {
            case AccrualNoticeStatus.Created:
                return next == AccrualNoticeStatus.Validated || next == AccrualNoticeStatus.Published;
            case AccrualNoticeStatus.Validated:
                switch (next)
                {
                    case AccrualNoticeStatus.Published:
                    case AccrualNoticeStatus.Retired:
                        return true;
                }
                return false;
            case AccrualNoticeStatus.Published:
                switch (next)
                {
                    case AccrualNoticeStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)CreatedAt - (double)WindowDays, 2);
    }

    public AccrualNotice Copy()
    {
        return new AccrualNotice(Code, Label, WindowDays, CreatedAt, Category, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
