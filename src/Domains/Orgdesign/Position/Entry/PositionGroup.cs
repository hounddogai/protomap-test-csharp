namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position group is in its life cycle.</summary>
public enum PositionGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A position group. Its code is unique within a tenant.</summary>
public sealed class PositionGroup(string code, string label, string notes, int attempts, int revision, double budget, string ownerId, string title, string checksum, PositionGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int Attempts { get; set; } = attempts;
    public int Revision { get; set; } = revision;
    public double Budget { get; set; } = budget;
    public string OwnerId { get; set; } = ownerId;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public PositionGroupStatus Status { get; set; } = status;

    /// <summary>Starts a position group in status requested.</summary>
    public static PositionGroup NewPositionGroup(string code, string label)
    {
        return new PositionGroup(code, label, "", 0, 0, 0.0, "", "", "", PositionGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionGroupStatus next)
    {
        switch (Status)
        {
            case PositionGroupStatus.Requested:
                switch (next)
                {
                    case PositionGroupStatus.Granted:
                    case PositionGroupStatus.Revoked:
                        return true;
                }
                return false;
            case PositionGroupStatus.Granted:
                switch (next)
                {
                    case PositionGroupStatus.Revoked:
                        return true;
                }
                return false;
            case PositionGroupStatus.Revoked:
                return next == PositionGroupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PositionGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"position group"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Attempts + Budget, 2);
    }

    public PositionGroup Copy()
    {
        return new PositionGroup(Code, Label, Notes, Attempts, Revision, Budget, OwnerId, Title, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
