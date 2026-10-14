namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending equity grant override is in its life cycle.</summary>
public enum PendingEquityGrantOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending equity grant override. Its code is unique within a tenant.</summary>
public sealed class PendingEquityGrantOverride(string code, string name, double discount, string externalRef, string category, PendingEquityGrantOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public string ExternalRef { get; set; } = externalRef;
    public string Category { get; set; } = category;
    public PendingEquityGrantOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a pending equity grant override in status planned.</summary>
    public static PendingEquityGrantOverride NewPendingEquityGrantOverride(string code, string name)
    {
        return new PendingEquityGrantOverride(code, name, 0.0, "", "", PendingEquityGrantOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingEquityGrantOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingEquityGrantOverrideStatus next)
    {
        switch (Status)
        {
            case PendingEquityGrantOverrideStatus.Planned:
                switch (next)
                {
                    case PendingEquityGrantOverrideStatus.InProgress:
                        return true;
                }
                return false;
            case PendingEquityGrantOverrideStatus.Scheduled:
                return next == PendingEquityGrantOverrideStatus.InProgress;
            case PendingEquityGrantOverrideStatus.InProgress:
                return next == PendingEquityGrantOverrideStatus.Completed || next == PendingEquityGrantOverrideStatus.Cancelled;
            case PendingEquityGrantOverrideStatus.Completed:
                switch (next)
                {
                    case PendingEquityGrantOverrideStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingEquityGrantOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending equity grant override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending equity grant override"} {Code}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public PendingEquityGrantOverride Copy()
    {
        return new PendingEquityGrantOverride(Code, Name, Discount, ExternalRef, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
