namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg score is in its life cycle.</summary>
public enum ReorgScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A reorg score. Its code is unique within a tenant.</summary>
public sealed class ReorgScore(string code, string label, double floorValue, int version, string channel, double fte, ReorgScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public int Version { get; set; } = version;
    public string Channel { get; set; } = channel;
    public double Fte { get; set; } = fte;
    public ReorgScoreStatus Status { get; set; } = status;

    /// <summary>Starts a reorg score in status open.</summary>
    public static ReorgScore NewReorgScore(string code, string label)
    {
        return new ReorgScore(code, label, 0.0, 0, "", 0.0, ReorgScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgScoreStatus next)
    {
        switch (Status)
        {
            case ReorgScoreStatus.Open:
                return next == ReorgScoreStatus.Resolved || next == ReorgScoreStatus.Archived;
            case ReorgScoreStatus.InReview:
                switch (next)
                {
                    case ReorgScoreStatus.Archived:
                        return true;
                }
                return false;
            case ReorgScoreStatus.Resolved:
                return next == ReorgScoreStatus.Archived;
            case ReorgScoreStatus.Reopened:
                switch (next)
                {
                    case ReorgScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round(Fte - FloorValue, 2);
    }

    public ReorgScore Copy()
    {
        return new ReorgScore(Code, Label, FloorValue, Version, Channel, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
