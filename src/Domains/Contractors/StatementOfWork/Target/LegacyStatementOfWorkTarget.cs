namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy statement of work target is in its life cycle.</summary>
public enum LegacyStatementOfWorkTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a legacy statement of work target and its current status.</summary>
public sealed class LegacyStatementOfWorkTarget(string code, string name, string externalRef, string team, string managerId, int tenureMonths, double amount, string title, bool visible, LegacyStatementOfWorkTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public string Team { get; set; } = team;
    public string ManagerId { get; set; } = managerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Amount { get; set; } = amount;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public LegacyStatementOfWorkTargetStatus Status { get; set; } = status;

    /// <summary>Starts a legacy statement of work target in status proposed.</summary>
    public static LegacyStatementOfWorkTarget NewLegacyStatementOfWorkTarget(string code, string name)
    {
        return new LegacyStatementOfWorkTarget(
            code,
            name,
            "",
            "",
            "",
            0,
            0.0,
            "",
            false,
            LegacyStatementOfWorkTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyStatementOfWorkTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyStatementOfWorkTargetStatus next)
    {
        switch (Status)
        {
            case LegacyStatementOfWorkTargetStatus.Proposed:
                switch (next)
                {
                    case LegacyStatementOfWorkTargetStatus.Approved:
                    case LegacyStatementOfWorkTargetStatus.Rejected:
                        return true;
                }
                return false;
            case LegacyStatementOfWorkTargetStatus.Approved:
                return next == LegacyStatementOfWorkTargetStatus.Rejected || next == LegacyStatementOfWorkTargetStatus.Withdrawn;
            case LegacyStatementOfWorkTargetStatus.Rejected:
                return next == LegacyStatementOfWorkTargetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyStatementOfWorkTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy statement of work target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round((double)TenureMonths + Amount, 2);
    }

    public LegacyStatementOfWorkTarget Copy()
    {
        return new LegacyStatementOfWorkTarget(
            Code,
            Name,
            ExternalRef,
            Team,
            ManagerId,
            TenureMonths,
            Amount,
            Title,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
