namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global position rollup is in its life cycle.</summary>
public enum GlobalPositionRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global position rollup. Its code is unique within a tenant.</summary>
public sealed class GlobalPositionRollup(string code, string name, double ratio, string label, int priority, string unit, double amount, string checksum, bool approved, GlobalPositionRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public string Checksum { get; set; } = checksum;
    public bool Approved { get; set; } = approved;
    public GlobalPositionRollupStatus Status { get; set; } = status;

    /// <summary>Starts a global position rollup in status open.</summary>
    public static GlobalPositionRollup NewGlobalPositionRollup(string code, string name)
    {
        return new GlobalPositionRollup(code, name, 0.0, "", 0, "", 0.0, "", false, GlobalPositionRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPositionRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPositionRollupStatus next)
    {
        switch (Status)
        {
            case GlobalPositionRollupStatus.Open:
                switch (next)
                {
                    case GlobalPositionRollupStatus.Archived:
                        return true;
                }
                return false;
            case GlobalPositionRollupStatus.InReview:
                switch (next)
                {
                    case GlobalPositionRollupStatus.Reopened:
                        return true;
                }
                return false;
            case GlobalPositionRollupStatus.Resolved:
                switch (next)
                {
                    case GlobalPositionRollupStatus.Archived:
                        return true;
                }
                return false;
            case GlobalPositionRollupStatus.Reopened:
                return next == GlobalPositionRollupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalPositionRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global position rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Ratio * Amount, 2);
    }

    public GlobalPositionRollup Copy()
    {
        return new GlobalPositionRollup(
            Code,
            Name,
            Ratio,
            Label,
            Priority,
            Unit,
            Amount,
            Checksum,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
