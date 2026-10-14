namespace Acme.Workforce.Orgdesign.CostModel.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model tier is in its life cycle.</summary>
public enum CostModelTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A cost model tier as the cost model team keeps it.</summary>
public sealed class CostModelTier(string code, string name, string channel, double fte, bool approved, CostModelTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public double Fte { get; set; } = fte;
    public bool Approved { get; set; } = approved;
    public CostModelTierStatus Status { get; set; } = status;

    /// <summary>Starts a cost model tier in status open.</summary>
    public static CostModelTier NewCostModelTier(string code, string name)
    {
        return new CostModelTier(code, name, "", 0.0, false, CostModelTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelTierStatus next)
    {
        switch (Status)
        {
            case CostModelTierStatus.Open:
                switch (next)
                {
                    case CostModelTierStatus.Archived:
                        return true;
                }
                return false;
            case CostModelTierStatus.InReview:
                switch (next)
                {
                    case CostModelTierStatus.Reopened:
                    case CostModelTierStatus.Archived:
                        return true;
                }
                return false;
            case CostModelTierStatus.Resolved:
                return next == CostModelTierStatus.Reopened || next == CostModelTierStatus.Archived;
            case CostModelTierStatus.Reopened:
                switch (next)
                {
                    case CostModelTierStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"cost model tier"} {Code}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public CostModelTier Copy()
    {
        return new CostModelTier(Code, Name, Channel, Fte, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
