namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path window is in its life cycle.</summary>
public enum CareerPathWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A career path window. Its code is unique within a tenant.</summary>
public sealed class CareerPathWindow(string code, string title, int priority, string costCenter, string team, int retries, CareerPathWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public string CostCenter { get; set; } = costCenter;
    public string Team { get; set; } = team;
    public int Retries { get; set; } = retries;
    public CareerPathWindowStatus Status { get; set; } = status;

    /// <summary>Starts a career path window in status draft.</summary>
    public static CareerPathWindow NewCareerPathWindow(string code, string title)
    {
        return new CareerPathWindow(code, title, 0, "", "", 0, CareerPathWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathWindowStatus next)
    {
        switch (Status)
        {
            case CareerPathWindowStatus.Draft:
                return next == CareerPathWindowStatus.Pending || next == CareerPathWindowStatus.Closed;
            case CareerPathWindowStatus.Pending:
                switch (next)
                {
                    case CareerPathWindowStatus.Closed:
                        return true;
                }
                return false;
            case CareerPathWindowStatus.Active:
                return next == CareerPathWindowStatus.Suspended;
            case CareerPathWindowStatus.Suspended:
                switch (next)
                {
                    case CareerPathWindowStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Retries - (double)Priority, 2);
    }

    public CareerPathWindow Copy()
    {
        return new CareerPathWindow(Code, Title, Priority, CostCenter, Team, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
