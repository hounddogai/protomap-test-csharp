namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat adjustment is in its life cycle.</summary>
public enum LicenseSeatAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A license seat adjustment as the license seat team keeps it.</summary>
public sealed class LicenseSeatAdjustment(string code, string name, int startsAt, int limit, string label, double cost, LicenseSeatAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public int Limit { get; set; } = limit;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public LicenseSeatAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a license seat adjustment in status new.</summary>
    public static LicenseSeatAdjustment NewLicenseSeatAdjustment(string code, string name)
    {
        return new LicenseSeatAdjustment(code, name, 0, 0, "", 0.0, LicenseSeatAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatAdjustmentStatus next)
    {
        switch (Status)
        {
            case LicenseSeatAdjustmentStatus.New:
                switch (next)
                {
                    case LicenseSeatAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case LicenseSeatAdjustmentStatus.Queued:
                switch (next)
                {
                    case LicenseSeatAdjustmentStatus.Running:
                    case LicenseSeatAdjustmentStatus.Failed:
                        return true;
                }
                return false;
            case LicenseSeatAdjustmentStatus.Running:
                switch (next)
                {
                    case LicenseSeatAdjustmentStatus.Done:
                        return true;
                }
                return false;
            case LicenseSeatAdjustmentStatus.Done:
                switch (next)
                {
                    case LicenseSeatAdjustmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LicenseSeatAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Limit * Cost, 2);
    }

    public LicenseSeatAdjustment Copy()
    {
        return new LicenseSeatAdjustment(Code, Name, StartsAt, Limit, Label, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
