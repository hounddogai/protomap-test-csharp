namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan checkpoint is in its life cycle.</summary>
public enum ReturnPlanCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A return plan checkpoint as the return plan team keeps it.</summary>
public sealed class ReturnPlanCheckpoint(string code, string title, string managerId, string name, int quantity, int updatedAt, string phone, ReturnPlanCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Phone { get; set; } = phone;
    public ReturnPlanCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a return plan checkpoint in status proposed.</summary>
    public static ReturnPlanCheckpoint NewReturnPlanCheckpoint(string code, string title)
    {
        return new ReturnPlanCheckpoint(code, title, "", "", 0, 0, "", ReturnPlanCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanCheckpointStatus next)
    {
        switch (Status)
        {
            case ReturnPlanCheckpointStatus.Proposed:
                return next == ReturnPlanCheckpointStatus.Approved;
            case ReturnPlanCheckpointStatus.Approved:
                switch (next)
                {
                    case ReturnPlanCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
            case ReturnPlanCheckpointStatus.Rejected:
                switch (next)
                {
                    case ReturnPlanCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)UpdatedAt + (double)Quantity, 2);
    }

    public ReturnPlanCheckpoint Copy()
    {
        return new ReturnPlanCheckpoint(Code, Title, ManagerId, Name, Quantity, UpdatedAt, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
