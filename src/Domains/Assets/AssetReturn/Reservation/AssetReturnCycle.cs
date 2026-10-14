namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return cycle is in its life cycle.</summary>
public enum AssetReturnCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One asset return cycle, with what the planners need to act on it.</summary>
public sealed class AssetReturnCycle(string code, string name, int updatedAt, List<string> tags, int revision, string department, AssetReturnCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public List<string> Tags { get; set; } = tags;
    public int Revision { get; set; } = revision;
    public string Department { get; set; } = department;
    public AssetReturnCycleStatus Status { get; set; } = status;

    /// <summary>Starts a asset return cycle in status idle.</summary>
    public static AssetReturnCycle NewAssetReturnCycle(string code, string name)
    {
        return new AssetReturnCycle(code, name, 0, new List<string>(), 0, "", AssetReturnCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnCycleStatus next)
    {
        switch (Status)
        {
            case AssetReturnCycleStatus.Idle:
                switch (next)
                {
                    case AssetReturnCycleStatus.Draining:
                        return true;
                }
                return false;
            case AssetReturnCycleStatus.Warming:
                return next == AssetReturnCycleStatus.Ready || next == AssetReturnCycleStatus.Draining;
            case AssetReturnCycleStatus.Ready:
                switch (next)
                {
                    case AssetReturnCycleStatus.Draining:
                    case AssetReturnCycleStatus.Stopped:
                        return true;
                }
                return false;
            case AssetReturnCycleStatus.Draining:
                return next == AssetReturnCycleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AssetReturnCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt - (double)Revision, 2);
    }

    public AssetReturnCycle Copy()
    {
        return new AssetReturnCycle(Code, Name, UpdatedAt, Tags, Revision, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
