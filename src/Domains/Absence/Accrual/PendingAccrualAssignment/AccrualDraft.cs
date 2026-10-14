namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual draft is in its life cycle.</summary>
public enum AccrualDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a accrual draft and its current status.</summary>
public sealed class AccrualDraft(string code, string label, string country, bool archived, int endsAt, AccrualDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public bool Archived { get; set; } = archived;
    public int EndsAt { get; set; } = endsAt;
    public AccrualDraftStatus Status { get; set; } = status;

    /// <summary>Starts a accrual draft in status idle.</summary>
    public static AccrualDraft NewAccrualDraft(string code, string label)
    {
        return new AccrualDraft(code, label, "", false, 0, AccrualDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualDraftStatus next)
    {
        switch (Status)
        {
            case AccrualDraftStatus.Idle:
                switch (next)
                {
                    case AccrualDraftStatus.Ready:
                        return true;
                }
                return false;
            case AccrualDraftStatus.Warming:
                switch (next)
                {
                    case AccrualDraftStatus.Stopped:
                        return true;
                }
                return false;
            case AccrualDraftStatus.Ready:
                return next == AccrualDraftStatus.Stopped;
            case AccrualDraftStatus.Draining:
                return next == AccrualDraftStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AccrualDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public AccrualDraft Copy()
    {
        return new AccrualDraft(Code, Label, Country, Archived, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
