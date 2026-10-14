namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor checkpoint is in its life cycle.</summary>
public enum VisitorCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A visitor checkpoint as the visitor team keeps it.</summary>
public sealed class VisitorCheckpoint(string code, string title, string skill, int tenureMonths, int quantity, VisitorCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Quantity { get; set; } = quantity;
    public VisitorCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a visitor checkpoint in status requested.</summary>
    public static VisitorCheckpoint NewVisitorCheckpoint(string code, string title)
    {
        return new VisitorCheckpoint(code, title, "", 0, 0, VisitorCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorCheckpointStatus next)
    {
        switch (Status)
        {
            case VisitorCheckpointStatus.Requested:
                return next == VisitorCheckpointStatus.Granted || next == VisitorCheckpointStatus.Revoked;
            case VisitorCheckpointStatus.Granted:
                return next == VisitorCheckpointStatus.Revoked || next == VisitorCheckpointStatus.Expired;
            case VisitorCheckpointStatus.Revoked:
                return next == VisitorCheckpointStatus.Expired;
        }
        return false;
    }

    public void MoveTo(VisitorCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)TenureMonths + (double)Quantity, 2);
    }

    public VisitorCheckpoint Copy()
    {
        return new VisitorCheckpoint(Code, Title, Skill, TenureMonths, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
