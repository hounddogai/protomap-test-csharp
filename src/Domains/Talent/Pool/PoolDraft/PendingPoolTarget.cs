namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pool target is in its life cycle.</summary>
public enum PendingPoolTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending pool target as the pool team keeps it.</summary>
public sealed class PendingPoolTarget(string code, string label, string employeeId, string name, string skill, int updatedAt, string title, string costCenter, int revision, double margin, bool active, PendingPoolTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public int Revision { get; set; } = revision;
    public double Margin { get; set; } = margin;
    public bool Active { get; set; } = active;
    public PendingPoolTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pending pool target in status planned.</summary>
    public static PendingPoolTarget NewPendingPoolTarget(string code, string label)
    {
        return new PendingPoolTarget(
            code,
            label,
            "",
            "",
            "",
            0,
            "",
            "",
            0,
            0.0,
            false,
            PendingPoolTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPoolTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPoolTargetStatus next)
    {
        switch (Status)
        {
            case PendingPoolTargetStatus.Planned:
                switch (next)
                {
                    case PendingPoolTargetStatus.InProgress:
                    case PendingPoolTargetStatus.Completed:
                        return true;
                }
                return false;
            case PendingPoolTargetStatus.Scheduled:
                switch (next)
                {
                    case PendingPoolTargetStatus.InProgress:
                        return true;
                }
                return false;
            case PendingPoolTargetStatus.InProgress:
                switch (next)
                {
                    case PendingPoolTargetStatus.Completed:
                        return true;
                }
                return false;
            case PendingPoolTargetStatus.Completed:
                switch (next)
                {
                    case PendingPoolTargetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPoolTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pool target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Margin * (double)Revision, 2);
    }

    public PendingPoolTarget Copy()
    {
        return new PendingPoolTarget(
            Code,
            Label,
            EmployeeId,
            Name,
            Skill,
            UpdatedAt,
            Title,
            CostCenter,
            Revision,
            Margin,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
