namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg slot is in its life cycle.</summary>
public enum ReorgSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One reorg slot, with what the planners need to act on it.</summary>
public sealed class ReorgSlot(string code, string title, string segment, double weight, string grade, double threshold, ReorgSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Weight { get; set; } = weight;
    public string Grade { get; set; } = grade;
    public double Threshold { get; set; } = threshold;
    public ReorgSlotStatus Status { get; set; } = status;

    /// <summary>Starts a reorg slot in status draft.</summary>
    public static ReorgSlot NewReorgSlot(string code, string title)
    {
        return new ReorgSlot(code, title, "", 0.0, "", 0.0, ReorgSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgSlotStatus next)
    {
        switch (Status)
        {
            case ReorgSlotStatus.Draft:
                switch (next)
                {
                    case ReorgSlotStatus.Active:
                        return true;
                }
                return false;
            case ReorgSlotStatus.Pending:
                switch (next)
                {
                    case ReorgSlotStatus.Suspended:
                    case ReorgSlotStatus.Closed:
                        return true;
                }
                return false;
            case ReorgSlotStatus.Active:
                return next == ReorgSlotStatus.Suspended || next == ReorgSlotStatus.Closed;
            case ReorgSlotStatus.Suspended:
                return next == ReorgSlotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ReorgSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Threshold + Weight, 2);
    }

    public ReorgSlot Copy()
    {
        return new ReorgSlot(Code, Title, Segment, Weight, Grade, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
