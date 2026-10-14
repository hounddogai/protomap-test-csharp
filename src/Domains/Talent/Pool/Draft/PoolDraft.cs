namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool draft is in its life cycle.</summary>
public enum PoolDraftStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pool draft, with what the planners need to act on it.</summary>
public sealed class PoolDraft(string code, string title, int expiresAt, string grade, string locale, double ceiling, double amount, string salaryBand, bool active, string segment, PoolDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public string Locale { get; set; } = locale;
    public double Ceiling { get; set; } = ceiling;
    public double Amount { get; set; } = amount;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Active { get; set; } = active;
    public string Segment { get; set; } = segment;
    public PoolDraftStatus Status { get; set; } = status;

    /// <summary>Starts a pool draft in status proposed.</summary>
    public static PoolDraft NewPoolDraft(string code, string title)
    {
        return new PoolDraft(code, title, 0, "", "", 0.0, 0.0, "", false, "", PoolDraftStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolDraftStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolDraftStatus next)
    {
        switch (Status)
        {
            case PoolDraftStatus.Proposed:
                return next == PoolDraftStatus.Rejected;
            case PoolDraftStatus.Approved:
                switch (next)
                {
                    case PoolDraftStatus.Rejected:
                    case PoolDraftStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolDraftStatus.Rejected:
                switch (next)
                {
                    case PoolDraftStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)ExpiresAt - Amount, 2);
    }

    public PoolDraft Copy()
    {
        return new PoolDraft(
            Code,
            Title,
            ExpiresAt,
            Grade,
            Locale,
            Ceiling,
            Amount,
            SalaryBand,
            Active,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
