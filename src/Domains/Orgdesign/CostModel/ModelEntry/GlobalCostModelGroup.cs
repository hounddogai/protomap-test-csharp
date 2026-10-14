namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global cost model group is in its life cycle.</summary>
public enum GlobalCostModelGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A global cost model group. Its code is unique within a tenant.</summary>
public sealed class GlobalCostModelGroup(string code, string name, int capacity, string category, int windowDays, int expiresAt, string channel, GlobalCostModelGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public int WindowDays { get; set; } = windowDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public GlobalCostModelGroupStatus Status { get; set; } = status;

    /// <summary>Starts a global cost model group in status idle.</summary>
    public static GlobalCostModelGroup NewGlobalCostModelGroup(string code, string name)
    {
        return new GlobalCostModelGroup(code, name, 0, "", 0, 0, "", GlobalCostModelGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalCostModelGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalCostModelGroupStatus next)
    {
        switch (Status)
        {
            case GlobalCostModelGroupStatus.Idle:
                switch (next)
                {
                    case GlobalCostModelGroupStatus.Warming:
                    case GlobalCostModelGroupStatus.Ready:
                        return true;
                }
                return false;
            case GlobalCostModelGroupStatus.Warming:
                return next == GlobalCostModelGroupStatus.Ready;
            case GlobalCostModelGroupStatus.Ready:
                switch (next)
                {
                    case GlobalCostModelGroupStatus.Draining:
                    case GlobalCostModelGroupStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalCostModelGroupStatus.Draining:
                switch (next)
                {
                    case GlobalCostModelGroupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalCostModelGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global cost model group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global cost model group"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)WindowDays + (double)ExpiresAt, 2);
    }

    public GlobalCostModelGroup Copy()
    {
        return new GlobalCostModelGroup(Code, Name, Capacity, Category, WindowDays, ExpiresAt, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
