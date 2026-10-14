namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction preference is in its life cycle.</summary>
public enum DeductionPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a deduction preference and its current status.</summary>
public sealed class DeductionPreference(string code, string name, string phone, double fte, double maximum, double floorValue, string grade, DeductionPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public double Fte { get; set; } = fte;
    public double Maximum { get; set; } = maximum;
    public double FloorValue { get; set; } = floorValue;
    public string Grade { get; set; } = grade;
    public DeductionPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a deduction preference in status planned.</summary>
    public static DeductionPreference NewDeductionPreference(string code, string name)
    {
        return new DeductionPreference(code, name, "", 0.0, 0.0, 0.0, "", DeductionPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionPreferenceStatus next)
    {
        switch (Status)
        {
            case DeductionPreferenceStatus.Planned:
                return next == DeductionPreferenceStatus.InProgress;
            case DeductionPreferenceStatus.Scheduled:
                switch (next)
                {
                    case DeductionPreferenceStatus.Completed:
                        return true;
                }
                return false;
            case DeductionPreferenceStatus.InProgress:
                return next == DeductionPreferenceStatus.Completed || next == DeductionPreferenceStatus.Cancelled;
            case DeductionPreferenceStatus.Completed:
                switch (next)
                {
                    case DeductionPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Fte * FloorValue, 2);
    }

    public DeductionPreference Copy()
    {
        return new DeductionPreference(Code, Name, Phone, Fte, Maximum, FloorValue, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
