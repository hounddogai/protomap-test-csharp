namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal headcount reservation is in its life cycle.</summary>
public enum SeasonalHeadcountReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal headcount reservation. Its code is unique within a tenant.</summary>
public sealed class SeasonalHeadcountReservation(string code, string title, int retries, string department, double cost, string skill, string phone, SeasonalHeadcountReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string Department { get; set; } = department;
    public double Cost { get; set; } = cost;
    public string Skill { get; set; } = skill;
    public string Phone { get; set; } = phone;
    public SeasonalHeadcountReservationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal headcount reservation in status idle.</summary>
    public static SeasonalHeadcountReservation NewSeasonalHeadcountReservation(string code, string title)
    {
        return new SeasonalHeadcountReservation(
            code,
            title,
            0,
            "",
            0.0,
            "",
            "",
            SeasonalHeadcountReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHeadcountReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHeadcountReservationStatus next)
    {
        switch (Status)
        {
            case SeasonalHeadcountReservationStatus.Idle:
                switch (next)
                {
                    case SeasonalHeadcountReservationStatus.Warming:
                        return true;
                }
                return false;
            case SeasonalHeadcountReservationStatus.Warming:
                return next == SeasonalHeadcountReservationStatus.Ready;
            case SeasonalHeadcountReservationStatus.Ready:
                return next == SeasonalHeadcountReservationStatus.Draining;
            case SeasonalHeadcountReservationStatus.Draining:
                switch (next)
                {
                    case SeasonalHeadcountReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalHeadcountReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal headcount reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetLoad()
    {
        return Math.Round((double)Retries - Cost, 2);
    }

    public SeasonalHeadcountReservation Copy()
    {
        return new SeasonalHeadcountReservation(Code, Title, Retries, Department, Cost, Skill, Phone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
