namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat milestone is in its life cycle.</summary>
public enum LicenseSeatMilestoneStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A license seat milestone as the license seat team keeps it.</summary>
public sealed class LicenseSeatMilestone(string code, string label, bool archived, bool pinned, string contactEmail, string segment, int quantity, LicenseSeatMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public bool Pinned { get; set; } = pinned;
    public string ContactEmail { get; set; } = contactEmail;
    public string Segment { get; set; } = segment;
    public int Quantity { get; set; } = quantity;
    public LicenseSeatMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a license seat milestone in status created.</summary>
    public static LicenseSeatMilestone NewLicenseSeatMilestone(string code, string label)
    {
        return new LicenseSeatMilestone(code, label, false, false, "", "", 0, LicenseSeatMilestoneStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatMilestoneStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatMilestoneStatus next)
    {
        switch (Status)
        {
            case LicenseSeatMilestoneStatus.Created:
                return next == LicenseSeatMilestoneStatus.Published;
            case LicenseSeatMilestoneStatus.Validated:
                return next == LicenseSeatMilestoneStatus.Published || next == LicenseSeatMilestoneStatus.Retired;
            case LicenseSeatMilestoneStatus.Published:
                switch (next)
                {
                    case LicenseSeatMilestoneStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"license seat milestone"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public LicenseSeatMilestone Copy()
    {
        return new LicenseSeatMilestone(Code, Label, Archived, Pinned, ContactEmail, Segment, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
