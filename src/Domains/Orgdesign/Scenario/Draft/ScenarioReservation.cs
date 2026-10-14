namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario reservation is in its life cycle.</summary>
public enum ScenarioReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scenario reservation. Its code is unique within a tenant.</summary>
public sealed class ScenarioReservation(string code, string title, double ratio, string phone, string grade, int leaveDays, ScenarioReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Phone { get; set; } = phone;
    public string Grade { get; set; } = grade;
    public int LeaveDays { get; set; } = leaveDays;
    public ScenarioReservationStatus Status { get; set; } = status;

    /// <summary>Starts a scenario reservation in status idle.</summary>
    public static ScenarioReservation NewScenarioReservation(string code, string title)
    {
        return new ScenarioReservation(code, title, 0.0, "", "", 0, ScenarioReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioReservationStatus next)
    {
        switch (Status)
        {
            case ScenarioReservationStatus.Idle:
                switch (next)
                {
                    case ScenarioReservationStatus.Warming:
                        return true;
                }
                return false;
            case ScenarioReservationStatus.Warming:
                switch (next)
                {
                    case ScenarioReservationStatus.Draining:
                    case ScenarioReservationStatus.Stopped:
                        return true;
                }
                return false;
            case ScenarioReservationStatus.Ready:
                switch (next)
                {
                    case ScenarioReservationStatus.Draining:
                        return true;
                }
                return false;
            case ScenarioReservationStatus.Draining:
                switch (next)
                {
                    case ScenarioReservationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScenarioReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scenario reservation"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Ratio - (double)LeaveDays, 2);
    }

    public ScenarioReservation Copy()
    {
        return new ScenarioReservation(Code, Title, Ratio, Phone, Grade, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
