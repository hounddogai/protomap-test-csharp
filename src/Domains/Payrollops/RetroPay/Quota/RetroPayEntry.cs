namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay entry is in its life cycle.</summary>
public enum RetroPayEntryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One retro pay entry, with what the planners need to act on it.</summary>
public sealed class RetroPayEntry(string code, string name, int quantity, int durationMinutes, string location, int batchSize, string label, RetroPayEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Location { get; set; } = location;
    public int BatchSize { get; set; } = batchSize;
    public string Label { get; set; } = label;
    public RetroPayEntryStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay entry in status open.</summary>
    public static RetroPayEntry NewRetroPayEntry(string code, string name)
    {
        return new RetroPayEntry(code, name, 0, 0, "", 0, "", RetroPayEntryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayEntryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayEntryStatus next)
    {
        switch (Status)
        {
            case RetroPayEntryStatus.Open:
                switch (next)
                {
                    case RetroPayEntryStatus.Resolved:
                    case RetroPayEntryStatus.Reopened:
                        return true;
                }
                return false;
            case RetroPayEntryStatus.InReview:
                return next == RetroPayEntryStatus.Resolved;
            case RetroPayEntryStatus.Resolved:
                switch (next)
                {
                    case RetroPayEntryStatus.Reopened:
                        return true;
                }
                return false;
            case RetroPayEntryStatus.Reopened:
                return next == RetroPayEntryStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RetroPayEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)DurationMinutes - (double)Quantity, 2);
    }

    public RetroPayEntry Copy()
    {
        return new RetroPayEntry(Code, Name, Quantity, DurationMinutes, Location, BatchSize, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
