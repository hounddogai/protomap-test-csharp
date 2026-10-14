namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual access badge budget is in its life cycle.</summary>
public enum ManualAccessBadgeBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A manual access badge budget as the access badge team keeps it.</summary>
public sealed class ManualAccessBadgeBudget(string code, string title, int limit, string checksum, string unit, ManualAccessBadgeBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string Checksum { get; set; } = checksum;
    public string Unit { get; set; } = unit;
    public ManualAccessBadgeBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a manual access badge budget in status open.</summary>
    public static ManualAccessBadgeBudget NewManualAccessBadgeBudget(string code, string title)
    {
        return new ManualAccessBadgeBudget(code, title, 0, "", "", ManualAccessBadgeBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAccessBadgeBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAccessBadgeBudgetStatus next)
    {
        switch (Status)
        {
            case ManualAccessBadgeBudgetStatus.Open:
                return next == ManualAccessBadgeBudgetStatus.InReview;
            case ManualAccessBadgeBudgetStatus.InReview:
                return next == ManualAccessBadgeBudgetStatus.Resolved || next == ManualAccessBadgeBudgetStatus.Reopened;
            case ManualAccessBadgeBudgetStatus.Resolved:
                return next == ManualAccessBadgeBudgetStatus.Reopened || next == ManualAccessBadgeBudgetStatus.Archived;
            case ManualAccessBadgeBudgetStatus.Reopened:
                switch (next)
                {
                    case ManualAccessBadgeBudgetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualAccessBadgeBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual access badge budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public ManualAccessBadgeBudget Copy()
    {
        return new ManualAccessBadgeBudget(Code, Title, Limit, Checksum, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
