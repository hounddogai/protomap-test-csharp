namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle slot is in its life cycle.</summary>
public enum MeritCycleSlotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A merit cycle slot as the merit cycle team keeps it.</summary>
public sealed class MeritCycleSlot(string code, string title, List<string> tags, double threshold, int revision, string costCenter, string unit, string name, string notes, double weight, MeritCycleSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public double Threshold { get; set; } = threshold;
    public int Revision { get; set; } = revision;
    public string CostCenter { get; set; } = costCenter;
    public string Unit { get; set; } = unit;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public double Weight { get; set; } = weight;
    public MeritCycleSlotStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle slot in status draft.</summary>
    public static MeritCycleSlot NewMeritCycleSlot(string code, string title)
    {
        return new MeritCycleSlot(
            code,
            title,
            new List<string>(),
            0.0,
            0,
            "",
            "",
            "",
            "",
            0.0,
            MeritCycleSlotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleSlotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleSlotStatus next)
    {
        switch (Status)
        {
            case MeritCycleSlotStatus.Draft:
                return next == MeritCycleSlotStatus.Pending || next == MeritCycleSlotStatus.Active;
            case MeritCycleSlotStatus.Pending:
                return next == MeritCycleSlotStatus.Active;
            case MeritCycleSlotStatus.Active:
                switch (next)
                {
                    case MeritCycleSlotStatus.Suspended:
                        return true;
                }
                return false;
            case MeritCycleSlotStatus.Suspended:
                return next == MeritCycleSlotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(MeritCycleSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"merit cycle slot"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedVolume()
    {
        return Math.Round(Threshold - (double)Revision, 2);
    }

    public MeritCycleSlot Copy()
    {
        return new MeritCycleSlot(
            Code,
            Title,
            Tags,
            Threshold,
            Revision,
            CostCenter,
            Unit,
            Name,
            Notes,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
