namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility checkpoint is in its life cycle.</summary>
public enum MobilityCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A mobility checkpoint as the mobility team keeps it.</summary>
public sealed class MobilityCheckpoint(string code, string name, double margin, int leaveDays, string label, string category, double maximum, string currency, MobilityCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public MobilityCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a mobility checkpoint in status open.</summary>
    public static MobilityCheckpoint NewMobilityCheckpoint(string code, string name)
    {
        return new MobilityCheckpoint(code, name, 0.0, 0, "", "", 0.0, "", MobilityCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityCheckpointStatus next)
    {
        switch (Status)
        {
            case MobilityCheckpointStatus.Open:
                return next == MobilityCheckpointStatus.InReview;
            case MobilityCheckpointStatus.InReview:
                switch (next)
                {
                    case MobilityCheckpointStatus.Reopened:
                        return true;
                }
                return false;
            case MobilityCheckpointStatus.Resolved:
                return next == MobilityCheckpointStatus.Reopened;
            case MobilityCheckpointStatus.Reopened:
                return next == MobilityCheckpointStatus.Archived;
        }
        return false;
    }

    public void MoveTo(MobilityCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"mobility checkpoint"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Maximum + Margin, 2);
    }

    public MobilityCheckpoint Copy()
    {
        return new MobilityCheckpoint(Code, Name, Margin, LeaveDays, Label, Category, Maximum, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
