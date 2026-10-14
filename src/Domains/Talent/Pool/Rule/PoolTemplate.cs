namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool template is in its life cycle.</summary>
public enum PoolTemplateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pool template. Its code is unique within a tenant.</summary>
public sealed class PoolTemplate(string code, string label, string source, string managerId, bool archived, double amount, PoolTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string ManagerId { get; set; } = managerId;
    public bool Archived { get; set; } = archived;
    public double Amount { get; set; } = amount;
    public PoolTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a pool template in status proposed.</summary>
    public static PoolTemplate NewPoolTemplate(string code, string label)
    {
        return new PoolTemplate(code, label, "", "", false, 0.0, PoolTemplateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolTemplateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolTemplateStatus next)
    {
        switch (Status)
        {
            case PoolTemplateStatus.Proposed:
                return next == PoolTemplateStatus.Approved || next == PoolTemplateStatus.Withdrawn;
            case PoolTemplateStatus.Approved:
                switch (next)
                {
                    case PoolTemplateStatus.Rejected:
                        return true;
                }
                return false;
            case PoolTemplateStatus.Rejected:
                switch (next)
                {
                    case PoolTemplateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pool template"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PoolTemplate Copy()
    {
        return new PoolTemplate(Code, Label, Source, ManagerId, Archived, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
