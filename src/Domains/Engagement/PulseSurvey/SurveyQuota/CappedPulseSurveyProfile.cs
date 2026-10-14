namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pulse survey profile is in its life cycle.</summary>
public enum CappedPulseSurveyProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped pulse survey profile. Its code is unique within a tenant.</summary>
public sealed class CappedPulseSurveyProfile(string code, string title, string checksum, double weight, int revision, bool approved, double floorValue, bool archived, string currency, double maximum, int limit, CappedPulseSurveyProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public double Weight { get; set; } = weight;
    public int Revision { get; set; } = revision;
    public bool Approved { get; set; } = approved;
    public double FloorValue { get; set; } = floorValue;
    public bool Archived { get; set; } = archived;
    public string Currency { get; set; } = currency;
    public double Maximum { get; set; } = maximum;
    public int Limit { get; set; } = limit;
    public CappedPulseSurveyProfileStatus Status { get; set; } = status;

    /// <summary>Starts a capped pulse survey profile in status new.</summary>
    public static CappedPulseSurveyProfile NewCappedPulseSurveyProfile(string code, string title)
    {
        return new CappedPulseSurveyProfile(
            code,
            title,
            "",
            0.0,
            0,
            false,
            0.0,
            false,
            "",
            0.0,
            0,
            CappedPulseSurveyProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPulseSurveyProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPulseSurveyProfileStatus next)
    {
        switch (Status)
        {
            case CappedPulseSurveyProfileStatus.New:
                switch (next)
                {
                    case CappedPulseSurveyProfileStatus.Running:
                    case CappedPulseSurveyProfileStatus.Done:
                        return true;
                }
                return false;
            case CappedPulseSurveyProfileStatus.Queued:
                switch (next)
                {
                    case CappedPulseSurveyProfileStatus.Done:
                        return true;
                }
                return false;
            case CappedPulseSurveyProfileStatus.Running:
                return next == CappedPulseSurveyProfileStatus.Failed;
            case CappedPulseSurveyProfileStatus.Done:
                switch (next)
                {
                    case CappedPulseSurveyProfileStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPulseSurveyProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pulse survey profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"capped pulse survey profile"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Weight * (double)Revision, 2);
    }

    public CappedPulseSurveyProfile Copy()
    {
        return new CappedPulseSurveyProfile(
            Code,
            Title,
            Checksum,
            Weight,
            Revision,
            Approved,
            FloorValue,
            Archived,
            Currency,
            Maximum,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
