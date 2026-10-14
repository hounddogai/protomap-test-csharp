namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling position bundle is in its life cycle.</summary>
public enum RollingPositionBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One rolling position bundle, with what the planners need to act on it.</summary>
public sealed class RollingPositionBundle(string code, string label, int revision, string reviewer, int quantity, int capacity, int startsAt, int endsAt, string employeeId, double fte, RollingPositionBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string Reviewer { get; set; } = reviewer;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public string EmployeeId { get; set; } = employeeId;
    public double Fte { get; set; } = fte;
    public RollingPositionBundleStatus Status { get; set; } = status;

    /// <summary>Starts a rolling position bundle in status new.</summary>
    public static RollingPositionBundle NewRollingPositionBundle(string code, string label)
    {
        return new RollingPositionBundle(code, label, 0, "", 0, 0, 0, 0, "", 0.0, RollingPositionBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPositionBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPositionBundleStatus next)
    {
        switch (Status)
        {
            case RollingPositionBundleStatus.New:
                switch (next)
                {
                    case RollingPositionBundleStatus.Queued:
                        return true;
                }
                return false;
            case RollingPositionBundleStatus.Queued:
                switch (next)
                {
                    case RollingPositionBundleStatus.Running:
                    case RollingPositionBundleStatus.Failed:
                        return true;
                }
                return false;
            case RollingPositionBundleStatus.Running:
                return next == RollingPositionBundleStatus.Failed;
            case RollingPositionBundleStatus.Done:
                switch (next)
                {
                    case RollingPositionBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPositionBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling position bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Revision - (double)Quantity, 2);
    }

    public RollingPositionBundle Copy()
    {
        return new RollingPositionBundle(
            Code,
            Label,
            Revision,
            Reviewer,
            Quantity,
            Capacity,
            StartsAt,
            EndsAt,
            EmployeeId,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
