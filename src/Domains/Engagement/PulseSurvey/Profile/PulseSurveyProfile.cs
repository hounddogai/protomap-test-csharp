namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey profile is in its life cycle.</summary>
public enum PulseSurveyProfileStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pulse survey profile and its current status.</summary>
public sealed class PulseSurveyProfile(string code, string name, double rate, double ratio, string segment, string employeeId, string unit, PulseSurveyProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public string Segment { get; set; } = segment;
    public string EmployeeId { get; set; } = employeeId;
    public string Unit { get; set; } = unit;
    public PulseSurveyProfileStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey profile in status created.</summary>
    public static PulseSurveyProfile NewPulseSurveyProfile(string code, string name)
    {
        return new PulseSurveyProfile(code, name, 0.0, 0.0, "", "", "", PulseSurveyProfileStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyProfileStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyProfileStatus next)
    {
        switch (Status)
        {
            case PulseSurveyProfileStatus.Created:
                return next == PulseSurveyProfileStatus.Published;
            case PulseSurveyProfileStatus.Validated:
                return next == PulseSurveyProfileStatus.Published || next == PulseSurveyProfileStatus.Retired;
            case PulseSurveyProfileStatus.Published:
                return next == PulseSurveyProfileStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PulseSurveyProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pulse survey profile"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ratio - Rate, 2);
    }

    public PulseSurveyProfile Copy()
    {
        return new PulseSurveyProfile(Code, Name, Rate, Ratio, Segment, EmployeeId, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
