namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh adjustment is in its life cycle.</summary>
public enum RefreshAdjustmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One refresh adjustment, with what the planners need to act on it.</summary>
public sealed class RefreshAdjustment(string code, string label, double budget, string source, string description, double fte, double margin, bool pinned, double amount, int windowDays, string locale, RefreshAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Source { get; set; } = source;
    public string Description { get; set; } = description;
    public double Fte { get; set; } = fte;
    public double Margin { get; set; } = margin;
    public bool Pinned { get; set; } = pinned;
    public double Amount { get; set; } = amount;
    public int WindowDays { get; set; } = windowDays;
    public string Locale { get; set; } = locale;
    public RefreshAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a refresh adjustment in status draft.</summary>
    public static RefreshAdjustment NewRefreshAdjustment(string code, string label)
    {
        return new RefreshAdjustment(
            code,
            label,
            0.0,
            "",
            "",
            0.0,
            0.0,
            false,
            0.0,
            0,
            "",
            RefreshAdjustmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshAdjustmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshAdjustmentStatus next)
    {
        switch (Status)
        {
            case RefreshAdjustmentStatus.Draft:
                switch (next)
                {
                    case RefreshAdjustmentStatus.Suspended:
                    case RefreshAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case RefreshAdjustmentStatus.Pending:
                switch (next)
                {
                    case RefreshAdjustmentStatus.Suspended:
                    case RefreshAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case RefreshAdjustmentStatus.Active:
                switch (next)
                {
                    case RefreshAdjustmentStatus.Suspended:
                    case RefreshAdjustmentStatus.Closed:
                        return true;
                }
                return false;
            case RefreshAdjustmentStatus.Suspended:
                switch (next)
                {
                    case RefreshAdjustmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Fte * Margin, 2);
    }

    public RefreshAdjustment Copy()
    {
        return new RefreshAdjustment(
            Code,
            Label,
            Budget,
            Source,
            Description,
            Fte,
            Margin,
            Pinned,
            Amount,
            WindowDays,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
