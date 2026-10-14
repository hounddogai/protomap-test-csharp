namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual statement of work profile is in its life cycle.</summary>
public enum ManualStatementOfWorkProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One manual statement of work profile, with what the planners need to act on it.</summary>
public sealed class ManualStatementOfWorkProfile(string code, string title, string jobCode, double score, double margin, bool visible, bool pinned, int quantity, int priority, ManualStatementOfWorkProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public double Score { get; set; } = score;
    public double Margin { get; set; } = margin;
    public bool Visible { get; set; } = visible;
    public bool Pinned { get; set; } = pinned;
    public int Quantity { get; set; } = quantity;
    public int Priority { get; set; } = priority;
    public ManualStatementOfWorkProfileStatus Status { get; set; } = status;

    /// <summary>Starts a manual statement of work profile in status open.</summary>
    public static ManualStatementOfWorkProfile NewManualStatementOfWorkProfile(string code, string title)
    {
        return new ManualStatementOfWorkProfile(
            code,
            title,
            "",
            0.0,
            0.0,
            false,
            false,
            0,
            0,
            ManualStatementOfWorkProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualStatementOfWorkProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualStatementOfWorkProfileStatus next)
    {
        switch (Status)
        {
            case ManualStatementOfWorkProfileStatus.Open:
                switch (next)
                {
                    case ManualStatementOfWorkProfileStatus.Reopened:
                    case ManualStatementOfWorkProfileStatus.Archived:
                        return true;
                }
                return false;
            case ManualStatementOfWorkProfileStatus.InReview:
                return next == ManualStatementOfWorkProfileStatus.Reopened || next == ManualStatementOfWorkProfileStatus.Archived;
            case ManualStatementOfWorkProfileStatus.Resolved:
                switch (next)
                {
                    case ManualStatementOfWorkProfileStatus.Archived:
                        return true;
                }
                return false;
            case ManualStatementOfWorkProfileStatus.Reopened:
                switch (next)
                {
                    case ManualStatementOfWorkProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualStatementOfWorkProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual statement of work profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Priority * Margin, 2);
    }

    public ManualStatementOfWorkProfile Copy()
    {
        return new ManualStatementOfWorkProfile(
            Code,
            Title,
            JobCode,
            Score,
            Margin,
            Visible,
            Pinned,
            Quantity,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
