namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey slot is in its life cycle.</summary>
public enum PulseSurveySlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pulse survey slot as the pulse survey team keeps it.</summary>
public sealed class PulseSurveySlot(string code, string name, double minimum, string summary, int attempts, string salaryBand, double fte, string phone, double floorValue, PulseSurveySlotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Summary { get; set; } = summary;
    public int Attempts { get; set; } = attempts;
    public string SalaryBand { get; set; } = salaryBand;
    public double Fte { get; set; } = fte;
    public string Phone { get; set; } = phone;
    public double FloorValue { get; set; } = floorValue;
    public PulseSurveySlotStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey slot in status new.</summary>
    public static PulseSurveySlot NewPulseSurveySlot(string code, string name)
    {
        return new PulseSurveySlot(code, name, 0.0, "", 0, "", 0.0, "", 0.0, PulseSurveySlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveySlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveySlotStatus next)
    {
        switch (Status)
        {
            case PulseSurveySlotStatus.New:
                switch (next)
                {
                    case PulseSurveySlotStatus.Done:
                        return true;
                }
                return false;
            case PulseSurveySlotStatus.Queued:
                return next == PulseSurveySlotStatus.Running || next == PulseSurveySlotStatus.Done;
            case PulseSurveySlotStatus.Running:
                switch (next)
                {
                    case PulseSurveySlotStatus.Done:
                    case PulseSurveySlotStatus.Failed:
                        return true;
                }
                return false;
            case PulseSurveySlotStatus.Done:
                return next == PulseSurveySlotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PulseSurveySlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pulse survey slot"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round(Minimum + Fte, 2);
    }

    public PulseSurveySlot Copy()
    {
        return new PulseSurveySlot(
            Code,
            Name,
            Minimum,
            Summary,
            Attempts,
            SalaryBand,
            Fte,
            Phone,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
