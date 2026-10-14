namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment window is in its life cycle.</summary>
public enum AssignmentWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A assignment window as the assignment team keeps it.</summary>
public sealed class AssignmentWindow(string code, string name, double amount, string department, int updatedAt, bool archived, int capacity, string employeeId, string locale, AssignmentWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Department { get; set; } = department;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Archived { get; set; } = archived;
    public int Capacity { get; set; } = capacity;
    public string EmployeeId { get; set; } = employeeId;
    public string Locale { get; set; } = locale;
    public AssignmentWindowStatus Status { get; set; } = status;

    /// <summary>Starts a assignment window in status new.</summary>
    public static AssignmentWindow NewAssignmentWindow(string code, string name)
    {
        return new AssignmentWindow(code, name, 0.0, "", 0, false, 0, "", "", AssignmentWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentWindowStatus next)
    {
        switch (Status)
        {
            case AssignmentWindowStatus.New:
                switch (next)
                {
                    case AssignmentWindowStatus.Failed:
                        return true;
                }
                return false;
            case AssignmentWindowStatus.Queued:
                return next == AssignmentWindowStatus.Running;
            case AssignmentWindowStatus.Running:
                switch (next)
                {
                    case AssignmentWindowStatus.Done:
                    case AssignmentWindowStatus.Failed:
                        return true;
                }
                return false;
            case AssignmentWindowStatus.Done:
                return next == AssignmentWindowStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AssignmentWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Amount - (double)Capacity, 2);
    }

    public AssignmentWindow Copy()
    {
        return new AssignmentWindow(
            Code,
            Name,
            Amount,
            Department,
            UpdatedAt,
            Archived,
            Capacity,
            EmployeeId,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
