namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position entry is in its life cycle.</summary>
public enum PositionEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A position entry. Its code is unique within a tenant.</summary>
public sealed class PositionEntry(string code, string label, string ownerId, double rating, double fte, double cost, string currency, PositionEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public double Rating { get; set; } = rating;
    public double Fte { get; set; } = fte;
    public double Cost { get; set; } = cost;
    public string Currency { get; set; } = currency;
    public PositionEntryStatus Status { get; set; } = status;

    /// <summary>Starts a position entry in status planned.</summary>
    public static PositionEntry NewPositionEntry(string code, string label)
    {
        return new PositionEntry(code, label, "", 0.0, 0.0, 0.0, "", PositionEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionEntryStatus next)
    {
        switch (Status)
        {
            case PositionEntryStatus.Planned:
                switch (next)
                {
                    case PositionEntryStatus.Completed:
                        return true;
                }
                return false;
            case PositionEntryStatus.Scheduled:
                switch (next)
                {
                    case PositionEntryStatus.InProgress:
                        return true;
                }
                return false;
            case PositionEntryStatus.InProgress:
                switch (next)
                {
                    case PositionEntryStatus.Completed:
                    case PositionEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case PositionEntryStatus.Completed:
                return next == PositionEntryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PositionEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Fte - Rating, 2);
    }

    public PositionEntry Copy()
    {
        return new PositionEntry(Code, Label, OwnerId, Rating, Fte, Cost, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
