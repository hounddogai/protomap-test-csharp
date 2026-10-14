namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy desk entry is in its life cycle.</summary>
public enum LegacyDeskEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One legacy desk entry, with what the planners need to act on it.</summary>
public sealed class LegacyDeskEntry(string code, string title, double score, string externalRef, int retries, string channel, LegacyDeskEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string ExternalRef { get; set; } = externalRef;
    public int Retries { get; set; } = retries;
    public string Channel { get; set; } = channel;
    public LegacyDeskEntryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy desk entry in status draft.</summary>
    public static LegacyDeskEntry NewLegacyDeskEntry(string code, string title)
    {
        return new LegacyDeskEntry(code, title, 0.0, "", 0, "", LegacyDeskEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyDeskEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyDeskEntryStatus next)
    {
        switch (Status)
        {
            case LegacyDeskEntryStatus.Draft:
                return next == LegacyDeskEntryStatus.Active || next == LegacyDeskEntryStatus.Suspended;
            case LegacyDeskEntryStatus.Pending:
                return next == LegacyDeskEntryStatus.Suspended || next == LegacyDeskEntryStatus.Closed;
            case LegacyDeskEntryStatus.Active:
                return next == LegacyDeskEntryStatus.Suspended || next == LegacyDeskEntryStatus.Closed;
            case LegacyDeskEntryStatus.Suspended:
                return next == LegacyDeskEntryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyDeskEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy desk entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Retries * Score, 2);
    }

    public LegacyDeskEntry Copy()
    {
        return new LegacyDeskEntry(Code, Title, Score, ExternalRef, Retries, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
