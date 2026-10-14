namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pulse survey record is in its life cycle.</summary>
public enum GlobalPulseSurveyRecordStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A global pulse survey record as the pulse survey team keeps it.</summary>
public sealed class GlobalPulseSurveyRecord(string code, string name, double weight, bool pinned, int endsAt, double ceiling, string summary, GlobalPulseSurveyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public bool Pinned { get; set; } = pinned;
    public int EndsAt { get; set; } = endsAt;
    public double Ceiling { get; set; } = ceiling;
    public string Summary { get; set; } = summary;
    public GlobalPulseSurveyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a global pulse survey record in status created.</summary>
    public static GlobalPulseSurveyRecord NewGlobalPulseSurveyRecord(string code, string name)
    {
        return new GlobalPulseSurveyRecord(
            code,
            name,
            0.0,
            false,
            0,
            0.0,
            "",
            GlobalPulseSurveyRecordStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPulseSurveyRecordStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPulseSurveyRecordStatus next)
    {
        switch (Status)
        {
            case GlobalPulseSurveyRecordStatus.Created:
                switch (next)
                {
                    case GlobalPulseSurveyRecordStatus.Published:
                        return true;
                }
                return false;
            case GlobalPulseSurveyRecordStatus.Validated:
                return next == GlobalPulseSurveyRecordStatus.Published;
            case GlobalPulseSurveyRecordStatus.Published:
                switch (next)
                {
                    case GlobalPulseSurveyRecordStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPulseSurveyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pulse survey record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global pulse survey record"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Weight - Ceiling, 2);
    }

    public GlobalPulseSurveyRecord Copy()
    {
        return new GlobalPulseSurveyRecord(Code, Name, Weight, Pinned, EndsAt, Ceiling, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
