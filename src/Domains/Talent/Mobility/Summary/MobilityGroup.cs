namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility group is in its life cycle.</summary>
public enum MobilityGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a mobility group and its current status.</summary>
public sealed class MobilityGroup(string code, string name, List<string> tags, bool visible, string contactEmail, int quantity, MobilityGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public bool Visible { get; set; } = visible;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public MobilityGroupStatus Status { get; set; } = status;

    /// <summary>Starts a mobility group in status planned.</summary>
    public static MobilityGroup NewMobilityGroup(string code, string name)
    {
        return new MobilityGroup(code, name, new List<string>(), false, "", 0, MobilityGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityGroupStatus next)
    {
        switch (Status)
        {
            case MobilityGroupStatus.Planned:
                switch (next)
                {
                    case MobilityGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case MobilityGroupStatus.Scheduled:
                return next == MobilityGroupStatus.Cancelled;
            case MobilityGroupStatus.InProgress:
                return next == MobilityGroupStatus.Completed;
            case MobilityGroupStatus.Completed:
                switch (next)
                {
                    case MobilityGroupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public MobilityGroup Copy()
    {
        return new MobilityGroup(Code, Name, Tags, Visible, ContactEmail, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
