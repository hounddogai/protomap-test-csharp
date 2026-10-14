namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh checkpoint is in its life cycle.</summary>
public enum RefreshCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a refresh checkpoint and its current status.</summary>
public sealed class RefreshCheckpoint(string code, string title, string locale, string currency, string notes, string salaryBand, double score, RefreshCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string Currency { get; set; } = currency;
    public string Notes { get; set; } = notes;
    public string SalaryBand { get; set; } = salaryBand;
    public double Score { get; set; } = score;
    public RefreshCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a refresh checkpoint in status open.</summary>
    public static RefreshCheckpoint NewRefreshCheckpoint(string code, string title)
    {
        return new RefreshCheckpoint(code, title, "", "", "", "", 0.0, RefreshCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshCheckpointStatus next)
    {
        switch (Status)
        {
            case RefreshCheckpointStatus.Open:
                return next == RefreshCheckpointStatus.Reopened;
            case RefreshCheckpointStatus.InReview:
                switch (next)
                {
                    case RefreshCheckpointStatus.Reopened:
                        return true;
                }
                return false;
            case RefreshCheckpointStatus.Resolved:
                switch (next)
                {
                    case RefreshCheckpointStatus.Reopened:
                    case RefreshCheckpointStatus.Archived:
                        return true;
                }
                return false;
            case RefreshCheckpointStatus.Reopened:
                return next == RefreshCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RefreshCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"refresh checkpoint"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public RefreshCheckpoint Copy()
    {
        return new RefreshCheckpoint(Code, Title, Locale, Currency, Notes, SalaryBand, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
