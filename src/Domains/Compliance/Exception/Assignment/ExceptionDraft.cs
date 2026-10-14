namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception draft is in its life cycle.</summary>
public enum ExceptionDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A exception draft. Its code is unique within a tenant.</summary>
public sealed class ExceptionDraft(string code, string title, string skill, int expiresAt, int retries, string unit, string description, int limit, int sequence, string team, ExceptionDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Retries { get; set; } = retries;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public int Limit { get; set; } = limit;
    public int Sequence { get; set; } = sequence;
    public string Team { get; set; } = team;
    public ExceptionDraftStatus Status { get; set; } = status;

    /// <summary>Starts a exception draft in status idle.</summary>
    public static ExceptionDraft NewExceptionDraft(string code, string title)
    {
        return new ExceptionDraft(code, title, "", 0, 0, "", "", 0, 0, "", ExceptionDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionDraftStatus next)
    {
        switch (Status)
        {
            case ExceptionDraftStatus.Idle:
                switch (next)
                {
                    case ExceptionDraftStatus.Ready:
                        return true;
                }
                return false;
            case ExceptionDraftStatus.Warming:
                switch (next)
                {
                    case ExceptionDraftStatus.Draining:
                    case ExceptionDraftStatus.Stopped:
                        return true;
                }
                return false;
            case ExceptionDraftStatus.Ready:
                switch (next)
                {
                    case ExceptionDraftStatus.Stopped:
                        return true;
                }
                return false;
            case ExceptionDraftStatus.Draining:
                switch (next)
                {
                    case ExceptionDraftStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"exception draft"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Limit + (double)ExpiresAt, 2);
    }

    public ExceptionDraft Copy()
    {
        return new ExceptionDraft(
            Code,
            Title,
            Skill,
            ExpiresAt,
            Retries,
            Unit,
            Description,
            Limit,
            Sequence,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
