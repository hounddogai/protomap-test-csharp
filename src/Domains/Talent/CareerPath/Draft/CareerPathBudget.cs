namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path budget is in its life cycle.</summary>
public enum CareerPathBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One career path budget, with what the planners need to act on it.</summary>
public sealed class CareerPathBudget(string code, string name, string employeeId, string notes, double rating, int updatedAt, string label, int sequence, CareerPathBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string Notes { get; set; } = notes;
    public double Rating { get; set; } = rating;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public CareerPathBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a career path budget in status idle.</summary>
    public static CareerPathBudget NewCareerPathBudget(string code, string name)
    {
        return new CareerPathBudget(code, name, "", "", 0.0, 0, "", 0, CareerPathBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathBudgetStatus next)
    {
        switch (Status)
        {
            case CareerPathBudgetStatus.Idle:
                return next == CareerPathBudgetStatus.Warming || next == CareerPathBudgetStatus.Ready;
            case CareerPathBudgetStatus.Warming:
                return next == CareerPathBudgetStatus.Ready || next == CareerPathBudgetStatus.Stopped;
            case CareerPathBudgetStatus.Ready:
                switch (next)
                {
                    case CareerPathBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case CareerPathBudgetStatus.Draining:
                return next == CareerPathBudgetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CareerPathBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Rating - (double)Sequence, 2);
    }

    public CareerPathBudget Copy()
    {
        return new CareerPathBudget(Code, Name, EmployeeId, Notes, Rating, UpdatedAt, Label, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
